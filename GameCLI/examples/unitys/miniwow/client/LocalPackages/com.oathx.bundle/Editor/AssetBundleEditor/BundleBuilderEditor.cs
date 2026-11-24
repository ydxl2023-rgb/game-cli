using Oathx.Bundle.Assets;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Oathx.Bundle.Editor
{
    public static class BundleBuilderEditorUtility
    {
        /// <summary>
        /// copy directory
        /// </summary>
        /// <param name="sourceDir"></param>
        /// <param name="destinationDir"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string sourceDir, string destinationDir, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
                throw new DirectoryNotFoundException(sourceDir);

            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, copySubDirs);
                }
            }
        }

        /// <summary>
        /// copy directory with exclude subfix
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="excludeSubfix"></param>
        public static void CopyDirectory(string sourceDirName, string destDirName, string excludeSubfix)
        {
            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (string folderPath in Directory.GetDirectories(sourceDirName, "*", SearchOption.AllDirectories))
            {
                string srcPath = folderPath.Replace(sourceDirName, destDirName);
                if (!Directory.Exists(srcPath))
                    Directory.CreateDirectory(srcPath);
            }

            List<string> copyFiles = new List<string>();
            string[] files = Directory.GetFiles(sourceDirName, "*.*", SearchOption.AllDirectories);
            foreach (string filePath in files)
            {
                if (!string.IsNullOrEmpty(excludeSubfix) && filePath.EndsWith(excludeSubfix))
                    continue;

                string fileFullPath = filePath.Replace("\\", "/");
                copyFiles.Add(fileFullPath);
            }

            for (int i = 0; i < copyFiles.Count; i++)
            {
                string filePath = copyFiles[i];
                string fileName = Path.GetFileName(filePath);
                string destPath = Path.Combine(destDirName, fileName);

                File.Copy(filePath, destPath, true);
            }
        }

        /// <summary>
        /// copy directory
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        public static void CopyDirectory(string sourceDirName, string destDirName)
        {
            CopyDirectory(sourceDirName.Replace("\\", "/"), destDirName.Replace("\\", "/"), true);
        }

        /// <summary>
        /// try get bundle files
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="files"></param>
        /// <param name="entryType"></param>
        /// <returns></returns>
        public static bool TryGetBundleFiles(string assetBundle, out List<AssetFileInfo> files, out EntryType entryType)
        {
            files = new List<AssetFileInfo>();
            entryType = EntryType.None;

            foreach (string file in AssetDatabase.GetAssetPathsFromAssetBundle(assetBundle))
            {
                string filepath = file.ToLower();

                string ext = Path.GetExtension(filepath);
                if (!string.IsNullOrEmpty(ext))
                {
                    entryType = ext.Equals(".unity") ? EntryType.Scene : EntryType.Pack;
                }

                if (TryGetFileInfo(filepath, out uint checksum, out uint size))
                    files.Add(new AssetFileInfo() { file=filepath, size=size});
            }

            return true;
        }

        /// <summary>
        /// get file info
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="checksum"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool TryGetFileInfo(string filePath, out uint checksum, out uint size)
        {
            FileInfo info = new FileInfo(filePath);
            if (info == null)
            {
                checksum = 0;
                size = 0;
                return false;
            }
            else
            {
                size = (uint)(info.Length);
                checksum = XHotfixUtility.ComputeFileChecksum(filePath);
            }

            return true;
        }

        /// <summary>
        /// get all files
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="list"></param>
        /// <param name="allFile"></param>
        public static void GetAllFile(DirectoryInfo directory, List<string> list, bool allFile = true)
        {
            foreach (FileSystemInfo fsi in directory.GetFileSystemInfos())
            {
                if (fsi is FileInfo)
                {
                    if (!fsi.Extension.Equals(".meta") && !string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(fsi.Name)))
                        list.Add(fsi.FullName);
                }
                else
                {
                    if (allFile)
                        GetAllFile((DirectoryInfo)fsi, list);
                }
            }
        }

        /// <summary>
        /// process nobundle resources
        /// </summary>
        /// <returns></returns>
        public static bool ProcessNobundleResources()
        {
            string fileCheckPath = Path.Combine(Application.dataPath, typeof(Resources).Name).Replace("\\", "/");
            if (!Directory.Exists(fileCheckPath))
                Directory.CreateDirectory(fileCheckPath);

            List<string> defaultFiles = new List<string>();

            var directory = new DirectoryInfo(fileCheckPath);
            GetAllFile(directory, defaultFiles, false);

            foreach (string file in defaultFiles)
            {
                string replace = Path.Combine(Application.dataPath,
                    typeof(Resources).Name + "/").Replace("\\", "/").ToLower();

                string path = file.ToLower().Replace("\\", "/").Replace(replace, "");
                if (!XRes.Exists(path))
                {
                    if (TryGetFileInfo(file, out uint nChecksum, out uint nSize))
                    {
                        List<AssetFileInfo> files = new List<AssetFileInfo>();
                        files.Add(new AssetFileInfo() { 
                            file = path, size = nSize
                        });

                        if (Path.HasExtension(path))
                            path = Path.ChangeExtension(path, null);

                        XRes.AddEntry(new Entry()
                        {
                            entryType = EntryType.File,
                            locationType = LocationType.Resource,
                            name = path,
                            files = files.ToArray(),
                            checksum = nChecksum,
                            size = nSize,
                            dependencies = new string[] { }
                        });
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// build and save bundle sheet file
        /// </summary>
        /// <param name="info"></param>
        /// <param name="hotfixVersionDirectory"></param>
        private static string CopyBundleSheetFile(BuildInfo info, string hotfixVersionDirectory)
        {
            var path = Path.Combine(Application.dataPath, typeof(Resources).Name, typeof(XBundleSheet).Name.ToLower() + ".bytes");
            XRes.Save(path, true, info.GetVersion());

            var outputDirectory = info.outputDirectory;
            var json = Path.Combine(outputDirectory, typeof(XBundleSheet).Name.ToLower() + ".bytes");
            if (File.Exists(json))
                File.Delete(json);

            File.Copy(path, json);
            return json;
        }

        /// <summary>
        /// generate bundle sheet file
        /// </summary>
        /// <param name="hotfixVersionDirectory"></param>
        /// <param name="assetBundleManifest"></param>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static string GenerateBundleSheet(string hotfixVersionDirectory, AssetBundleManifest assetBundleManifest, BuildInfo buildInfo)
        {
            // reset old sheet
            XRes.ResetBundleSheet();

            // get all bundles
            var bundles = assetBundleManifest.GetAllAssetBundles();
            foreach (var assetBundle in bundles)
            {
                if (TryGetBundleFiles(assetBundle, out var files, out var entryType))
                {
                    List<string> dependencies = new List<string>();
                    dependencies.AddRange(assetBundleManifest.GetAllDependencies(assetBundle));

                    if (dependencies.Contains(assetBundle))
                        dependencies.Remove(assetBundle);

                    // get file info
                    if (TryGetFileInfo(Path.Combine(hotfixVersionDirectory, assetBundle),
                        out uint checksum, out uint size))
                    {
                        var entry = new Entry()
                        {
                            entryType = entryType,
                            locationType = LocationType.Streaming,
                            name = assetBundle,
                            files = files.ToArray(),
                            dependencies = dependencies.ToArray(),
                            checksum = checksum,
                            size = size,
                        };

                        XRes.AddEntry(entry);
                    }
                }
            }

            // process nobundle resources
            ProcessNobundleResources();

            // copy bundle sheet file
            return CopyBundleSheetFile(buildInfo, hotfixVersionDirectory);
        }

        /// <summary>
        /// generate zip package
        /// </summary>
        /// <param name="hotfixVersionDirectory"></param>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static bool GenerateReleasePackage(string hotfixVersionDirectory, BuildInfo buildInfo,
            string bundleSheetFilePath, string installPackageFile)
        {
            string tempDir = Path.Combine(buildInfo.outputDirectory, "temp");
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            string releasesDir = Path.Combine(buildInfo.outputDirectory, "releases");
            if (!Directory.Exists(releasesDir))
                Directory.CreateDirectory(releasesDir);

            string zipFilePath = Path.Combine(releasesDir, buildInfo.GetVersion() + ".zip");
            if (File.Exists(zipFilePath))
                File.Delete(zipFilePath);

            string zipDir = Path.Combine(tempDir, $"{buildInfo.buildTarget}/{buildInfo.GetAOTVersion()}/hotfix/{buildInfo.GetHotfixVersion()}");
            if (Directory.Exists(zipDir))
                Directory.Delete(zipDir);
            Directory.CreateDirectory(zipDir);

            CopyDirectory(hotfixVersionDirectory, zipDir, true);

            // copy bundle sheet file to zip directory
            string compressionDirectory = Path.Combine(tempDir, buildInfo.buildTarget.ToString());
            File.Copy(bundleSheetFilePath, 
                Path.Combine(compressionDirectory, buildInfo.GetAOTVersion(), Path.GetFileName(bundleSheetFilePath)), true);

            // if build installer, copy it to zip directory
            if (buildInfo.buildTarget == BuildTarget.Android || buildInfo.buildTarget == BuildTarget.iOS)
            {
                if (!string.IsNullOrEmpty(installPackageFile) && !buildInfo.onlyBundle)
                {
                    string installDir = Path.Combine(compressionDirectory, "install");
                    if (!Directory.Exists(installDir))
                        Directory.CreateDirectory(installDir);

                    if (File.Exists(installPackageFile))
                    {
                        File.Copy(installPackageFile,
                            Path.Combine(installDir, Path.GetFileName(installPackageFile)), true);
                    }
                }
            }

            // create zip file
            System.IO.Compression.ZipFile.CreateFromDirectory(compressionDirectory, zipFilePath,
                System.IO.Compression.CompressionLevel.Fastest, true);

            Directory.Delete(tempDir, true);

            return true;
        }

        /// <summary>
        /// build bundles
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static bool BuildBundles(BuildInfo buildInfo)
        {
            string buildOutputDirectory = Path.Combine(buildInfo.outputDirectory);
            if (string.IsNullOrEmpty(buildOutputDirectory))
                return false;

            // for clear output directory
            if (buildInfo.clearFolders)
            {
                if (Directory.Exists(buildOutputDirectory))
                    Directory.Delete(buildOutputDirectory, true);
            }

            // create output directory
            if (!Directory.Exists(buildOutputDirectory))
                Directory.CreateDirectory(buildOutputDirectory);

            // create hotfix version directory
            string hotfixVersion = string.Format("{0}.{1}",
                buildInfo.srcVersion, buildInfo.resVersion);
            string hotfixVersionDirectory = Path.Combine(buildOutputDirectory, "hotfix", hotfixVersion);
            if (!Directory.Exists(hotfixVersionDirectory))
                Directory.CreateDirectory(hotfixVersionDirectory);

            // build asset bundles
            var buildManifest = BuildPipeline.BuildAssetBundles(hotfixVersionDirectory, buildInfo.options, buildInfo.buildTarget);
            if (buildManifest)
            {
                // if copy to streaming assets
                if (buildInfo.copyToStreaming)
                {
                    if (Directory.Exists(Application.streamingAssetsPath))
                        Directory.Delete(Application.streamingAssetsPath, true);

                    CopyDirectory(hotfixVersionDirectory, Application.streamingAssetsPath);
                }

                // generate bundle sheet file
                string bundleSheetFilePath = GenerateBundleSheet(hotfixVersionDirectory, buildManifest, buildInfo);
                if (!string.IsNullOrEmpty(bundleSheetFilePath))
                {
                    // we must save version info, the file will be used in hotfix module
                    var versionJsonFilePath = Path.Combine(buildOutputDirectory, typeof(XHotfixVersion).Name.ToLower() + ".json");
                    buildInfo.SaveVersion(versionJsonFilePath);

                    // generate install package
                    string installPackageFile = GenericInstallPackage(buildInfo);
                    // generate release package
                    if (buildInfo.zipHotfixPackage)
                        GenerateReleasePackage(hotfixVersionDirectory, buildInfo, bundleSheetFilePath, installPackageFile);
                }
            }

            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// generic install package
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static string GenericInstallPackage(BuildInfo buildInfo)
        {
            if (buildInfo.onlyBundle)
                return string.Empty;
            
            var scenes = ProcessScenes();
            scenes = FilterBundleScenes(scenes, buildInfo);

            BuildOptions buildOptions = BuildOptions.None;
            if (buildInfo.development)
                buildOptions |= BuildOptions.Development;

            if (buildInfo.connectProfiler)
                buildOptions |= BuildOptions.ConnectWithProfiler;

            if (buildInfo.allowDebugging)
                buildOptions |= BuildOptions.AllowDebugging;

            return GenericBuild(scenes.ToArray(), 
                buildInfo.outputDirectory, buildInfo, buildOptions);
        }

        /// <summary>
        /// process scenes
        /// </summary>
        /// <returns></returns>
        public static List<string> ProcessScenes()
        {
            List<string> defaultScenes = new List<string>();
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene setting = EditorBuildSettings.scenes[i];
                defaultScenes.Add(setting.path);
            }

            return defaultScenes;
        }

        /// <summary>
        /// 过滤已进 bundle 的场景：useBundle 打包时运行时按 XBundleSource.Bundle 从 bundle 加载场景，
        /// Build Settings 中被 bundle 覆盖的场景不再打进 player，避免同一场景双份计入包体
        /// </summary>
        /// <param name="scenes">ProcessScenes 采集的场景列表</param>
        /// <param name="buildInfo">构建信息</param>
        /// <returns>过滤后的场景列表；sheet 缺失或解析失败时原样返回（fail-safe 不过滤）</returns>
        private static List<string> FilterBundleScenes(List<string> scenes, BuildInfo buildInfo)
        {
            if (!buildInfo.useBundle || scenes == null || scenes.Count == 0)
            {
                return scenes;
            }

            // 与 CopyBundleSheetFile 写出的运行时注册表同源，保证过滤口径与运行时加载口径一致
            var sheetPath = Path.Combine(Application.dataPath,
                typeof(Resources).Name, typeof(XBundleSheet).Name.ToLower() + ".bytes");
            if (!File.Exists(sheetPath))
            {
                Debug.LogWarning("[BundleBuilderEditor] bundle sheet not found, keep all scenes in player build");
                return scenes;
            }

            if (!BundleSheetInfo.TryParse(File.ReadAllText(sheetPath), out var sheet))
            {
                Debug.LogWarning("[BundleBuilderEditor] bundle sheet parse failed, keep all scenes in player build");
                return scenes;
            }

            // 收集 bundle 清单里的全部资产路径（小写、正斜杠，与 sheet 键一致）
            var bundledFiles = new HashSet<string>();
            foreach (var entry in sheet.entrys)
            {
                if (!entry || entry.files == null)
                    continue;

                foreach (var file in entry.files)
                {
                    if (!file || string.IsNullOrEmpty(file.file))
                        continue;

                    bundledFiles.Add(file.file.ToLowerInvariant().Replace('\\', '/'));
                }
            }

            var filtered = new List<string>();
            foreach (var scene in scenes)
            {
                if (bundledFiles.Contains(scene.ToLowerInvariant().Replace('\\', '/')))
                    Debug.Log($"[BundleBuilderEditor] exclude bundled scene from player build: {scene}");
                else
                    filtered.Add(scene);
            }

            // 过滤后必须至少保留一个启动场景，否则放弃过滤维持旧行为
            if (filtered.Count == 0)
            {
                Debug.LogWarning("[BundleBuilderEditor] scene filter would remove all scenes, keep all scenes in player build");
                return scenes;
            }

            return filtered;
        }

        /// <summary>
        /// 构建安装包；Bundle 模式使用本次总清单保留资源依赖的类型。
        /// </summary>
        /// <param name="scenes">安装包内置场景列表</param>
        /// <param name="targetDir">构建输出目录</param>
        /// <param name="buildInfo">构建平台与版本配置</param>
        /// <param name="option">安装包构建选项</param>
        /// <returns>安装包输出路径</returns>
        public static string GenericBuild(string[] scenes, string targetDir, BuildInfo buildInfo, BuildOptions option)
        {
            AssetDatabase.Refresh();

            // if current build target group not match, switch it
            var curTargetgroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (curTargetgroup != BuildPipeline.GetBuildTargetGroup(buildInfo.buildTarget))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup, buildInfo.buildTarget);
            }

            // get extension name
            string extension = "exe";
            switch(buildInfo.buildTarget)
            {
                case BuildTarget.Android:
                    // 按勾选决定是否出 AAB；该全局开关每次构建显式设置，打完不还原
                    EditorUserBuildSettings.buildAppBundle = buildInfo.buildAppBundle;
                    extension = buildInfo.buildAppBundle ? "aab" : "apk";
                    break;
                case BuildTarget.iOS:
                    extension = "ipa";
                    break;
            }

            // build player
            string outputFile = $"{targetDir}/install/{buildInfo.GetVersion()}.{extension}";
            var playerOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputFile,
                target = buildInfo.buildTarget,
                options = option
            };

            if (buildInfo.useBundle)
            {
                string hotfixVersion = buildInfo.GetHotfixVersion();
                string manifestPath = Path.GetFullPath(Path.Combine(targetDir, "hotfix",
                    hotfixVersion, hotfixVersion + ".manifest"));
                if (!File.Exists(manifestPath))
                {
                    throw new UnityEditor.Build.BuildFailedException(
                        $"Bundle 总清单不存在，无法保留 Bundle 所需类型，请先重新构建 Bundle：{manifestPath}");
                }

                // 仅存在于 Bundle 的组件和引擎类型不会被内置场景引用，必须通过总清单告知裁剪器。
                playerOptions.assetBundleManifestPath = manifestPath;
            }

            BuildPipeline.BuildPlayer(playerOptions);

            return outputFile;
        }
    }

    /// <summary>
    /// window mode
    /// </summary>
    public class BundleBuilderEditor : IBundleEditor
    {
        /// <summary>
        /// parent window
        /// </summary>
        public BundleEditor window
        { get; private set; }

        /// <summary>
        /// builder setting
        /// </summary>
        public BuildInfo buildInfo
        { get; private set; }

        /// <summary>
        /// build asset bundle options foldout
        /// </summary>
        private bool buildAssetBundleOptions = true;
        private bool debugSettings = false;
        private bool cdnSettings = false;
        private bool versionSettings = true;
        private bool buildSettings = true;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="parent"></param>
        public BundleBuilderEditor(BundleEditor parent)
        {
            window = parent;
        }
        /// <summary>
        /// enable window
        /// </summary>
        /// <param name="rect"></param>
        public void OnEnable(Rect rect)
        {
            buildInfo = ReadBuildInfo();
        }

        /// <summary>
        /// disable window
        /// </summary>
        public void OnDisable()
        {
            SaveBuildInfo();
        }

        /// <summary>
        /// read build info, if not exists, create new one
        /// </summary>
        /// <returns></returns>
        private BuildInfo ReadBuildInfo()
        {
            var filepath = Path.Combine(Application.dataPath, typeof(BuildInfo).Name);
            if (File.Exists(filepath))
                return JsonUtility.FromJson<BuildInfo>(File.ReadAllText(filepath));

            return new BuildInfo();
        }

        /// <summary>
        /// save build info
        /// </summary>
        private void SaveBuildInfo()
        {
            if (buildInfo != null)
            {
                var json = JsonUtility.ToJson(buildInfo);
                File.WriteAllText(Path.Combine(Application.dataPath, typeof(BuildInfo).Name), json);
            }
        }

        /// <summary>
        /// override gui
        /// </summary>
        /// <param name="rect"></param>
        public void OnGUI(Rect rect)
        { 
            if (buildInfo != null)
            {
                buildInfo.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup(typeof(BuildTarget).Name, buildInfo.buildTarget);
               
                EditorGUILayout.Space(2);
                buildAssetBundleOptions = EditorGUILayout.Foldout(buildAssetBundleOptions, typeof(BuildAssetBundleOptions).Name);
                if (buildAssetBundleOptions)
                {
                    var indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;

                    string[] names = Enum.GetNames(typeof(BuildAssetBundleOptions));
                    foreach (string name in names)
                    {
                        BuildAssetBundleOptions option = (BuildAssetBundleOptions)Enum.Parse(typeof(BuildAssetBundleOptions), name);
                        bool hasOption = (buildInfo.options & option) == option;
                        bool newHasOption = EditorGUILayout.ToggleLeft(name, hasOption);
                        if (newHasOption != hasOption)
                        {
                            if (newHasOption)
                                buildInfo.options |= option;
                            else
                                buildInfo.options &= ~option;
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = indent;
                }

                EditorGUILayout.Space(2);
                versionSettings = EditorGUILayout.Foldout(versionSettings, "Version Settings");
                if (versionSettings)
                {
                    var versionIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;

                    buildInfo.appVersion = EditorGUILayout.IntField("App Version", buildInfo.appVersion);
                    buildInfo.channel = (NewChannel)EditorGUILayout.EnumPopup("Channel", buildInfo.channel);
                    buildInfo.paralleVersion = EditorGUILayout.IntField("Paralle Version", buildInfo.paralleVersion);
                    buildInfo.aotVersion = EditorGUILayout.IntField("AOT Version", buildInfo.aotVersion);
                    buildInfo.srcVersion = EditorGUILayout.IntField("Scripts Version", buildInfo.srcVersion);
                    buildInfo.resVersion = EditorGUILayout.IntField("Art Version", buildInfo.resVersion);

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = versionIndent;
                }

                buildInfo.outputDirectory = $"{buildInfo.buildTarget}/{buildInfo.GetAOTVersion()}";

                EditorGUILayout.Space(2);
                debugSettings = EditorGUILayout.Foldout(debugSettings, "Debug Setting");
                if (debugSettings)
                {
                    var debugIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    buildInfo.connectProfiler = EditorGUILayout.ToggleLeft("Connect Profiler", buildInfo.connectProfiler);
                    buildInfo.development = EditorGUILayout.ToggleLeft("Development Build", buildInfo.development);
                    buildInfo.allowDebugging = EditorGUILayout.ToggleLeft("Allow Debugging", buildInfo.allowDebugging);

                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = debugIndent;
                }
             
                EditorGUILayout.Space(2);
                
                buildSettings = EditorGUILayout.Foldout(buildSettings, "Build Settings");
                if (buildSettings)
                {
                    var buildIndent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    buildInfo.clearFolders = EditorGUILayout.ToggleLeft("Clear Folders", buildInfo.clearFolders);
                    buildInfo.copyToStreaming = EditorGUILayout.ToggleLeft("Copy To StreamingAssets", buildInfo.copyToStreaming);
                    buildInfo.useBundle = EditorGUILayout.ToggleLeft("Use Bundle", buildInfo.useBundle);
                    if (buildInfo.buildTarget == BuildTarget.Android)
                    {
                        buildInfo.buildAppBundle = EditorGUILayout.ToggleLeft("Build App Bundle (AAB)", buildInfo.buildAppBundle);
                    }
                    buildInfo.onlyBundle = EditorGUILayout.ToggleLeft("Only Bundle", buildInfo.onlyBundle);
                    buildInfo.zipHotfixPackage = EditorGUILayout.ToggleLeft("Zip Hotfix Package", buildInfo.zipHotfixPackage);
                    buildInfo.includeInstaller = EditorGUILayout.ToggleLeft("Include installer", buildInfo.includeInstaller);
                    EditorGUILayout.Space();
                    EditorGUI.indentLevel = buildIndent;
                }

                EditorGUILayout.Space(2);
                cdnSettings = EditorGUILayout.Foldout(cdnSettings, "CND Setting");
                if (cdnSettings)
                {
                    for (int i = 0; i < buildInfo.cdns.Length; i++)
                    {
                        buildInfo.cdns[i] = EditorGUILayout.TextField($"{i}", buildInfo.cdns[i]);
                    }
                }

                buildInfo.authUrl = EditorGUILayout.TextField("Auth URL", buildInfo.authUrl);
                buildInfo.payUrl = EditorGUILayout.TextField("Pay URL", buildInfo.payUrl);

                LayoutBuild();
            }
        }

        /// <summary>
        /// layout build
        /// </summary>
        private void LayoutBuild()
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Out put directory:", buildInfo.outputDirectory);
            if (GUILayout.Button("Build"))
            {
                SaveBuildInfo();

                bool succeed = BundleBuilderEditorUtility.BuildBundles(buildInfo);
                if (succeed)
                {
                    OnReload();
                }
            }
        }

        /// <summary>
        /// on reload
        /// </summary>
        public void OnReload()
        {
        }

        /// <summary>
        /// on update
        /// </summary>
        public void OnUpdate()
        { }
    }
}
