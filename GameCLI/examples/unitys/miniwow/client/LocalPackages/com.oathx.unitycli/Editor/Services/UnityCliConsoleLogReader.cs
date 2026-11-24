using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Reads Unity Console entries through UnityEditor internal reflection APIs.
    /// </summary>
    internal static class UnityCliConsoleLogReader
    {
        /// <summary>
        /// Default number of console entries returned when no limit is supplied.
        /// </summary>
        const int DefaultCount = 20;
        /// <summary>
        /// Hard cap for console entries returned over the CLI bridge.
        /// </summary>
        const int MaxCount = 200;

        /// <summary>
        /// Reads Unity Console entries with optional type and text filtering.
        /// </summary>
        /// <param name="count">The requested maximum number of entries to return.</param>
        /// <param name="type">The requested entry type: all, log, warning, or error.</param>
        /// <param name="search">The optional text or wildcard search pattern.</param>
        /// <returns>
        /// 序列化的 Console 数据。totalCount 统计命中 type/search 过滤的条目数（不受返回上限影响）；
        /// totalEntries 为过滤前的 Console 原始总条目数。
        /// </returns>
        public static ConsoleData Read(int count, string type, string search)
        {
            count = count <= 0 ? DefaultCount : Math.Min(count, MaxCount);
            type = NormalizeType(type);
            search = search ?? string.Empty;

            var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
            var logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
            if (logEntriesType == null || logEntryType == null)
            {
                throw new InvalidOperationException("Unity console reflection types are unavailable.");
            }

            var getCount = logEntriesType.GetMethod("GetCount", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var getEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (getCount == null || getEntry == null)
            {
                throw new InvalidOperationException("Unity console reflection methods are unavailable.");
            }

            startGettingEntries?.Invoke(null, null);
            try
            {
                var total = Convert.ToInt32(getCount.Invoke(null, null), CultureInfo.InvariantCulture);
                var logs = new List<ConsoleLogEntry>();
                int matched = 0;

                // 必须扫完全部条目：matched 才能在返回条数截断后仍然精确；
                // 迭代顺序为新到旧，因此只保留最先命中的 count 条（即最新的）。
                for (var index = total - 1; index >= 0; index--)
                {
                    var entry = Activator.CreateInstance(logEntryType);
                    getEntry.Invoke(null, new[] { (object)index, entry });

                    var log = new ConsoleLogEntry
                    {
                        message = ReadStringAny(logEntryType, entry, "condition", "message"),
                        stackTrace = ReadStringAny(logEntryType, entry, "stackTrace", "stacktrace"),
                        file = ReadStringAny(logEntryType, entry, "file"),
                        line = ReadInt(logEntryType, entry, "line"),
                        mode = ReadInt(logEntryType, entry, "mode"),
                        instanceId = ReadInt(logEntryType, entry, "instanceID")
                    };
                    log.type = ClassifyType(log.mode);

                    if ((type == "all" || string.Equals(type, log.type, StringComparison.Ordinal))
                        && MatchesSearch(log, search))
                    {
                        matched++;
                        if (logs.Count < count)
                        {
                            logs.Add(log);
                        }
                    }
                }

                logs.Reverse();
                return new ConsoleData
                {
                    totalCount = matched,
                    totalEntries = total,
                    returnedCount = logs.Count,
                    type = type,
                    search = search,
                    respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                    entries = logs.ToArray()
                };
            }
            finally
            {
                endGettingEntries?.Invoke(null, null);
            }
        }

        /// <summary>
        /// Normalizes a requested console type filter.
        /// </summary>
        /// <param name="type">The raw type value from the CLI request.</param>
        /// <returns>A supported type filter, defaulting to all.</returns>
        static string NormalizeType(string type)
        {
            if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, "warning", StringComparison.OrdinalIgnoreCase)
                || string.Equals(type, "log", StringComparison.OrdinalIgnoreCase))
            {
                return type.ToLowerInvariant();
            }

            return "all";
        }

        /// <summary>
        /// Maps Unity's internal log mode flags to a public log type name.
        /// </summary>
        /// <param name="mode">The Unity internal LogEntry mode bit field.</param>
        /// <returns>The classified log type.</returns>
        static string ClassifyType(int mode)
        {
            // Low bits cover runtime severities (warning = bit 9, error/assert/exception = bits 0,1,4,6,8).
            // Compiler diagnostics (CS errors/warnings) use different bits on Unity 2022.3:
            // error = bit 11, warning = bit 12 (sampled from real entries on 2022.3.62f2).
            if ((mode & (1 | 2 | 16 | 64 | 256 | (1 << 11))) != 0)
            {
                return "error";
            }

            if ((mode & (128 | 512 | (1 << 12))) != 0)
            {
                return "warning";
            }

            return "log";
        }

        /// <summary>
        /// Checks whether a console entry matches the requested search pattern.
        /// </summary>
        /// <param name="log">The console log entry to test.</param>
        /// <param name="search">The optional text or wildcard search pattern.</param>
        /// <returns>True when the entry matches the search pattern.</returns>
        static bool MatchesSearch(ConsoleLogEntry log, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return true;
            }

            var value = (log.message ?? string.Empty)
                + "\n" + (log.stackTrace ?? string.Empty)
                + "\n" + (log.file ?? string.Empty);

            if (search.IndexOf('*') < 0 && search.IndexOf('?') < 0)
            {
                return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            var pattern = search;
            if (!pattern.StartsWith("*", StringComparison.Ordinal))
            {
                pattern = "*" + pattern;
            }

            if (!pattern.EndsWith("*", StringComparison.Ordinal))
            {
                pattern += "*";
            }

            return WildcardMatch(value, pattern);
        }

        /// <summary>
        /// Performs case-insensitive wildcard matching with * and ? tokens.
        /// </summary>
        /// <param name="value">The text to inspect.</param>
        /// <param name="pattern">The wildcard pattern.</param>
        /// <returns>True when the value matches the wildcard pattern.</returns>
        static bool WildcardMatch(string value, string pattern)
        {
            value = value ?? string.Empty;
            pattern = pattern ?? string.Empty;

            var valueIndex = 0;
            var patternIndex = 0;
            var starIndex = -1;
            var matchIndex = 0;

            while (valueIndex < value.Length)
            {
                if (patternIndex < pattern.Length
                    && (pattern[patternIndex] == '?'
                        || char.ToUpperInvariant(pattern[patternIndex]) == char.ToUpperInvariant(value[valueIndex])))
                {
                    patternIndex++;
                    valueIndex++;
                    continue;
                }

                if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
                {
                    starIndex = patternIndex++;
                    matchIndex = valueIndex;
                    continue;
                }

                if (starIndex != -1)
                {
                    patternIndex = starIndex + 1;
                    valueIndex = ++matchIndex;
                    continue;
                }

                return false;
            }

            while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex == pattern.Length;
        }

        /// <summary>
        /// Reads the first non-empty string from any of the named fields or properties.
        /// </summary>
        /// <param name="type">The reflected Unity log entry type.</param>
        /// <param name="instance">The log entry instance to inspect.</param>
        /// <param name="names">Candidate field or property names in priority order.</param>
        /// <returns>The first non-empty string value, or an empty string.</returns>
        static string ReadStringAny(Type type, object instance, params string[] names)
        {
            foreach (var name in names)
            {
                var value = ReadString(type, instance, name);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads a string field or property through reflection.
        /// </summary>
        /// <param name="type">The reflected Unity log entry type.</param>
        /// <param name="instance">The log entry instance to inspect.</param>
        /// <param name="name">The field or property name.</param>
        /// <returns>The reflected string value, or an empty string.</returns>
        static string ReadString(Type type, object instance, string name)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return Convert.ToString(field.GetValue(instance), CultureInfo.InvariantCulture);
            }

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return Convert.ToString(property.GetValue(instance, null), CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads an integer field or property through reflection.
        /// </summary>
        /// <param name="type">The reflected Unity log entry type.</param>
        /// <param name="instance">The log entry instance to inspect.</param>
        /// <param name="name">The field or property name.</param>
        /// <returns>The reflected integer value, or zero.</returns>
        static int ReadInt(Type type, object instance, string name)
        {
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return Convert.ToInt32(field.GetValue(instance), CultureInfo.InvariantCulture);
            }

            var property = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.GetIndexParameters().Length == 0)
            {
                return Convert.ToInt32(property.GetValue(instance, null), CultureInfo.InvariantCulture);
            }

            return 0;
        }
    }
}
