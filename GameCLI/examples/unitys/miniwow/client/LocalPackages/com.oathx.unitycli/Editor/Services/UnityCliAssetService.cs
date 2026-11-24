using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Backend for <c>POST /asset</c>. Dispatches to one of six AssetDatabase actions:
    /// <c>search / create-folder / import / delete / move / copy</c>.
    ///
    /// All asset paths are validated to live under <c>Assets/</c> or <c>Packages/</c>.
    /// AssetDatabase rejects anything else, but we surface a friendlier error before
    /// the call.
    /// </summary>
    internal static class UnityCliAssetService
    {
        // -------- entry point --------------------------------------------------------

        /// <summary>
        /// Executes the requested AssetDatabase operation.
        /// </summary>
        /// <param name="request">The bridge request containing the asset action and paths.</param>
        /// <returns>The asset command data and a human-readable status message.</returns>
        public static (AssetData data, string message) Execute(UnityCliBridgeRequest request)
        {
            if (request == null)
            {
                throw new InvalidOperationException("Empty request.");
            }

            var action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            switch (action)
            {
                case "search":         return DoSearch(request);
                case "create-folder":  return DoCreateFolder(request);
                case "import":         return DoImport(request);
                case "refresh":
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    return (new AssetData { action = "refresh", respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture) }, "asset database refreshed");
                case "delete":         return DoDelete(request);
                case "move":           return DoMove(request);
                case "copy":           return DoCopy(request);
                default:
                    throw new InvalidOperationException(
                        "asset requires --action search|create-folder|import|delete|move|copy " +
                        "(got: \"" + (request.action ?? string.Empty) + "\").");
            }
        }

        // -------- search -------------------------------------------------------------

        /// <summary>
        /// Searches the AssetDatabase with the optional text query and type filter.
        /// </summary>
        /// <param name="request">The bridge request containing search text, type, and count.</param>
        /// <returns>The matching asset entries and a summary message.</returns>
        static (AssetData, string) DoSearch(UnityCliBridgeRequest request)
        {
            var query = (request.search ?? string.Empty).Trim();
            var typeFilter = (request.type ?? string.Empty).Trim();
            var requestedCount = request.count > 0 ? request.count : 50;
            // Hard cap so a wide-open search can't return tens of thousands of asset
            // entries over the pipe.
            requestedCount = Math.Min(requestedCount, 500);

            var filterParts = new List<string>(2);
            if (query.Length > 0) filterParts.Add(query);
            if (typeFilter.Length > 0) filterParts.Add("t:" + typeFilter);
            var filter = string.Join(" ", filterParts);

            string[] guids;
            try
            {
                guids = AssetDatabase.FindAssets(filter);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("AssetDatabase.FindAssets threw: " + ex.Message +
                    " (filter: \"" + filter + "\")");
            }

            var entries = new List<AssetEntry>(Math.Min(guids.Length, requestedCount));
            foreach (var guid in guids)
            {
                if (entries.Count >= requestedCount) break;
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                entries.Add(BuildEntry(guid, path));
            }

            return (
                new AssetData
                {
                    action = "search",
                    filter = filter,
                    totalMatched = guids.Length,
                    returnedCount = entries.Count,
                    respondedAtUtc = NowUtc(),
                    assets = entries.ToArray()
                },
                "asset search: " + entries.Count + " of " + guids.Length + " match(es)"
            );
        }

        // -------- create-folder ------------------------------------------------------

        /// <summary>
        /// Creates a folder under Assets or Packages, including missing intermediate folders.
        /// </summary>
        /// <param name="request">The bridge request containing the destination folder path.</param>
        /// <returns>The created or existing folder metadata and a summary message.</returns>
        static (AssetData, string) DoCreateFolder(UnityCliBridgeRequest request)
        {
            var dst = NormalizeAssetPath(request.assetDstPath);
            RequireUnderAssetsOrPackages(dst, "--dst");

            if (AssetDatabase.IsValidFolder(dst))
            {
                // Idempotent: already a folder is success, just no-op so callers can run
                // create-folder repeatedly without special-casing.
                var existingGuid = AssetDatabase.AssetPathToGUID(dst);
                return (BuildSinglePathData("create-folder", string.Empty, dst, existingGuid), "folder already exists");
            }

            // AssetDatabase.CreateFolder only creates ONE level at a time. Walk down from
            // the first missing ancestor so callers can pass deep paths in one shot.
            var segments = dst.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    var newGuid = AssetDatabase.CreateFolder(current, segments[i]);
                    if (string.IsNullOrEmpty(newGuid))
                    {
                        throw new InvalidOperationException(
                            "AssetDatabase.CreateFolder failed at \"" + current + "/" + segments[i] + "\".");
                    }
                }
                current = next;
            }

            var guid = AssetDatabase.AssetPathToGUID(dst);
            return (BuildSinglePathData("create-folder", string.Empty, dst, guid), "folder created");
        }

        // -------- import (copy file from disk into Assets/ then refresh) -------------

        /// <summary>
        /// Copies a disk file into the Unity project and imports it through AssetDatabase.
        /// </summary>
        /// <param name="request">The bridge request containing source and destination paths.</param>
        /// <returns>The imported asset metadata and a summary message.</returns>
        static (AssetData, string) DoImport(UnityCliBridgeRequest request)
        {
            var src = (request.assetSrcPath ?? string.Empty).Trim();
            var dst = NormalizeAssetPath(request.assetDstPath);

            if (string.IsNullOrEmpty(src))
            {
                throw new InvalidOperationException("import requires --src <disk path>.");
            }
            RequireUnderAssetsOrPackages(dst, "--dst");

            if (!File.Exists(src))
            {
                throw new InvalidOperationException("Source file not found on disk: " + src);
            }

            // Resolve dst absolute path under the project root so File.Copy behaves the
            // same regardless of the CLI's current working directory at request time.
            var projectRoot = Path.GetDirectoryName(Application.dataPath)
                              ?? Directory.GetCurrentDirectory();
            var absoluteDst = Path.GetFullPath(Path.Combine(projectRoot, dst));

            var dstDir = Path.GetDirectoryName(absoluteDst);
            if (!string.IsNullOrEmpty(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }

            try
            {
                File.Copy(src, absoluteDst, overwrite: true);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("File.Copy failed: " + ex.Message);
            }

            AssetDatabase.ImportAsset(dst, ImportAssetOptions.ForceUpdate);

            var guid = AssetDatabase.AssetPathToGUID(dst);
            return (BuildSinglePathData("import", src, dst, guid), "asset imported");
        }

        // -------- delete -------------------------------------------------------------

        /// <summary>
        /// Deletes an asset from Assets or Packages.
        /// </summary>
        /// <param name="request">The bridge request containing the asset path to delete.</param>
        /// <returns>The deleted asset metadata and a summary message.</returns>
        static (AssetData, string) DoDelete(UnityCliBridgeRequest request)
        {
            // Delete reads the path from --src semantically: "the asset I'm consuming".
            // Allow --dst as an alias to be forgiving.
            var path = NormalizeAssetPath(
                string.IsNullOrEmpty(request.assetSrcPath) ? request.assetDstPath : request.assetSrcPath);
            RequireUnderAssetsOrPackages(path, "--src");

            var guidBefore = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guidBefore))
            {
                throw new InvalidOperationException("Asset does not exist: " + path);
            }

            if (!AssetDatabase.DeleteAsset(path))
            {
                throw new InvalidOperationException(
                    "AssetDatabase.DeleteAsset returned false for: " + path +
                    " (file may be locked, in use by another process, or referenced by a non-deletable system asset).");
            }

            return (BuildSinglePathData("delete", path, string.Empty, guidBefore), "asset deleted");
        }

        // -------- move ---------------------------------------------------------------

        /// <summary>
        /// Moves an existing asset to a new project path.
        /// </summary>
        /// <param name="request">The bridge request containing source and destination asset paths.</param>
        /// <returns>The moved asset metadata and a summary message.</returns>
        static (AssetData, string) DoMove(UnityCliBridgeRequest request)
        {
            var src = NormalizeAssetPath(request.assetSrcPath);
            var dst = NormalizeAssetPath(request.assetDstPath);
            RequireUnderAssetsOrPackages(src, "--src");
            RequireUnderAssetsOrPackages(dst, "--dst");

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(src)))
            {
                throw new InvalidOperationException("Source asset does not exist: " + src);
            }

            EnsureParentFolder(dst);

            // ValidateMoveAsset returns "" on success and a human-readable error otherwise.
            var validation = AssetDatabase.ValidateMoveAsset(src, dst);
            if (!string.IsNullOrEmpty(validation))
            {
                throw new InvalidOperationException("ValidateMoveAsset rejected the move: " + validation);
            }

            var error = AssetDatabase.MoveAsset(src, dst);
            if (!string.IsNullOrEmpty(error))
            {
                throw new InvalidOperationException("AssetDatabase.MoveAsset failed: " + error);
            }

            var guid = AssetDatabase.AssetPathToGUID(dst);
            return (BuildSinglePathData("move", src, dst, guid), "asset moved");
        }

        // -------- copy ---------------------------------------------------------------

        /// <summary>
        /// Copies an existing asset to another project path.
        /// </summary>
        /// <param name="request">The bridge request containing source and destination asset paths.</param>
        /// <returns>The copied asset metadata and a summary message.</returns>
        static (AssetData, string) DoCopy(UnityCliBridgeRequest request)
        {
            var src = NormalizeAssetPath(request.assetSrcPath);
            var dst = NormalizeAssetPath(request.assetDstPath);
            RequireUnderAssetsOrPackages(src, "--src");
            RequireUnderAssetsOrPackages(dst, "--dst");

            if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(src)))
            {
                throw new InvalidOperationException("Source asset does not exist: " + src);
            }

            EnsureParentFolder(dst);

            if (!AssetDatabase.CopyAsset(src, dst))
            {
                throw new InvalidOperationException(
                    "AssetDatabase.CopyAsset failed (dst already exists or invalid path): " + dst);
            }

            var guid = AssetDatabase.AssetPathToGUID(dst);
            return (BuildSinglePathData("copy", src, dst, guid), "asset copied");
        }

        // -------- helpers ------------------------------------------------------------

        /// <summary>
        /// Normalizes a Unity asset path to AssetDatabase-compatible forward slashes.
        /// </summary>
        /// <param name="raw">The raw path received from the CLI request.</param>
        /// <returns>The normalized asset path, or an empty string.</returns>
        static string NormalizeAssetPath(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            // AssetDatabase always wants forward slashes, even on Windows.
            return raw.Trim().Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>
        /// Rejects paths outside Unity project asset roots before calling AssetDatabase.
        /// </summary>
        /// <param name="path">The normalized asset path to validate.</param>
        /// <param name="flagName">The CLI flag name used in error messages.</param>
        static void RequireUnderAssetsOrPackages(string path, string flagName)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new InvalidOperationException("Missing " + flagName + " <Assets/... or Packages/...>.");
            }

            if (!path.StartsWith("Assets/", StringComparison.Ordinal) &&
                !path.Equals("Assets", StringComparison.Ordinal) &&
                !path.StartsWith("Packages/", StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    flagName + " must be inside Assets/ or Packages/ (got: \"" + path + "\").");
            }
        }

        /// <summary>
        /// Creates missing parent folders for an asset destination path.
        /// </summary>
        /// <param name="assetPath">The destination asset path.</param>
        static void EnsureParentFolder(string assetPath)
        {
            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(parent) || parent == "Assets" || parent.StartsWith("Packages/"))
            {
                return;
            }

            if (AssetDatabase.IsValidFolder(parent))
            {
                return;
            }

            // Build the chain with the same logic as create-folder but without producing a result.
            var segments = parent.Split('/');
            var current = segments[0];
            for (var i = 1; i < segments.Length; i++)
            {
                var next = current + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    var newGuid = AssetDatabase.CreateFolder(current, segments[i]);
                    if (string.IsNullOrEmpty(newGuid))
                    {
                        throw new InvalidOperationException(
                            "Failed to auto-create destination folder: " + next);
                    }
                }
                current = next;
            }
        }

        /// <summary>
        /// Builds response data for operations that affect a single asset path.
        /// </summary>
        /// <param name="action">The asset action name.</param>
        /// <param name="src">The source asset path, when applicable.</param>
        /// <param name="dst">The destination asset path, when applicable.</param>
        /// <param name="guid">The known asset GUID, when available.</param>
        /// <returns>A response payload containing the affected asset entry.</returns>
        static AssetData BuildSinglePathData(string action, string src, string dst, string guid)
        {
            var resolvedPath = string.IsNullOrEmpty(dst) ? src : dst;
            var entry = string.IsNullOrEmpty(resolvedPath)
                ? null
                : (string.IsNullOrEmpty(guid) ? BuildEntry(string.Empty, resolvedPath) : BuildEntry(guid, resolvedPath));

            return new AssetData
            {
                action = action,
                srcPath = src ?? string.Empty,
                dstPath = dst ?? string.Empty,
                filter = string.Empty,
                totalMatched = entry == null ? 0 : 1,
                returnedCount = entry == null ? 0 : 1,
                respondedAtUtc = NowUtc(),
                assets = entry == null ? Array.Empty<AssetEntry>() : new[] { entry }
            };
        }

        /// <summary>
        /// Builds a serializable asset entry from a GUID and Unity asset path.
        /// </summary>
        /// <param name="guid">The asset GUID, or an empty string to resolve it.</param>
        /// <param name="path">The Unity asset path.</param>
        /// <returns>The serialized asset entry.</returns>
        static AssetEntry BuildEntry(string guid, string path)
        {
            // Folder check first because LoadMainAssetAtPath on a folder returns
            // DefaultAsset, which is technically a Type but tells the caller little.
            var isFolder = AssetDatabase.IsValidFolder(path);
            string typeName;
            if (isFolder)
            {
                typeName = "Folder";
            }
            else
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                typeName = type == null ? "Unknown" : (type.FullName ?? type.Name);
            }

            return new AssetEntry
            {
                guid = string.IsNullOrEmpty(guid) ? AssetDatabase.AssetPathToGUID(path) : guid,
                path = path,
                mainAssetType = typeName,
                isFolder = isFolder
            };
        }

        /// <summary>
        /// Formats the current UTC time for bridge responses.
        /// </summary>
        /// <returns>An ISO-8601 UTC timestamp.</returns>
        static string NowUtc()
        {
            return DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
