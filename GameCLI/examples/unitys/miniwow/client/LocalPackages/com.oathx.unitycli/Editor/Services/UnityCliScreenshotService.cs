using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Captures the Unity Game view and writes the result beside the Assets folder.
    /// </summary>
    internal static class UnityCliScreenshotService
    {
        /// <summary>
        /// Stores the default project-root folder used for screenshots.
        /// </summary>
        const string DefaultScreenshotFolder = "UnityCLI-Screenshots";

        /// <summary>
        /// Stores the number of logical pixels removed from the top of an EditorWindow when excluding chrome.
        /// </summary>
        const float EditorWindowToolbarHeight = 22f;
        /// <summary>
        /// Stores the number of logical pixels removed for the GameView display/scale toolbar.
        /// </summary>
        const float GameViewToolbarHeight = 24f;

        /// <summary>
        /// Captures a Game view screenshot for a bridge request.
        /// </summary>
        /// <param name="request">The bridge request.</param>
        /// <returns>The screenshot metadata.</returns>
        internal static ScreenshotData Execute(UnityCliBridgeRequest request)
        {
            var projectRoot = ProjectRoot;
            var superSize = Math.Max(1, request != null ? request.screenshotSuperSize : 1);
            var outputPath = ResolveOutputPath(projectRoot, request);

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            var capture = CaptureGameView(superSize);
            File.WriteAllBytes(outputPath, capture.pngBytes);

            return new ScreenshotData
            {
                path = outputPath,
                projectRelativePath = ToProjectRelativePath(projectRoot, outputPath),
                directory = Path.GetDirectoryName(outputPath),
                fileName = Path.GetFileName(outputPath),
                width = capture.width,
                height = capture.height,
                bytes = capture.pngBytes.LongLength,
                superSize = superSize,
                captureMethod = capture.method,
                respondedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Gets the current Unity project root.
        /// </summary>
        static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        /// <summary>
        /// Captures and crops the GameView editor window.
        /// </summary>
        /// <param name="superSize">The output scale multiplier.</param>
        /// <returns>The captured screenshot payload.</returns>
        static ScreenshotCapture CaptureGameView(int superSize)
        {
            var window = FindGameView();
            var windowTexture = CaptureGameViewWindow(window);
            var contentTexture = CropGameViewContent(windowTexture);

            try
            {
                var scaledTexture = ResizeIfNeeded(contentTexture, superSize);
                if (!ReferenceEquals(scaledTexture, contentTexture))
                {
                    UnityEngine.Object.DestroyImmediate(contentTexture);
                }

                try
                {
                    return Encode(scaledTexture, "GameView.EditorWindow");
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(scaledTexture);
                }
            }
            catch
            {
                UnityEngine.Object.DestroyImmediate(contentTexture);
                throw;
            }
        }

        /// <summary>
        /// Finds or opens the Unity GameView editor window.
        /// </summary>
        /// <returns>The GameView editor window.</returns>
        static EditorWindow FindGameView()
        {
            var gameViewType = ResolveEditorWindowType("GameView");
            if (gameViewType == null)
            {
                throw new InvalidOperationException("UnityEditor.GameView type was not found.");
            }

            var window = Resources.FindObjectsOfTypeAll(gameViewType).OfType<EditorWindow>().FirstOrDefault()
                         ?? EditorWindow.GetWindow(gameViewType);
            if (window == null)
            {
                throw new InvalidOperationException("GameView window could not be opened.");
            }

            window.Focus();
            window.Repaint();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            return window;
        }

        /// <summary>
        /// Resolves a Unity editor window type by simple type name.
        /// </summary>
        /// <param name="typeName">The editor window type name.</param>
        /// <returns>The resolved editor window type, or null.</returns>
        static Type ResolveEditorWindowType(string typeName)
        {
            return typeof(EditorWindow).Assembly.GetTypes()
                .FirstOrDefault(type => typeof(EditorWindow).IsAssignableFrom(type)
                                        && (string.Equals(type.Name, typeName, StringComparison.Ordinal)
                                            || string.Equals(type.FullName, typeName, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Captures the GameView window rectangle using the native main-window backend with a Unity screen-pixel fallback.
        /// </summary>
        /// <param name="window">The GameView editor window.</param>
        /// <returns>The captured window texture.</returns>
        static Texture2D CaptureGameViewWindow(EditorWindow window)
        {
            var rect = window.position;
            var topChromeHeight = EditorWindowToolbarHeight + GameViewToolbarHeight;
            rect.y += topChromeHeight;
            rect.height = Mathf.Max(1f, rect.height - topChromeHeight);

            if (TryCaptureEditorWindowFromNativeMainWindow(rect, out var nativeTexture))
            {
                return nativeTexture;
            }

            return CaptureScreenRect(rect);
        }

        /// <summary>
        /// Captures the Unity main window through Win32 and crops the requested editor-window rectangle.
        /// </summary>
        /// <param name="windowRect">The editor-window rectangle in Unity logical coordinates.</param>
        /// <param name="texture">The cropped texture.</param>
        /// <returns>True when the native capture succeeded.</returns>
        static bool TryCaptureEditorWindowFromNativeMainWindow(Rect windowRect, out Texture2D texture)
        {
            texture = null;
            if (!NativeScreenCapture.TryCaptureUnityEditorWindow(out var mainTexture))
            {
                return false;
            }

            try
            {
                if (!TryGetMainEditorWindowRect(out var mainRect) || mainRect.width <= 0f || mainRect.height <= 0f)
                {
                    return false;
                }

                var scaleX = mainTexture.width / mainRect.width;
                var scaleY = mainTexture.height / mainRect.height;
                var cropRect = ClampTopLeftRect(
                    new RectInt(
                        Mathf.RoundToInt((windowRect.x - mainRect.x) * scaleX),
                        Mathf.RoundToInt((windowRect.y - mainRect.y) * scaleY),
                        Mathf.RoundToInt(windowRect.width * scaleX),
                        Mathf.RoundToInt(windowRect.height * scaleY)),
                    mainTexture.width,
                    mainTexture.height);

                if (cropRect.width <= 0 || cropRect.height <= 0)
                {
                    return false;
                }

                texture = CropTextureFromTopLeft(mainTexture, cropRect);
                return true;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mainTexture);
            }
        }

        /// <summary>
        /// Reads a screen rectangle through Unity's editor pixel API.
        /// </summary>
        /// <param name="rect">The screen rectangle in Unity logical coordinates.</param>
        /// <returns>The captured texture.</returns>
        static Texture2D CaptureScreenRect(Rect rect)
        {
            var readWidth = Mathf.Max(1, Mathf.RoundToInt(rect.width));
            var readHeight = Mathf.Max(1, Mathf.RoundToInt(rect.height));
            var colors = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(
                new Vector2(rect.x, rect.y),
                readWidth,
                readHeight);

            if (colors == null || colors.Length == 0)
            {
                throw new InvalidOperationException("GameView screen pixels could not be read.");
            }

            var texture = new Texture2D(readWidth, readHeight, TextureFormat.RGBA32, false);
            texture.SetPixels(colors);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>
        /// Tries to resolve the Unity main editor window rectangle in logical coordinates.
        /// </summary>
        /// <param name="rect">The resolved rectangle.</param>
        /// <returns>True when the rectangle was resolved.</returns>
        static bool TryGetMainEditorWindowRect(out Rect rect)
        {
            rect = default;
            var containerWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ContainerWindow");
            if (containerWindowType == null)
            {
                return false;
            }

            var windows = Resources.FindObjectsOfTypeAll(containerWindowType);
            var showModeProperty = containerWindowType.GetProperty("showMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var positionProperty = containerWindowType.GetProperty("position", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var window in windows)
            {
                var showMode = showModeProperty != null ? showModeProperty.GetValue(window, null) : null;
                if (showMode == null || Convert.ToInt32(showMode, CultureInfo.InvariantCulture) != 4)
                {
                    continue;
                }

                var position = positionProperty != null ? positionProperty.GetValue(window, null) : null;
                if (position is Rect mainRect && mainRect.width > 0f && mainRect.height > 0f)
                {
                    rect = mainRect;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Crops the likely GameView content from a captured GameView window texture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <returns>The cropped content texture, or the original source when auto-crop fails.</returns>
        static Texture2D CropGameViewContent(Texture2D source)
        {
            if (!TryFindGameViewContentRect(source, ResolveGameViewAspect(), out var contentRect))
            {
                return source;
            }

            var cropped = CropTextureFromTopLeft(source, contentRect);
            UnityEngine.Object.DestroyImmediate(source);
            return cropped;
        }

        /// <summary>
        /// Finds the likely GameView image content inside an editor-window capture.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="targetAspect">The expected GameView aspect ratio.</param>
        /// <param name="rect">The resolved content rectangle in top-left coordinates.</param>
        /// <returns>True when content was found.</returns>
        static bool TryFindGameViewContentRect(Texture2D source, float targetAspect, out RectInt rect)
        {
            rect = default;
            var pixels = source.GetPixels32();
            if (pixels == null || pixels.Length == 0)
            {
                return false;
            }

            var rowCounts = new int[source.height];
            var columnCounts = new int[source.width];
            var ignoredTop = Mathf.Clamp(Mathf.RoundToInt(source.height * 0.09f), 0, source.height - 1);
            for (var y = ignoredTop; y < source.height; y++)
            {
                var textureRow = source.height - 1 - y;
                for (var x = 0; x < source.width; x++)
                {
                    if (!IsLikelyGamePixel(pixels[textureRow * source.width + x]))
                    {
                        continue;
                    }

                    rowCounts[y]++;
                    columnCounts[x]++;
                }
            }

            var rowThreshold = Mathf.Max(4, Mathf.RoundToInt(source.width * 0.08f));
            var columnThreshold = Mathf.Max(4, Mathf.RoundToInt(source.height * 0.08f));
            var minY = FindFirstAbove(rowCounts, rowThreshold);
            var maxY = FindLastAbove(rowCounts, rowThreshold);
            var minX = FindFirstAbove(columnCounts, columnThreshold);
            var maxX = FindLastAbove(columnCounts, columnThreshold);
            if (minX < 0 || minY < 0 || maxX <= minX || maxY <= minY)
            {
                return false;
            }

            rect = ExpandTopLeftRectToAspect(
                new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1),
                targetAspect,
                source.width,
                source.height);
            if (rect.y < minY)
            {
                var trimTop = minY - rect.y;
                rect = ClampTopLeftRect(
                    new RectInt(rect.x, minY, rect.width, rect.height - trimTop),
                    source.width,
                    source.height);
            }

            return rect.width > source.width * 0.1f && rect.height > source.height * 0.1f;
        }

        /// <summary>
        /// Returns whether a pixel is likely part of the rendered GameView content rather than editor chrome.
        /// </summary>
        /// <param name="color">The pixel color.</param>
        /// <returns>True when the pixel likely belongs to game content.</returns>
        static bool IsLikelyGamePixel(Color32 color)
        {
            var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
            if (max < 24)
            {
                return false;
            }

            var saturation = max == 0 ? 0f : (max - min) / (float)max;
            if (saturation > 0.12f && max > 35)
            {
                return true;
            }

            return color.r > 175 && color.g > 150 && color.b > 90;
        }

        /// <summary>
        /// Finds the first index whose value reaches a threshold.
        /// </summary>
        /// <param name="values">The values to inspect.</param>
        /// <param name="threshold">The threshold value.</param>
        /// <returns>The first matching index, or -1.</returns>
        static int FindFirstAbove(int[] values, int threshold)
        {
            for (var index = 0; index < values.Length; index++)
            {
                if (values[index] >= threshold)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the last index whose value reaches a threshold.
        /// </summary>
        /// <param name="values">The values to inspect.</param>
        /// <param name="threshold">The threshold value.</param>
        /// <returns>The last matching index, or -1.</returns>
        static int FindLastAbove(int[] values, int threshold)
        {
            for (var index = values.Length - 1; index >= 0; index--)
            {
                if (values[index] >= threshold)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Resolves the current GameView aspect ratio.
        /// </summary>
        /// <returns>The GameView aspect ratio.</returns>
        static float ResolveGameViewAspect()
        {
            var gameViewType = ResolveEditorWindowType("GameView");
            var targetSizeMethod = gameViewType != null
                ? gameViewType.GetMethod("GetMainGameViewTargetSize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                : null;

            if (targetSizeMethod != null && targetSizeMethod.Invoke(null, null) is Vector2 targetSize && targetSize.x > 0f && targetSize.y > 0f)
            {
                return targetSize.x / targetSize.y;
            }

            if (Screen.width > 0 && Screen.height > 0)
            {
                return Screen.width / (float)Screen.height;
            }

            return 16f / 9f;
        }

        /// <summary>
        /// Expands a top-left rectangle to the target aspect ratio.
        /// </summary>
        /// <param name="rect">The source rectangle.</param>
        /// <param name="aspect">The target aspect ratio.</param>
        /// <param name="width">The containing width.</param>
        /// <param name="height">The containing height.</param>
        /// <returns>The expanded rectangle.</returns>
        static RectInt ExpandTopLeftRectToAspect(RectInt rect, float aspect, int width, int height)
        {
            aspect = Mathf.Max(0.01f, aspect);
            var centerX = rect.x + rect.width * 0.5f;
            var centerY = rect.y + rect.height * 0.5f;
            var targetWidth = rect.width;
            var targetHeight = rect.height;

            if (targetWidth / (float)targetHeight < aspect)
            {
                targetWidth = Mathf.CeilToInt(targetHeight * aspect);
            }
            else
            {
                targetHeight = Mathf.CeilToInt(targetWidth / aspect);
            }

            var x = Mathf.RoundToInt(centerX - targetWidth * 0.5f);
            var y = Mathf.RoundToInt(centerY - targetHeight * 0.5f);
            return ClampTopLeftRect(new RectInt(x, y, targetWidth, targetHeight), width, height);
        }

        /// <summary>
        /// Clamps a top-left rectangle to a containing texture size.
        /// </summary>
        /// <param name="rect">The source rectangle.</param>
        /// <param name="width">The containing width.</param>
        /// <param name="height">The containing height.</param>
        /// <returns>The clamped rectangle.</returns>
        static RectInt ClampTopLeftRect(RectInt rect, int width, int height)
        {
            var x = Mathf.Clamp(rect.x, 0, width);
            var y = Mathf.Clamp(rect.y, 0, height);
            var xMax = Mathf.Clamp(rect.x + rect.width, 0, width);
            var yMax = Mathf.Clamp(rect.y + rect.height, 0, height);
            return new RectInt(x, y, Mathf.Max(0, xMax - x), Mathf.Max(0, yMax - y));
        }

        /// <summary>
        /// Crops a texture using a top-left coordinate rectangle.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="topLeftRect">The top-left rectangle.</param>
        /// <returns>The cropped texture.</returns>
        static Texture2D CropTextureFromTopLeft(Texture2D source, RectInt topLeftRect)
        {
            var bottomLeftY = source.height - topLeftRect.y - topLeftRect.height;
            var colors = source.GetPixels(topLeftRect.x, bottomLeftY, topLeftRect.width, topLeftRect.height);
            var texture = new Texture2D(topLeftRect.width, topLeftRect.height, TextureFormat.RGBA32, false);
            texture.SetPixels(colors);
            texture.Apply(false, false);
            return texture;
        }

        /// <summary>
        /// Resizes a texture when the requested scale is greater than one.
        /// </summary>
        /// <param name="source">The source texture.</param>
        /// <param name="scale">The output scale.</param>
        /// <returns>The original texture or a resized copy.</returns>
        static Texture2D ResizeIfNeeded(Texture2D source, int scale)
        {
            if (scale <= 1)
            {
                return source;
            }

            var width = Mathf.Max(1, source.width * scale);
            var height = Mathf.Max(1, source.height * scale);
            var renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            var previousActive = RenderTexture.active;
            try
            {
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                var resized = new Texture2D(width, height, TextureFormat.RGBA32, false);
                resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                resized.Apply(false, false);
                return resized;
            }
            finally
            {
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        /// <summary>
        /// Encodes a texture as PNG screenshot data.
        /// </summary>
        /// <param name="texture">The source texture.</param>
        /// <param name="method">The capture method label.</param>
        /// <returns>The encoded screenshot payload.</returns>
        static ScreenshotCapture Encode(Texture2D texture, string method)
        {
            var bytes = texture.EncodeToPNG();
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidOperationException("GameView screenshot PNG encoding returned no bytes.");
            }

            return new ScreenshotCapture
            {
                width = texture.width,
                height = texture.height,
                pngBytes = bytes,
                method = method
            };
        }

        /// <summary>
        /// Resolves the requested output path under the project root and outside Assets.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="request">The bridge request.</param>
        /// <returns>The absolute screenshot output path.</returns>
        static string ResolveOutputPath(string projectRoot, UnityCliBridgeRequest request)
        {
            var requested = FirstNonEmpty(
                request != null ? request.screenshotPath : string.Empty,
                request != null ? request.assetDstPath : string.Empty).Trim();

            if (string.IsNullOrEmpty(requested))
            {
                requested = DefaultScreenshotFolder;
            }

            requested = requested.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

            var combined = Path.IsPathRooted(requested)
                ? Path.GetFullPath(requested)
                : Path.GetFullPath(Path.Combine(projectRoot, requested));

            var outputPath = IsUnderUnityAssetFolder(projectRoot, combined)
                ? RedirectAssetPathToScreenshotFolder(projectRoot, requested, combined)
                : LooksLikeDirectory(requested, combined)
                    ? Path.Combine(combined, CreateDefaultFileName())
                    : EnsurePngExtension(combined);

            EnsurePathIsUnderProjectRoot(projectRoot, outputPath);
            EnsurePathIsOutsideUnityAssetFolders(projectRoot, outputPath);
            return outputPath;
        }

        /// <summary>
        /// Returns whether a requested path should be treated as a directory.
        /// </summary>
        /// <param name="requested">The raw requested path.</param>
        /// <param name="combined">The absolute requested path.</param>
        /// <returns>True when the request names a directory.</returns>
        static bool LooksLikeDirectory(string requested, string combined)
        {
            return requested.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                   || Directory.Exists(combined)
                   || string.IsNullOrEmpty(Path.GetExtension(combined));
        }

        /// <summary>
        /// Ensures the output file uses the PNG extension.
        /// </summary>
        /// <param name="path">The output path.</param>
        /// <returns>The output path with a PNG extension.</returns>
        static string EnsurePngExtension(string path)
        {
            return string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase)
                ? path
                : Path.ChangeExtension(path, ".png");
        }

        /// <summary>
        /// Creates the default screenshot file name.
        /// </summary>
        /// <returns>The generated file name.</returns>
        static string CreateDefaultFileName()
        {
            return "GameView_" + DateTime.Now.ToString("yyyyMMdd_HHmmss_fff", CultureInfo.InvariantCulture) + ".png";
        }

        /// <summary>
        /// Validates that the path stays under the current project root.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="path">The output path.</param>
        static void EnsurePathIsUnderProjectRoot(string projectRoot, string path)
        {
            var root = EnsureTrailingSeparator(Path.GetFullPath(projectRoot));
            var fullPath = Path.GetFullPath(path);
            if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Screenshot output must stay under the Unity project root: " + projectRoot);
            }
        }

        /// <summary>
        /// Validates that screenshots are not written into Unity asset import folders.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="path">The output path.</param>
        static void EnsurePathIsOutsideUnityAssetFolders(string projectRoot, string path)
        {
            if (IsUnderUnityAssetFolder(projectRoot, path))
            {
                throw new InvalidOperationException("Screenshots must be saved beside Assets, not inside Assets or Packages.");
            }
        }

        /// <summary>
        /// Redirects a mistakenly asset-folder based screenshot request to the project-root screenshot folder.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="requested">The raw requested path.</param>
        /// <param name="combined">The absolute requested path.</param>
        /// <returns>The redirected absolute screenshot output path.</returns>
        static string RedirectAssetPathToScreenshotFolder(string projectRoot, string requested, string combined)
        {
            var fileName = LooksLikeDirectory(requested, combined)
                ? CreateDefaultFileName()
                : Path.GetFileName(EnsurePngExtension(combined));

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = CreateDefaultFileName();
            }

            return Path.GetFullPath(Path.Combine(projectRoot, DefaultScreenshotFolder, fileName));
        }

        /// <summary>
        /// Returns whether a path points inside Unity asset import folders.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="path">The path to inspect.</param>
        /// <returns>True when the path is under Assets or Packages.</returns>
        static bool IsUnderUnityAssetFolder(string projectRoot, string path)
        {
            var fullPath = Path.GetFullPath(path);
            return IsSameOrChildPath(fullPath, Path.Combine(projectRoot, "Assets"))
                   || IsSameOrChildPath(fullPath, Path.Combine(projectRoot, "Packages"));
        }

        /// <summary>
        /// Returns whether a path is the same as or nested under a directory.
        /// </summary>
        /// <param name="path">The candidate path.</param>
        /// <param name="directory">The containing directory.</param>
        /// <returns>True when the path is the directory or one of its descendants.</returns>
        static bool IsSameOrChildPath(string path, string directory)
        {
            var fullDirectory = Path.GetFullPath(directory);
            return string.Equals(path, fullDirectory, StringComparison.OrdinalIgnoreCase)
                   || path.StartsWith(EnsureTrailingSeparator(fullDirectory), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Converts an absolute path to a project-root-relative path.
        /// </summary>
        /// <param name="projectRoot">The Unity project root.</param>
        /// <param name="path">The absolute path.</param>
        /// <returns>The project-root-relative path.</returns>
        static string ToProjectRelativePath(string projectRoot, string path)
        {
            var root = EnsureTrailingSeparator(Path.GetFullPath(projectRoot));
            var fullPath = Path.GetFullPath(path);
            return fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(root.Length).Replace('\\', '/')
                : fullPath;
        }

        /// <summary>
        /// Ensures a path ends with a directory separator.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The path with a trailing separator.</returns>
        static string EnsureTrailingSeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Returns the first non-empty value from two candidates.
        /// </summary>
        /// <param name="first">The preferred value.</param>
        /// <param name="second">The fallback value.</param>
        /// <returns>The first non-empty value.</returns>
        static string FirstNonEmpty(string first, string second)
        {
            return string.IsNullOrWhiteSpace(first) ? (second ?? string.Empty) : first;
        }

        /// <summary>
        /// Stores a screenshot capture payload.
        /// </summary>
        struct ScreenshotCapture
        {
            /// <summary>
            /// Stores the screenshot width in pixels.
            /// </summary>
            public int width;
            /// <summary>
            /// Stores the screenshot height in pixels.
            /// </summary>
            public int height;
            /// <summary>
            /// Stores the PNG payload.
            /// </summary>
            public byte[] pngBytes;
            /// <summary>
            /// Stores the capture method label.
            /// </summary>
            public string method;
        }

        /// <summary>
        /// Provides Windows native screenshot helpers for Unity editor windows.
        /// </summary>
        static class NativeScreenCapture
        {
#if UNITY_EDITOR_WIN
            /// <summary>
            /// Stores the BitBlt source-copy raster operation.
            /// </summary>
            const int SrcCopy = 0x00CC0020;
            /// <summary>
            /// Stores the DIB RGB color mode.
            /// </summary>
            const uint DibRgbColors = 0;
            /// <summary>
            /// Stores the BI_RGB compression mode.
            /// </summary>
            const uint BiRgb = 0;

            /// <summary>
            /// Captures the Unity editor main window.
            /// </summary>
            /// <param name="texture">The captured texture.</param>
            /// <returns>True when the capture succeeded.</returns>
            public static bool TryCaptureUnityEditorWindow(out Texture2D texture)
            {
                texture = null;
                var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                RestoreWindow(handle);
                if (!GetWindowRect(handle, out var rect) || rect.Width <= 0 || rect.Height <= 0)
                {
                    return false;
                }

                return TryCaptureWindow(handle, rect.Width, rect.Height, out texture);
            }

            /// <summary>
            /// Captures a native window into a Unity texture.
            /// </summary>
            /// <param name="windowHandle">The native window handle.</param>
            /// <param name="width">The window width.</param>
            /// <param name="height">The window height.</param>
            /// <param name="texture">The captured texture.</param>
            /// <returns>True when the capture succeeded.</returns>
            static bool TryCaptureWindow(IntPtr windowHandle, int width, int height, out Texture2D texture)
            {
                texture = null;
                var windowDc = GetWindowDC(windowHandle);
                if (windowDc == IntPtr.Zero)
                {
                    return false;
                }

                var memoryDc = IntPtr.Zero;
                var bitmap = IntPtr.Zero;
                var oldObject = IntPtr.Zero;
                try
                {
                    memoryDc = CreateCompatibleDC(windowDc);
                    if (memoryDc == IntPtr.Zero)
                    {
                        return false;
                    }

                    bitmap = CreateCompatibleBitmap(windowDc, width, height);
                    if (bitmap == IntPtr.Zero)
                    {
                        return false;
                    }

                    oldObject = SelectObject(memoryDc, bitmap);
                    RestoreWindow(windowHandle);
                    var captured = PrintWindow(windowHandle, memoryDc, 2)
                                   || PrintWindow(windowHandle, memoryDc, 0)
                                   || BitBlt(memoryDc, 0, 0, width, height, windowDc, 0, 0, SrcCopy);
                    return captured && TryCreateTextureFromBitmap(memoryDc, bitmap, width, height, out texture);
                }
                finally
                {
                    if (oldObject != IntPtr.Zero)
                    {
                        SelectObject(memoryDc, oldObject);
                    }

                    if (bitmap != IntPtr.Zero)
                    {
                        DeleteObject(bitmap);
                    }

                    if (memoryDc != IntPtr.Zero)
                    {
                        DeleteDC(memoryDc);
                    }

                    ReleaseDC(windowHandle, windowDc);
                }
            }

            /// <summary>
            /// Creates a Unity texture from a native bitmap.
            /// </summary>
            /// <param name="deviceContext">The source device context.</param>
            /// <param name="bitmap">The source bitmap.</param>
            /// <param name="width">The bitmap width.</param>
            /// <param name="height">The bitmap height.</param>
            /// <param name="texture">The created texture.</param>
            /// <returns>True when the texture was created.</returns>
            static bool TryCreateTextureFromBitmap(IntPtr deviceContext, IntPtr bitmap, int width, int height, out Texture2D texture)
            {
                texture = null;
                var info = new BitmapInfo
                {
                    header = new BitmapInfoHeader
                    {
                        biSize = Marshal.SizeOf(typeof(BitmapInfoHeader)),
                        biWidth = width,
                        biHeight = -height,
                        biPlanes = 1,
                        biBitCount = 32,
                        biCompression = BiRgb,
                        biSizeImage = (uint)(width * height * 4)
                    }
                };

                var buffer = new byte[width * height * 4];
                var result = GetDIBits(deviceContext, bitmap, 0, (uint)height, buffer, ref info, DibRgbColors);
                if (result == 0)
                {
                    return false;
                }

                var pixels = new Color32[width * height];
                for (var y = 0; y < height; y++)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var sourceIndex = (y * width + x) * 4;
                        var targetIndex = ((height - 1 - y) * width) + x;
                        pixels[targetIndex] = new Color32(
                            buffer[sourceIndex + 2],
                            buffer[sourceIndex + 1],
                            buffer[sourceIndex],
                            buffer[sourceIndex + 3]);
                    }
                }

                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                return true;
            }

            /// <summary>
            /// Restores a minimized native window.
            /// </summary>
            /// <param name="handle">The native window handle.</param>
            static void RestoreWindow(IntPtr handle)
            {
                if (IsIconic(handle))
                {
                    ShowWindow(handle, 9);
                }
            }

            [DllImport("user32.dll")]
            static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            static extern int ReleaseDC(IntPtr hWnd, IntPtr hDc);

            [DllImport("user32.dll")]
            static extern bool GetWindowRect(IntPtr hWnd, out NativeRect rect);

            [DllImport("user32.dll")]
            static extern bool IsIconic(IntPtr hWnd);

            [DllImport("user32.dll")]
            static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll")]
            static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, uint nFlags);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll")]
            static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

            [DllImport("gdi32.dll")]
            static extern bool DeleteObject(IntPtr hObject);

            [DllImport("gdi32.dll")]
            static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

            [DllImport("gdi32.dll")]
            static extern bool BitBlt(IntPtr hdc, int x, int y, int cx, int cy, IntPtr hdcSrc, int x1, int y1, int rop);

            [DllImport("gdi32.dll")]
            static extern int GetDIBits(IntPtr hdc, IntPtr hbm, uint start, uint cLines, byte[] lpvBits, ref BitmapInfo lpbmi, uint usage);

            /// <summary>
            /// Stores a native window rectangle.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            struct NativeRect
            {
                /// <summary>
                /// Stores the left coordinate.
                /// </summary>
                public int Left;
                /// <summary>
                /// Stores the top coordinate.
                /// </summary>
                public int Top;
                /// <summary>
                /// Stores the right coordinate.
                /// </summary>
                public int Right;
                /// <summary>
                /// Stores the bottom coordinate.
                /// </summary>
                public int Bottom;
                /// <summary>
                /// Gets the rectangle width.
                /// </summary>
                public int Width => Right - Left;
                /// <summary>
                /// Gets the rectangle height.
                /// </summary>
                public int Height => Bottom - Top;
            }

            /// <summary>
            /// Stores BITMAPINFO for GetDIBits.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            struct BitmapInfo
            {
                /// <summary>
                /// Stores the bitmap header.
                /// </summary>
                public BitmapInfoHeader header;
                /// <summary>
                /// Stores the first color table entry.
                /// </summary>
                public uint colors;
            }

            /// <summary>
            /// Stores BITMAPINFOHEADER for GetDIBits.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            struct BitmapInfoHeader
            {
                /// <summary>
                /// Stores the header size.
                /// </summary>
                public int biSize;
                /// <summary>
                /// Stores the bitmap width.
                /// </summary>
                public int biWidth;
                /// <summary>
                /// Stores the bitmap height.
                /// </summary>
                public int biHeight;
                /// <summary>
                /// Stores the plane count.
                /// </summary>
                public short biPlanes;
                /// <summary>
                /// Stores the bits per pixel.
                /// </summary>
                public short biBitCount;
                /// <summary>
                /// Stores the compression mode.
                /// </summary>
                public uint biCompression;
                /// <summary>
                /// Stores the image size in bytes.
                /// </summary>
                public uint biSizeImage;
                /// <summary>
                /// Stores the horizontal pixels per meter.
                /// </summary>
                public int biXPelsPerMeter;
                /// <summary>
                /// Stores the vertical pixels per meter.
                /// </summary>
                public int biYPelsPerMeter;
                /// <summary>
                /// Stores the color count.
                /// </summary>
                public uint biClrUsed;
                /// <summary>
                /// Stores the important color count.
                /// </summary>
                public uint biClrImportant;
            }
#else
            /// <summary>
            /// Captures the Unity editor main window.
            /// </summary>
            /// <param name="texture">The captured texture.</param>
            /// <returns>Always false on non-Windows editor platforms.</returns>
            public static bool TryCaptureUnityEditorWindow(out Texture2D texture)
            {
                texture = null;
                return false;
            }
#endif
        }
    }
}
