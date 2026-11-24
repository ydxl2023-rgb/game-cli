using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Play Mode only: resolve a type from loaded assemblies, pick a method by name + arguments, then <see cref="MethodInfo.Invoke"/>.
    /// Static methods use <c>Invoke(null, args)</c>. Instance methods require exactly one live <see cref="UnityEngine.Object"/> of that type in loaded scenes.
    /// </summary>
    internal static class ReflectionCallInvoker
    {
        /// <summary>
        /// Resolves and invokes a method requested by the CLI.
        /// </summary>
        /// <param name="invokeTypeName">The CLR type name that owns the method.</param>
        /// <param name="invokeMemberName">The method name to invoke.</param>
        /// <param name="argsJson">Optional JSON array of method arguments.</param>
        /// <param name="invokeArgStrings">Optional positional argument tokens from the CLI.</param>
        /// <returns>The serialized invocation result.</returns>
        public static CallInvocationData Invoke(string invokeTypeName, string invokeMemberName, string argsJson, string[] invokeArgStrings)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("UnityCLI call is only allowed while the Editor is in Play Mode.");
            }

            if (string.IsNullOrWhiteSpace(invokeTypeName))
            {
                throw new ArgumentException("invokeType is required (full CLR type name, e.g. App or MyGame.PlayerController).");
            }

            if (string.IsNullOrWhiteSpace(invokeMemberName))
            {
                throw new ArgumentException("invokeMember is required (method name).");
            }

            var declaringType = ResolveTypeFromLoadedAssemblies(invokeTypeName.Trim());
            if (declaringType == null)
            {
                throw new InvalidOperationException(
                    "Type not found in loaded assemblies: \"" + invokeTypeName + "\". Use a namespace-qualified or assembly-qualified name.");
            }

            var parsedArgs = BuildParsedArguments(argsJson, invokeArgStrings);
            var memberName = invokeMemberName.Trim();

            var method = SelectMethod(declaringType, memberName, requireStatic: true, parsedArgs);
            object instanceTarget = null;

            if (method == null)
            {
                instanceTarget = ResolveSinglePlayModeInstance(declaringType);
                method = SelectMethod(declaringType, memberName, requireStatic: false, parsedArgs);
            }

            if (method == null)
            {
                throw new InvalidOperationException(
                    "No matching method \"" + memberName + "\" on " + declaringType.FullName + " for the given arguments. "
                    + "Use a static method, or ensure there is exactly one instance of that type in loaded Play Mode scenes.");
            }

            if (!method.IsStatic && instanceTarget == null)
            {
                throw new InvalidOperationException(
                    "Instance method \"" + memberName + "\" requires exactly one active object of type " + declaringType.FullName + " in loaded scenes.");
            }

            var invokeArgs = BuildInvokeArguments(method, parsedArgs);
            object returnValue;
            try
            {
                returnValue = method.Invoke(instanceTarget, invokeArgs);
            }
            catch (TargetInvocationException exception)
            {
                throw new InvalidOperationException(exception.InnerException?.Message ?? exception.Message, exception.InnerException);
            }

            return BuildResult(invokeTypeName.Trim(), invokeMemberName.Trim(), method, returnValue);
        }

        /// <summary>
        /// Resolves a type by checking Type.GetType and every loaded assembly.
        /// </summary>
        /// <param name="typeName">The full or assembly-qualified CLR type name.</param>
        /// <returns>The resolved type, or null when no loaded assembly contains it.</returns>
        static Type ResolveTypeFromLoadedAssemblies(string typeName)
        {
            var direct = Type.GetType(typeName, false, false);
            if (direct != null)
            {
                return direct;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                Type found = null;
                try
                {
                    found = assembly.GetType(typeName, false, false);
                }
                catch
                {
                    continue;
                }

                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the only live scene instance of a UnityEngine.Object type in Play Mode.
        /// </summary>
        /// <param name="declaringType">The Unity object type that owns an instance method.</param>
        /// <returns>The single matching scene object, or null when none exists.</returns>
        static UnityEngine.Object ResolveSinglePlayModeInstance(Type declaringType)
        {
            if (!typeof(UnityEngine.Object).IsAssignableFrom(declaringType))
            {
                return null;
            }

            var findMethod = typeof(UnityEngine.Object).GetMethod(
                "FindObjectsOfType",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(Type), typeof(bool) },
                null);
            if (findMethod == null)
            {
                return null;
            }

            var raw = findMethod.Invoke(null, new object[] { declaringType, true }) as UnityEngine.Object[];
            if (raw == null || raw.Length == 0)
            {
                return null;
            }

            var eligible = new List<UnityEngine.Object>();
            for (var index = 0; index < raw.Length; index++)
            {
                var candidate = raw[index];
                if (candidate == null || !declaringType.IsInstanceOfType(candidate))
                {
                    continue;
                }

                if (candidate is Component component)
                {
                    if (!component.gameObject.scene.IsValid())
                    {
                        continue;
                    }
                }
                else if (candidate is GameObject gameObject)
                {
                    if (!gameObject.scene.IsValid())
                    {
                        continue;
                    }
                }
                else if (candidate is ScriptableObject)
                {
                    continue;
                }

                eligible.Add(candidate);
            }

            if (eligible.Count == 0)
            {
                return null;
            }

            if (eligible.Count > 1)
            {
                throw new InvalidOperationException(
                    "Multiple Play Mode instances of " + declaringType.FullName + " (" + eligible.Count + "). Remove extras or call a static method.");
            }

            return eligible[0];
        }

        /// <summary>
        /// Selects an overload whose parameter list can accept the parsed CLI arguments.
        /// </summary>
        /// <param name="declaringType">The type to search.</param>
        /// <param name="memberName">The method name to match.</param>
        /// <param name="requireStatic">Whether to restrict matching to static methods.</param>
        /// <param name="parsedArgs">The arguments already parsed from CLI input.</param>
        /// <returns>The best compatible method, or null when no overload matches.</returns>
        static MethodInfo SelectMethod(Type declaringType, string memberName, bool requireStatic, IReadOnlyList<object> parsedArgs)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
            var candidates = new List<MethodInfo>();
            for (var type = declaringType; type != null && type != typeof(object); type = type.BaseType)
            {
                foreach (var method in type.GetMethods(flags))
                {
                    if (!string.Equals(method.Name, memberName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (method.IsSpecialName || method.ContainsGenericParameters)
                    {
                        continue;
                    }

                    if (requireStatic ? !method.IsStatic : method.IsStatic)
                    {
                        continue;
                    }

                    candidates.Add(method);
                }
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            var ordered = candidates
                .OrderBy(m => Math.Abs(m.GetParameters().Length - parsedArgs.Count))
                .ThenBy(m => m.GetParameters().Count(p => !p.HasDefaultValue))
                .ThenBy(m => m.GetParameters().Length)
                .ToList();

            foreach (var method in ordered)
            {
                if (TryBuildInvokeArguments(method, parsedArgs, out _))
                {
                    return method;
                }
            }

            return null;
        }

        /// <summary>
        /// Builds the argument list from explicit tokens or a JSON argument array.
        /// </summary>
        /// <param name="argsJson">The JSON argument array supplied by the CLI.</param>
        /// <param name="invokeArgStrings">The tokenized positional arguments supplied by the CLI.</param>
        /// <returns>The parsed argument values.</returns>
        static List<object> BuildParsedArguments(string argsJson, string[] invokeArgStrings)
        {
            if (invokeArgStrings != null && invokeArgStrings.Length > 0)
            {
                return ParseArgTokens(invokeArgStrings);
            }

            return JsonLiteralArrayParser.Parse(string.IsNullOrWhiteSpace(argsJson) ? "[]" : argsJson.Trim());
        }

        /// <summary>
        /// Parses positional CLI argument tokens into primitive literal values.
        /// </summary>
        /// <param name="invokeArgStrings">The raw CLI argument tokens.</param>
        /// <returns>The parsed argument values.</returns>
        static List<object> ParseArgTokens(string[] invokeArgStrings)
        {
            var list = new List<object>(invokeArgStrings.Length);
            foreach (var token in invokeArgStrings)
            {
                list.Add(ParseArgToken(token));
            }

            return list;
        }

        /// <summary>
        /// Parses one CLI argument token into null, bool, number, or string.
        /// </summary>
        /// <param name="token">The raw token text.</param>
        /// <returns>The parsed literal value.</returns>
        static object ParseArgToken(string token)
        {
            if (token == null)
            {
                return string.Empty;
            }

            var trimmed = token.Trim();
            if (trimmed.Length == 0)
            {
                return string.Empty;
            }

            if (string.Equals(trimmed, "null", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(trimmed, "false", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (long.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var integral))
            {
                if (integral >= int.MinValue && integral <= int.MaxValue)
                {
                    return (double)(int)integral;
                }

                return (double)integral;
            }

            if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var floating))
            {
                return floating;
            }

            return token;
        }

        /// <summary>
        /// Builds the bridge DTO returned after a successful method invocation.
        /// </summary>
        /// <param name="typeName">The type name requested by the CLI.</param>
        /// <param name="memberName">The member name requested by the CLI.</param>
        /// <param name="method">The resolved method that was invoked.</param>
        /// <param name="returnValue">The raw method return value.</param>
        /// <returns>The serializable call result data.</returns>
        static CallInvocationData BuildResult(string typeName, string memberName, MethodInfo method, object returnValue)
        {
            return new CallInvocationData
            {
                invokeType = typeName,
                invokeMember = memberName,
                resolvedMember = method.Name,
                returnIsVoid = method.ReturnType == typeof(void),
                returnType = method.ReturnType == typeof(void) ? "System.Void" : GetFriendlyTypeName(method.ReturnType),
                returnSummary = method.ReturnType == typeof(void) ? string.Empty : FormatReturnValue(returnValue),
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Formats a method return value into a short CLI-friendly string.
        /// </summary>
        /// <param name="value">The raw return value.</param>
        /// <returns>A readable return-value summary.</returns>
        static string FormatReturnValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is UnityEngine.Object unityObject)
            {
                return unityObject.GetType().Name + " instanceId=" + unityObject.GetInstanceID();
            }

            if (value is IFormattable formattable && value is not string)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture) ?? string.Empty;
            }

            return Convert.ToString(value, CultureInfo.InvariantCulture) ?? value.GetType().FullName ?? string.Empty;
        }

        /// <summary>
        /// Gets a readable CLR type name for command output.
        /// </summary>
        /// <param name="type">The CLR type to describe.</param>
        /// <returns>The full type name when available.</returns>
        static string GetFriendlyTypeName(Type type)
        {
            return type.FullName ?? type.Name;
        }

        /// <summary>
        /// Tries to coerce parsed CLI arguments into the method parameter types.
        /// </summary>
        /// <param name="method">The candidate method overload.</param>
        /// <param name="parsedArgs">The parsed CLI argument values.</param>
        /// <param name="invokeArgs">The coerced argument array, when matching succeeds.</param>
        /// <returns>True when the arguments are compatible with the method.</returns>
        static bool TryBuildInvokeArguments(MethodInfo method, IReadOnlyList<object> parsedArgs, out object[] invokeArgs)
        {
            invokeArgs = null;
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                if (parsedArgs.Count == 0)
                {
                    invokeArgs = Array.Empty<object>();
                    return true;
                }

                return false;
            }

            var last = parameters[parameters.Length - 1];
            if (last.ParameterType.IsArray && last.IsDefined(typeof(ParamArrayAttribute), false))
            {
                return TryBuildParamsArguments(method, parsedArgs, out invokeArgs);
            }

            if (parsedArgs.Count > parameters.Length)
            {
                return false;
            }

            invokeArgs = new object[parameters.Length];
            for (var index = 0; index < parameters.Length; index++)
            {
                var parameter = parameters[index];
                if (index < parsedArgs.Count)
                {
                    if (!TryCoerce(parsedArgs[index], parameter.ParameterType, out invokeArgs[index]))
                    {
                        return false;
                    }
                }
                else if (parameter.HasDefaultValue)
                {
                    var defaultValue = parameter.DefaultValue;
                    invokeArgs[index] = ReferenceEquals(defaultValue, DBNull.Value) ? null : defaultValue;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to build arguments for a method that ends with a params array.
        /// </summary>
        /// <param name="method">The candidate method overload.</param>
        /// <param name="parsedArgs">The parsed CLI argument values.</param>
        /// <param name="invokeArgs">The coerced argument array, when matching succeeds.</param>
        /// <returns>True when the arguments are compatible with the params overload.</returns>
        static bool TryBuildParamsArguments(MethodInfo method, IReadOnlyList<object> parsedArgs, out object[] invokeArgs)
        {
            invokeArgs = null;
            var parameters = method.GetParameters();
            var requiredCount = parameters.Length - 1;
            if (parsedArgs.Count < requiredCount)
            {
                return false;
            }

            invokeArgs = new object[parameters.Length];
            for (var index = 0; index < requiredCount; index++)
            {
                if (!TryCoerce(parsedArgs[index], parameters[index].ParameterType, out invokeArgs[index]))
                {
                    return false;
                }
            }

            var paramElementType = parameters[parameters.Length - 1].ParameterType.GetElementType();
            if (paramElementType == null)
            {
                return false;
            }

            var rest = parsedArgs.Count - requiredCount;
            var paramsArray = Array.CreateInstance(paramElementType, rest);
            for (var index = 0; index < rest; index++)
            {
                if (!TryCoerce(parsedArgs[requiredCount + index], paramElementType, out var element))
                {
                    return false;
                }

                paramsArray.SetValue(element, index);
            }

            invokeArgs[parameters.Length - 1] = paramsArray;
            return true;
        }

        /// <summary>
        /// Builds the final invocation argument array or throws a detailed mismatch error.
        /// </summary>
        /// <param name="method">The selected method overload.</param>
        /// <param name="parsedArgs">The parsed CLI argument values.</param>
        /// <returns>The coerced argument array passed to MethodInfo.Invoke.</returns>
        static object[] BuildInvokeArguments(MethodInfo method, IReadOnlyList<object> parsedArgs)
        {
            if (!TryBuildInvokeArguments(method, parsedArgs, out var args))
            {
                var parameters = method.GetParameters();
                throw new InvalidOperationException(
                    "Arguments do not match any overload of " + method.DeclaringType?.FullName + "." + method.Name
                    + " (expected arity compatible with " + parameters.Length + " parameters; got " + parsedArgs.Count + ").");
            }

            return args;
        }

        /// <summary>
        /// Tries to convert a parsed literal into the requested CLR parameter type.
        /// </summary>
        /// <param name="raw">The parsed literal value.</param>
        /// <param name="targetType">The target CLR type.</param>
        /// <param name="coerced">The converted value, when conversion succeeds.</param>
        /// <returns>True when the value can be passed to the target parameter.</returns>
        static bool TryCoerce(object raw, Type targetType, out object coerced)
        {
            coerced = null;
            if (targetType == null)
            {
                return false;
            }

            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (raw == null)
            {
                if (!underlying.IsValueType || Nullable.GetUnderlyingType(targetType) != null)
                {
                    coerced = null;
                    return true;
                }

                return false;
            }

            if (underlying.IsInstanceOfType(raw))
            {
                coerced = raw;
                return true;
            }

            if (raw is double dbl)
            {
                if (underlying.IsEnum)
                {
                    try
                    {
                        coerced = Enum.ToObject(underlying, (long)Math.Round(dbl));
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (underlying.IsPrimitive && underlying != typeof(char))
                {
                    try
                    {
                        coerced = Convert.ChangeType(dbl, underlying, CultureInfo.InvariantCulture);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            if (raw is string str)
            {
                if (underlying == typeof(string))
                {
                    coerced = str;
                    return true;
                }

                if (underlying.IsEnum)
                {
                    try
                    {
                        coerced = Enum.Parse(underlying, str, true);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                try
                {
                    coerced = Convert.ChangeType(str, underlying, CultureInfo.InvariantCulture);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(underlying))
            {
                int id;
                if (raw is double dd)
                {
                    id = (int)dd;
                }
                else if (raw is long ll)
                {
                    id = (int)ll;
                }
                else
                {
                    return false;
                }

                var obj = EditorUtility.InstanceIDToObject(id);
                if (obj == null && id != 0)
                {
                    return false;
                }

                if (obj != null && !underlying.IsInstanceOfType(obj))
                {
                    return false;
                }

                coerced = obj;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Represents a Call Invocation Data class.
        /// </summary>
        internal sealed class CallInvocationData
        {
            public string invokeType = string.Empty;
            public string invokeMember = string.Empty;
            public string resolvedMember = string.Empty;
            public bool returnIsVoid;
            public string returnType = string.Empty;
            public string returnSummary = string.Empty;
            public string respondedAtUtc = string.Empty;
        }

        /// <summary>
        /// Provides Json Literal Array Parser operations.
        /// </summary>
        static class JsonLiteralArrayParser
        {
            public static List<object> Parse(string json)
            {
                var index = 0;
                SkipWs(json, ref index);
                if (index >= json.Length || json[index] != '[')
                {
                    throw new InvalidOperationException("args must be a JSON array, e.g. [] or [1,\"a\",true].");
                }

                index++;
                var list = new List<object>();
                SkipWs(json, ref index);
                if (index < json.Length && json[index] == ']')
                {
                    index++;
                    return list;
                }

                while (true)
                {
                    list.Add(ParseValue(json, ref index));
                    SkipWs(json, ref index);
                    if (index >= json.Length)
                    {
                        throw new InvalidOperationException("Unterminated JSON array.");
                    }

                    if (json[index] == ']')
                    {
                        break;
                    }

                    if (json[index] != ',')
                    {
                        throw new InvalidOperationException("Expected ',' or ']' in JSON array.");
                    }

                    index++;
                    SkipWs(json, ref index);
                }

                index++;
                SkipWs(json, ref index);
                if (index < json.Length)
                {
                    throw new InvalidOperationException("Unexpected trailing content in JSON array.");
                }

                return list;
            }

            static void SkipWs(string json, ref int index)
            {
                while (index < json.Length && char.IsWhiteSpace(json[index]))
                {
                    index++;
                }
            }

            static object ParseValue(string json, ref int index)
            {
                SkipWs(json, ref index);
                if (index >= json.Length)
                {
                    throw new InvalidOperationException("Unexpected end of JSON.");
                }

                var c = json[index];
                if (c == '"')
                {
                    return ParseString(json, ref index);
                }

                if (c == '-' || char.IsDigit(c))
                {
                    return ParseNumber(json, ref index);
                }

                if (StartsWithLiteral(json, index, "null"))
                {
                    index += 4;
                    return null;
                }

                if (StartsWithLiteral(json, index, "true"))
                {
                    index += 4;
                    return true;
                }

                if (StartsWithLiteral(json, index, "false"))
                {
                    index += 5;
                    return false;
                }

                throw new InvalidOperationException("Unsupported JSON value at position " + index + ".");
            }

            static bool StartsWithLiteral(string json, int index, string literal)
            {
                return index + literal.Length <= json.Length && string.CompareOrdinal(json, index, literal, 0, literal.Length) == 0;
            }

            static string ParseString(string json, ref int index)
            {
                if (json[index] != '"')
                {
                    throw new InvalidOperationException("Expected string.");
                }

                index++;
                var builder = new System.Text.StringBuilder();
                while (index < json.Length)
                {
                    var c = json[index];
                    if (c == '"')
                    {
                        index++;
                        return builder.ToString();
                    }

                    if (c == '\\')
                    {
                        index++;
                        if (index >= json.Length)
                        {
                            throw new InvalidOperationException("Unterminated escape in string.");
                        }

                        var e = json[index++];
                        switch (e)
                        {
                            case '"':
                                builder.Append('"');
                                break;
                            case '\\':
                                builder.Append('\\');
                                break;
                            case '/':
                                builder.Append('/');
                                break;
                            case 'b':
                                builder.Append('\b');
                                break;
                            case 'f':
                                builder.Append('\f');
                                break;
                            case 'n':
                                builder.Append('\n');
                                break;
                            case 'r':
                                builder.Append('\r');
                                break;
                            case 't':
                                builder.Append('\t');
                                break;
                            default:
                                throw new InvalidOperationException("Unsupported escape \\" + e + " in JSON string.");
                        }

                        continue;
                    }

                    builder.Append(c);
                    index++;
                }

                throw new InvalidOperationException("Unterminated JSON string.");
            }

            static double ParseNumber(string json, ref int index)
            {
                var start = index;
                if (json[index] == '-')
                {
                    index++;
                }

                while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '.' || json[index] == 'e' || json[index] == 'E' || json[index] == '+' || json[index] == '-'))
                {
                    index++;
                }

                var slice = json.Substring(start, index - start);
                if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                {
                    throw new InvalidOperationException("Invalid JSON number: " + slice);
                }

                return value;
            }
        }
    }
}
