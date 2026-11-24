using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Oathx.UnityCLI.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UnityCLITools.Editor
{
    /// <summary>
    /// Editor window used to install, monitor, and configure the UnityCLI bridge.
    /// </summary>
    public class UnityCLIWindow : EditorWindow
    {
        /// <summary>
        /// Project-relative path where the built CLI executable is installed.
        /// </summary>
        const string ExeRelativePath    = "Library/UnityCLI/UnityCLI.exe";
        /// <summary>
        /// Build configuration used for the standalone CLI project.
        /// </summary>
        const string BuildConfiguration = "Release";

        /// <summary>
        /// Package folder that contains the standalone CLI source project.
        /// </summary>
        const string UnityCliFolderName = "UnityCLI~";
        /// <summary>
        /// Path to the CLI csproj inside <see cref="UnityCliFolderName"/>.
        /// </summary>
        const string UnityCliProjectSubPath = "UnityCLI/UnityCLI.csproj";
        /// <summary>
        /// Stores the bundled UnityCLI skill folder name.
        /// </summary>
        const string UnityCliSkillFolderName = "unity-cli";
        /// <summary>
        /// Stores the Codex home environment variable name.
        /// </summary>
        const string CodexHomeEnvironmentVariable = "CODEX_HOME";
        /// <summary>
        /// Stores the UnityCLI local configuration folder name under the user profile.
        /// </summary>
        const string UnityCliUserConfigFolderName = ".unitycli";
        /// <summary>
        /// Stores the Jira local configuration file name.
        /// </summary>
        const string JiraConfigFileName = "jira.json";
        /// <summary>
        /// Stores the minimum available commands scroll height.
        /// </summary>
        const float MinimumCommandsScrollHeight = 120f;
        /// <summary>
        /// Stores the fixed adjustment subtracted from the commands scroll height.
        /// </summary>
        const float CommandsScrollHeightAdjustment = 84f;
        /// <summary>
        /// Stores the inner padding used by the commands scroll frame.
        /// </summary>
        const float CommandsScrollFramePadding = 4f;

        // Resolved once at type init from this script's own location via [CallerFilePath].
        // Walks up from the directory containing UnityCLIWindow.cs and returns the first
        // ancestor that has a child "UnityCLI~/UnityCLI/UnityCLI.csproj". Returns null if
        // the package layout no longer exposes the source project (e.g. the package was
        // installed without the UnityCLI~ folder).
        /// <summary>
        /// Resolved absolute path to the standalone CLI csproj.
        /// </summary>
        static readonly string ResolvedCsProjPath = ResolveCsProjPath();
        /// <summary>
        /// Resolved absolute path to the bundled unity-cli skill folder.
        /// </summary>
        static readonly string ResolvedUnityCliSkillPath = ResolveBundledUnityCliSkillPath();

        /// <summary>
        /// Describes one bridge command shown in the editor window.
        /// </summary>
        struct CommandInfo
        {
            public string Name;
            public string Method;
            public string Route;
            public string Description;
        }

        static readonly CommandInfo[] KnownCommands =
        {
            new CommandInfo { Name = "ping",    Method = "GET",  Route = "/ping",
                Description = "Health check; returns endpoint metadata and the routes the bridge exposes." },
            new CommandInfo { Name = "compile", Method = "POST", Route = "/compile",
                Description = "Request a script recompile. Use --wait to block until isCompiling == false." },
            new CommandInfo { Name = "console", Method = "GET",  Route = "/console",
                Description = "Read recent Console entries; filter by --type and --search." },
            new CommandInfo { Name = "search",  Method = "GET",  Route = "/search",
                Description = "Find loaded-scene GameObjects by name or wildcard, with components." },
            new CommandInfo { Name = "modify",  Method = "POST", Route = "/modify",
                Description = "Set a SerializedProperty on a component (works in Edit Mode)." },
            new CommandInfo { Name = "run",     Method = "POST", Route = "/run",
                Description = "Drive Play Mode: play, pause, resume, stop." },
            new CommandInfo { Name = "call",    Method = "POST", Route = "/call",
                Description = "Play-Mode only: reflection-invoke a static method or a unique scene-instance method." },
            new CommandInfo { Name = "save",    Method = "POST", Route = "/save",
                Description = "Save dirty open scenes + AssetDatabase.SaveAssets (mirrors File > Save). Use --scene to target one." },
            new CommandInfo { Name = "components", Method = "POST", Route = "/components",
                Description = "Add or remove a Component on a scene GameObject (Undo-aware). action=add|remove, --component=<type>." },
            new CommandInfo { Name = "asset",   Method = "POST", Route = "/asset",
                Description = "Manage AssetDatabase: search / create-folder / import / delete / move / copy." },
            new CommandInfo { Name = "ugui",    Method = "POST", Route = "/ugui",
                Description = "Build or update UGUI prefab from unity-cli visual-ui.json and asset-manifest.json." },
            new CommandInfo { Name = "widget-metadata", Method = "POST", Route = "/widget-metadata",
                Description = "Generate Oathx.Widget Metadata on an existing prefab when com.oathx.widget is installed." },
            new CommandInfo { Name = "prefab", Method = "POST", Route = "/prefab",
                Description = "Attach one prefab as a nested child inside another prefab asset, with optional open-scene sync." },
            new CommandInfo { Name = "screenshot", Method = "POST", Route = "/screenshot",
                Description = "Capture the current Game view to a PNG under a project-root folder beside Assets." },
        };

        Process buildProcess;
        bool isBuilding;
        int? lastExitCode;
        DateTime buildStartedUtc;

        Process pingProcess;
        bool isPinging;
        /// <summary>
        /// Captures stdout from the background ping process.
        /// </summary>
        readonly StringBuilder pingStdout = new StringBuilder();
        /// <summary>
        /// Protects ping process output state.
        /// </summary>
        readonly object pingLock = new object();
        HashSet<string> liveRoutes;
        string pingError;
        DateTime? lastRefreshUtc;

        bool commandsFoldout = true;
        bool jiraFoldout = true;
        string jiraSite = string.Empty;
        string jiraToken = string.Empty;
        string jiraStatusMessage = string.Empty;
        /// <summary>
        /// Scroll position for the available command list.
        /// </summary>
        Vector2 commandsScrollPosition;

        /// <summary>
        /// Opens the UnityCLI editor window.
        /// </summary>
        [MenuItem("Tools/UnityCLI/Window")]
        public static void Open()
        {
            var window = GetWindow<UnityCLIWindow>("UnityCLI");
            window.minSize = new Vector2(520, 320);
            window.Show();
        }

        void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.delayCall += AutoRefreshIfInstalled;
            LoadJiraConfigIntoFields();
        }

        void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            TryKillBuild();
            TryKillPing();
        }

        void AutoRefreshIfInstalled()
        {
            if (this == null) return;
            var exePath = Path.Combine(ProjectRoot, NormalizeRel(ExeRelativePath));
            if (File.Exists(exePath) && liveRoutes == null && !isPinging)
            {
                StartPing();
            }
        }

        void OnEditorUpdate()
        {
            var dirty = false;

            if (isBuilding && buildProcess != null && buildProcess.HasExited)
            {
                int exitCode;
                try { exitCode = buildProcess.ExitCode; }
                catch { exitCode = -1; }

                lastExitCode = exitCode;
                isBuilding = false;

                if (exitCode == 0)
                {
                    Debug.Log("[UnityCLI] dotnet build completed (exit 0)");
                }
                else
                {
                    Debug.LogError("[UnityCLI] dotnet build exited with code " + exitCode);
                }

                try { buildProcess.Dispose(); } catch { }
                buildProcess = null;
                AssetDatabase.Refresh();

                if (exitCode == 0)
                {
                    EditorApplication.delayCall += StartPing;
                }
                dirty = true;
            }

            if (isPinging && pingProcess != null && pingProcess.HasExited)
            {
                int exitCode;
                try { exitCode = pingProcess.ExitCode; }
                catch { exitCode = -1; }

                string output;
                lock (pingLock) { output = pingStdout.ToString(); }

                if (exitCode == 0)
                {
                    liveRoutes = ParseRoutes(output);
                    pingError = liveRoutes == null
                        ? "ping returned no 'routes:' line"
                        : null;
                }
                else
                {
                    liveRoutes = null;
                    pingError = "ping exit " + exitCode + ": " +
                                FirstNonEmptyLine(output);
                }

                lastRefreshUtc = DateTime.UtcNow;
                isPinging = false;
                try { pingProcess.Dispose(); } catch { }
                pingProcess = null;
                dirty = true;
            }

            if (isBuilding || isPinging || dirty)
            {
                Repaint();
            }
        }

        void OnGUI()
        {
            var projectRoot = ProjectRoot;
            var exePath = Path.Combine(projectRoot, NormalizeRel(ExeRelativePath));
            var csprojPath = ResolvedCsProjPath;
            var csprojAvailable = !string.IsNullOrEmpty(csprojPath) && File.Exists(csprojPath);
            var installed = File.Exists(exePath);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("UnityCLI Installer", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawStatusBox(installed);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Source Project", EditorStyles.miniBoldLabel);
            if (csprojAvailable)
            {
                DrawReadOnlyPath(csprojPath);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Could not locate \"" + UnityCliFolderName + "/" + UnityCliProjectSubPath +
                    "\" by walking up from this script. The UnityCLI source folder may have been " +
                    "removed or this package was installed without it.",
                    MessageType.Warning);
            }

            EditorGUILayout.LabelField("Output Path", EditorStyles.miniBoldLabel);
            DrawReadOnlyPath(exePath);

            EditorGUILayout.Space(8);
            DrawActionButtons(installed, csprojPath, csprojAvailable, exePath);

            EditorGUILayout.Space(8);
            DrawJiraConfigSection();

            EditorGUILayout.Space(8);
            DrawBridgeLoggingRow();

            EditorGUILayout.Space(8);
            DrawCommandsSection(installed);
        }

        /// <summary>
        /// Draws the bridge request/response logging toggle.
        /// </summary>
        static void DrawBridgeLoggingRow()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Bridge Logging",
                        "Mirror every request/response that flows over the named pipe " +
                        "into the Unity Console (CLI -> Editor and Editor -> CLI lines)."),
                    GUILayout.Width(110));

                var current = UnityCliBridgeLogSettings.VerboseLogging;
                var next = EditorGUILayout.ToggleLeft(
                    new GUIContent("Log requests / responses to Console",
                        "When on, every bridge call is echoed to the Console. " +
                        "Turn this off if /console polling is making the log too noisy."),
                    current);

                if (next != current)
                {
                    UnityCliBridgeLogSettings.VerboseLogging = next;
                }
            }
        }

        void DrawStatusBox(bool installed)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Status", GUILayout.Width(50));

                var prevColor = GUI.contentColor;
                string text;
                if (isBuilding)
                {
                    GUI.contentColor = new Color(1f, 0.85f, 0.3f);
                    var elapsed = (DateTime.UtcNow - buildStartedUtc).TotalSeconds;
                    text = string.Format("Installing... ({0:0.0}s)", elapsed);
                }
                else if (installed)
                {
                    GUI.contentColor = new Color(0.4f, 0.85f, 0.4f);
                    text = "Installed";
                }
                else
                {
                    GUI.contentColor = new Color(1f, 0.5f, 0.4f);
                    text = "Not Installed";
                }

                EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
                GUI.contentColor = prevColor;
            }
        }

        void DrawActionButtons(bool installed, string csprojPath, bool csprojAvailable, string exePath)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(isBuilding || !csprojAvailable))
                {
                    var label = installed ? "Reinstall" : "Install UnityCLI";
                    if (GUILayout.Button(label))
                    {
                        StartInstall(csprojPath);
                    }
                }

                using (new EditorGUI.DisabledScope(!Directory.Exists(ResolvedUnityCliSkillPath)))
                {
                    var content = new GUIContent("Skills",
                        "Install the bundled unity-cli skill into the Codex skills directory.");
                    if (GUILayout.Button(content))
                    {
                        InstallBundledUnityCliSkill();
                    }
                }
            }
        }

        void DrawJiraConfigSection()
        {
            jiraFoldout = EditorGUILayout.Foldout(jiraFoldout,
                "JIRA Config",
                true,
                EditorStyles.foldoutHeader);

            if (!jiraFoldout) return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox(
                    "Saved to the current Windows user profile, not to the Unity project. Jira CLI only needs the Jira address and access token.",
                    MessageType.Info);

                EditorGUILayout.LabelField("Config Path", EditorStyles.miniBoldLabel);
                DrawReadOnlyPath(GetJiraConfigPath());

                jiraSite = EditorGUILayout.TextField(
                    new GUIContent("Jira Address", "Example: http://jira.company.com:8080"),
                    jiraSite ?? string.Empty);
                jiraToken = EditorGUILayout.PasswordField(
                    new GUIContent("Access Token", "Jira personal access token. Stored only in the local user config file."),
                    jiraToken ?? string.Empty);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Save Jira Config"))
                    {
                        SaveJiraConfigFromFields();
                    }

                    if (GUILayout.Button("Reload"))
                    {
                        LoadJiraConfigIntoFields();
                        jiraStatusMessage = "Reloaded Jira config.";
                    }

                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(jiraToken)))
                    {
                        if (GUILayout.Button("Clear Token"))
                        {
                            jiraToken = string.Empty;
                            SaveJiraConfigFromFields();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Open Config Folder"))
                    {
                        var folder = Path.GetDirectoryName(GetJiraConfigPath());
                        if (!string.IsNullOrEmpty(folder))
                        {
                            Directory.CreateDirectory(folder);
                            EditorUtility.RevealInFinder(folder);
                        }
                    }

                    if (GUILayout.Button("Copy Auth Command"))
                    {
                        EditorGUIUtility.systemCopyBuffer = "Library\\UnityCLI\\UnityCLI.exe jira auth";
                        jiraStatusMessage = "Copied: Library\\UnityCLI\\UnityCLI.exe jira auth";
                    }
                }

                if (!string.IsNullOrEmpty(jiraStatusMessage))
                {
                    EditorGUILayout.HelpBox(jiraStatusMessage, MessageType.None);
                }
            }
        }

        void LoadJiraConfigIntoFields()
        {
            var config = LoadJiraConfig();
            jiraSite = config.Site ?? string.Empty;
            jiraToken = config.ApiToken ?? string.Empty;
        }

        void SaveJiraConfigFromFields()
        {
            try
            {
                var config = new JiraEditorConfig
                {
                    Site = NormalizeJiraSite(jiraSite),
                    ApiToken = jiraToken ?? string.Empty
                };

                if (string.IsNullOrWhiteSpace(config.Site))
                {
                    EditorUtility.DisplayDialog("UnityCLI Jira",
                        "Please enter a Jira address, for example:\nhttp://jira.company.com:8080",
                        "OK");
                    return;
                }

                SaveJiraConfig(config);
                jiraSite = config.Site;
                jiraToken = config.ApiToken;
                jiraStatusMessage = "Saved Jira config.";
                Debug.Log("[UnityCLI] Saved Jira config to " + GetJiraConfigPath());
            }
            catch (Exception ex)
            {
                jiraStatusMessage = "Failed to save Jira config: " + ex.Message;
                Debug.LogError("[UnityCLI] Failed to save Jira config: " + ex);
            }
        }

        /// <summary>
        /// Draws a disabled text field for a resolved file-system path.
        /// </summary>
        /// <param name="path">The path text to display.</param>
        static void DrawReadOnlyPath(string path)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(path ?? string.Empty,
                    GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        // Cached so we don't allocate a new GUIContent every OnGUI frame.
        // EditorGUIUtility.IconContent picks the dark/light variant of the built-in
        // "_Refresh" texture automatically; nulled out and re-fetched if the cached
        // image was ever unloaded (rare, but happens across some domain reloads).
        /// <summary>
        /// Cached refresh icon content reused across OnGUI calls.
        /// </summary>
        static GUIContent _refreshIconCache;

        /// <summary>
        /// Gets the icon or fallback label used by the command refresh button.
        /// </summary>
        /// <param name="busy">Whether the command list is currently refreshing.</param>
        /// <returns>A GUIContent instance for the refresh button.</returns>
        static GUIContent GetRefreshButtonContent(bool busy)
        {
            if (_refreshIconCache == null || _refreshIconCache.image == null)
            {
                _refreshIconCache = EditorGUIUtility.IconContent("Refresh")
                                    ?? EditorGUIUtility.IconContent("d_Refresh");
            }

            var tooltip = busy ? "Refreshing..." : "Refresh from Bridge";

            if (_refreshIconCache != null && _refreshIconCache.image != null)
            {
                return new GUIContent(_refreshIconCache.image, tooltip);
            }

            // Icon failed to load - fall back to a unicode glyph so the button is still usable.
            return new GUIContent("\u21BB", tooltip);
        }

        void DrawCommandsSection(bool installed)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                commandsFoldout = EditorGUILayout.Foldout(commandsFoldout,
                    "Available Commands", true, EditorStyles.foldoutHeader);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Enable All", EditorStyles.miniButtonLeft, GUILayout.Width(90)))
                {
                    foreach (var c in KnownCommands) UnityCliCommandSettings.SetEnabled(c.Route, true);
                }
                if (GUILayout.Button("Disable All", EditorStyles.miniButtonRight, GUILayout.Width(90)))
                {
                    foreach (var c in KnownCommands) UnityCliCommandSettings.SetEnabled(c.Route, false);
                }

                GUILayout.Space(6);

                using (new EditorGUI.DisabledScope(!installed || isPinging))
                {
                    if (GUILayout.Button(GetRefreshButtonContent(isPinging),
                            GUILayout.Width(28), GUILayout.Height(20)))
                    {
                        StartPing();
                    }
                }
            }

            if (!commandsFoldout) return;

            if (!installed)
            {
                EditorGUILayout.HelpBox(
                    "UnityCLI is not installed. Install it first to verify which routes the bridge exposes.",
                    MessageType.Info);
            }
            else if (pingError != null)
            {
                EditorGUILayout.HelpBox("Refresh failed: " + pingError, MessageType.Warning);
            }
            else if (liveRoutes == null)
            {
                EditorGUILayout.HelpBox(
                    "Routes have not been refreshed yet. Click \"Refresh from Bridge\" to query the running Editor.",
                    MessageType.None);
            }
            else if (lastRefreshUtc.HasValue)
            {
                var ago = (DateTime.UtcNow - lastRefreshUtc.Value).TotalSeconds;
                EditorGUILayout.LabelField(
                    string.Format("Last refresh: {0:0}s ago - {1} live route(s)",
                        ago, liveRoutes.Count),
                    EditorStyles.miniLabel);
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawCommandHeader();
                var scrollHeight = GetRemainingCommandsScrollHeight();
                DrawCommandsScrollView(scrollHeight);
            }
        }

        /// <summary>
        /// Draws the available commands scroll view inside an explicit visible frame.
        /// </summary>
        /// <param name="height">The frame height.</param>
        void DrawCommandsScrollView(float height)
        {
            var innerHeight = height - 250f;
            using (new EditorGUILayout.VerticalScope(
                       EditorStyles.helpBox,
                       GUILayout.Height(innerHeight),
                       GUILayout.ExpandWidth(true)))
            {
                commandsScrollPosition = EditorGUILayout.BeginScrollView(
                    commandsScrollPosition,
                    false,
                    true,
                    GUILayout.Height(innerHeight));
                try
                {
                    foreach (var cmd in KnownCommands)
                    {
                        DrawCommandRow(cmd);
                    }
                }
                finally
                {
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        /// <summary>
        /// Gets the remaining height available for the commands scroll view.
        /// </summary>
        /// <returns>The remaining commands scroll height.</returns>
        float GetRemainingCommandsScrollHeight()
        {
            var currentY = GUILayoutUtility.GetLastRect().yMax;
            if (currentY <= 0f)
            {
                currentY = 0f;
            }

            var availableHeight = position.height - currentY - CommandsScrollHeightAdjustment;
            return Mathf.Max(MinimumCommandsScrollHeight, availableHeight);
        }

        void DrawCommandHeader()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("On",      EditorStyles.miniBoldLabel, GUILayout.Width(28));
                EditorGUILayout.LabelField("Command", EditorStyles.miniBoldLabel, GUILayout.Width(80));
                EditorGUILayout.LabelField("Method",  EditorStyles.miniBoldLabel, GUILayout.Width(56));
                EditorGUILayout.LabelField("Route",   EditorStyles.miniBoldLabel, GUILayout.Width(90));
                EditorGUILayout.LabelField("Status",  EditorStyles.miniBoldLabel, GUILayout.Width(64));
                EditorGUILayout.LabelField("Description", EditorStyles.miniBoldLabel);
            }
        }

        void DrawCommandRow(CommandInfo cmd)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var enabled = UnityCliCommandSettings.IsEnabled(cmd.Route);
                var newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(28));
                if (newEnabled != enabled)
                {
                    UnityCliCommandSettings.SetEnabled(cmd.Route, newEnabled);
                    enabled = newEnabled;
                }

                EditorGUILayout.LabelField(cmd.Name, EditorStyles.boldLabel, GUILayout.Width(80));
                EditorGUILayout.LabelField(cmd.Method, GUILayout.Width(56));
                EditorGUILayout.LabelField(cmd.Route,  GUILayout.Width(90));

                var prev = GUI.contentColor;
                string statusText;
                if (!enabled)
                {
                    GUI.contentColor = new Color(1f, 0.65f, 0.2f);
                    statusText = "Disabled";
                }
                else if (liveRoutes == null)
                {
                    GUI.contentColor = new Color(0.65f, 0.65f, 0.65f);
                    statusText = "Unknown";
                }
                else
                {
                    var key = (cmd.Method + " " + cmd.Route).ToUpperInvariant();
                    if (liveRoutes.Contains(key))
                    {
                        GUI.contentColor = new Color(0.4f, 0.85f, 0.4f);
                        statusText = "Live";
                    }
                    else
                    {
                        GUI.contentColor = new Color(1f, 0.5f, 0.4f);
                        statusText = "Missing";
                    }
                }
                EditorGUILayout.LabelField(statusText, EditorStyles.miniBoldLabel, GUILayout.Width(64));
                GUI.contentColor = prev;

                EditorGUILayout.LabelField(cmd.Description, EditorStyles.wordWrappedLabel);
            }
        }

        void StartInstall(string csprojPath)
        {
            if (isBuilding) return;

            if (!File.Exists(csprojPath))
            {
                EditorUtility.DisplayDialog("UnityCLI",
                    "UnityCLI.csproj not found:\n" + csprojPath, "OK");
                return;
            }

            lastExitCode = null;
            buildStartedUtc = DateTime.UtcNow;

            var outputPath = Path.Combine(ProjectRoot, NormalizeRel("Library/UnityCLI"));
            Directory.CreateDirectory(outputPath);

            var arguments = "build " + QuoteArgument(csprojPath) +
                " -c " + BuildConfiguration +
                " -nologo -o " + QuoteArgument(outputPath);
            Debug.Log("[UnityCLI] $ dotnet " + arguments + "\nworkdir: " + ProjectRoot);

            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                WorkingDirectory = ProjectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            try
            {
                buildProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                buildProcess.OutputDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.Log("[UnityCLI] " + e.Data);
                    }
                };
                buildProcess.ErrorDataReceived += (_, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Debug.LogError("[UnityCLI] " + e.Data);
                    }
                };
                buildProcess.Start();
                buildProcess.BeginOutputReadLine();
                buildProcess.BeginErrorReadLine();
                isBuilding = true;
            }
            catch (Exception ex)
            {
                Debug.LogError("[UnityCLI] Failed to start dotnet: " + ex.Message +
                    "\nPlease ensure the .NET SDK is installed and on PATH.");
                try { buildProcess?.Dispose(); } catch { }
                buildProcess = null;
                isBuilding = false;
            }
        }

        /// <summary>
        /// Installs the bundled UnityCLI skill into the local Codex skills directory.
        /// </summary>
        void InstallBundledUnityCliSkill()
        {
            var sourcePath = ResolvedUnityCliSkillPath;
            if (!Directory.Exists(sourcePath))
            {
                EditorUtility.DisplayDialog("UnityCLI Skills",
                    "Bundled unity-cli skill was not found:\n" + sourcePath, "OK");
                return;
            }

            var codexSkillsRoot = GetCodexSkillsRoot();
            var destinationPath = Path.Combine(codexSkillsRoot, UnityCliSkillFolderName);
            var sourceFullPath = NormalizeFullPath(sourcePath);
            var destinationFullPath = NormalizeFullPath(destinationPath);
            var temporaryPath = destinationFullPath + ".installing-" + DateTime.UtcNow.Ticks;

            if (string.Equals(sourceFullPath, destinationFullPath, StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("UnityCLI Skills",
                    "Source and destination are the same folder:\n" + sourceFullPath, "OK");
                return;
            }

            try
            {
                Directory.CreateDirectory(codexSkillsRoot);
                DeleteDirectoryIfExists(temporaryPath);
                CopyDirectory(sourceFullPath, temporaryPath);
                DeleteDirectoryIfExists(destinationFullPath);
                Directory.Move(temporaryPath, destinationFullPath);

                Debug.Log("[UnityCLI] Installed unity-cli skill.\nsource: " +
                    sourceFullPath + "\ndestination: " + destinationFullPath);
                EditorUtility.DisplayDialog("UnityCLI Skills",
                    "Installed unity-cli skill to:\n" + destinationFullPath, "OK");
            }
            catch (Exception ex)
            {
                try { DeleteDirectoryIfExists(temporaryPath); } catch { }
                Debug.LogError("[UnityCLI] Failed to install unity-cli skill: " + ex);
                EditorUtility.DisplayDialog("UnityCLI Skills",
                    "Failed to install unity-cli skill:\n" + ex.Message, "OK");
            }
        }

        void StartPing()
        {
            if (isPinging) return;

            var exePath = Path.Combine(ProjectRoot, NormalizeRel(ExeRelativePath));
            if (!File.Exists(exePath))
            {
                pingError = "UnityCLI.exe not found at " + exePath;
                return;
            }

            lock (pingLock) { pingStdout.Clear(); }
            pingError = null;

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = "ping",
                WorkingDirectory = ProjectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            try
            {
                pingProcess = new Process { StartInfo = psi, EnableRaisingEvents = true };
                pingProcess.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        lock (pingLock) { pingStdout.AppendLine(e.Data); }
                    }
                };
                pingProcess.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        lock (pingLock) { pingStdout.AppendLine(e.Data); }
                    }
                };
                pingProcess.Start();
                pingProcess.BeginOutputReadLine();
                pingProcess.BeginErrorReadLine();
                isPinging = true;
            }
            catch (Exception ex)
            {
                pingError = "Failed to start UnityCLI.exe: " + ex.Message;
                try { pingProcess?.Dispose(); } catch { }
                pingProcess = null;
                isPinging = false;
            }
        }

        /// <summary>
        /// Parses route lines returned by the CLI ping command.
        /// </summary>
        /// <param name="pingOutput">The human-readable ping output.</param>
        /// <returns>The normalized route set, or null when no routes line is present.</returns>
        static HashSet<string> ParseRoutes(string pingOutput)
        {
            if (string.IsNullOrEmpty(pingOutput)) return null;

            foreach (var raw in pingOutput.Split('\n'))
            {
                var line = raw.Trim();
                if (!line.StartsWith("routes:", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var payload = line.Substring("routes:".Length).Trim();
                var set = new HashSet<string>();
                foreach (var token in payload.Split(','))
                {
                    var trimmed = token.Trim();
                    if (trimmed.Length > 0)
                    {
                        set.Add(trimmed.ToUpperInvariant());
                    }
                }
                return set;
            }

            return null;
        }

        /// <summary>
        /// Returns the first non-empty line in a block of process output.
        /// </summary>
        /// <param name="text">The text to inspect.</param>
        /// <returns>The first non-empty trimmed line, or an empty string.</returns>
        static string FirstNonEmptyLine(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text
                .Split('\n')
                .Select(l => l.Trim())
                .FirstOrDefault(l => l.Length > 0) ?? string.Empty;
        }

        void TryKillBuild()
        {
            if (buildProcess == null)
            {
                isBuilding = false;
                return;
            }

            try
            {
                if (!buildProcess.HasExited) buildProcess.Kill();
            }
            catch { }

            try { buildProcess.Dispose(); } catch { }
            buildProcess = null;
            isBuilding = false;
        }

        void TryKillPing()
        {
            if (pingProcess == null)
            {
                isPinging = false;
                return;
            }

            try
            {
                if (!pingProcess.HasExited) pingProcess.Kill();
            }
            catch { }

            try { pingProcess.Dispose(); } catch { }
            pingProcess = null;
            isPinging = false;
        }

        /// <summary>
        /// Converts a package-relative path to use the current platform separator.
        /// </summary>
        /// <param name="relative">The slash-separated relative path.</param>
        /// <returns>The relative path with platform-specific separators.</returns>
        static string NormalizeRel(string relative)
        {
            return relative.Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Gets the local Codex skills root path.
        /// </summary>
        /// <returns>The Codex skills root path.</returns>
        static string GetCodexSkillsRoot()
        {
            var codexHome = Environment.GetEnvironmentVariable(CodexHomeEnvironmentVariable);
            if (string.IsNullOrWhiteSpace(codexHome))
            {
                var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                codexHome = Path.Combine(userProfile, ".codex");
            }

            return Path.Combine(codexHome, "skills");
        }

        static string GetJiraConfigPath()
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(userProfile, UnityCliUserConfigFolderName, JiraConfigFileName);
        }

        static JiraEditorConfig LoadJiraConfig()
        {
            try
            {
                var path = GetJiraConfigPath();
                if (!File.Exists(path)) return new JiraEditorConfig();
                return JsonConvert.DeserializeObject<JiraEditorConfig>(File.ReadAllText(path))
                       ?? new JiraEditorConfig();
            }
            catch
            {
                return new JiraEditorConfig();
            }
        }

        static void SaveJiraConfig(JiraEditorConfig config)
        {
            var path = GetJiraConfigPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        static string NormalizeJiraSite(string site)
        {
            site = (site ?? string.Empty).Trim();
            while (site.EndsWith("/", StringComparison.Ordinal))
            {
                site = site.Substring(0, site.Length - 1);
            }

            return site;
        }

        /// <summary>
        /// Copies a directory tree recursively.
        /// </summary>
        /// <param name="sourceDirectory">The source directory path.</param>
        /// <param name="destinationDirectory">The destination directory path.</param>
        static void CopyDirectory(string sourceDirectory, string destinationDirectory)
        {
            Directory.CreateDirectory(destinationDirectory);

            foreach (var filePath in Directory.EnumerateFiles(sourceDirectory))
            {
                var fileName = Path.GetFileName(filePath);
                var destinationFilePath = Path.Combine(destinationDirectory, fileName);
                File.Copy(filePath, destinationFilePath, true);
            }

            foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory))
            {
                var directoryName = Path.GetFileName(directoryPath);
                var destinationSubDirectory = Path.Combine(destinationDirectory, directoryName);
                CopyDirectory(directoryPath, destinationSubDirectory);
            }
        }

        /// <summary>
        /// Deletes a directory tree if it already exists.
        /// </summary>
        /// <param name="directoryPath">The directory path.</param>
        static void DeleteDirectoryIfExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (var subDirectoryPath in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories))
            {
                var attributes = File.GetAttributes(subDirectoryPath);
                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(subDirectoryPath, attributes & ~FileAttributes.ReadOnly);
                }
            }

            foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
            {
                var attributes = File.GetAttributes(filePath);
                if ((attributes & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
                }
            }

            var rootAttributes = File.GetAttributes(directoryPath);
            if ((rootAttributes & FileAttributes.ReadOnly) != 0)
            {
                File.SetAttributes(directoryPath, rootAttributes & ~FileAttributes.ReadOnly);
            }

            Directory.Delete(directoryPath, true);
        }

        /// <summary>
        /// Normalizes a filesystem path to a comparable absolute form.
        /// </summary>
        /// <param name="path">The path to normalize.</param>
        /// <returns>The normalized full path.</returns>
        static string NormalizeFullPath(string path)
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Quotes a command-line argument for ProcessStartInfo argument strings.
        /// </summary>
        /// <param name="argument">The argument value to quote.</param>
        /// <returns>The quoted argument value.</returns>
        static string QuoteArgument(string argument)
        {
            return "\"" + argument.Replace("\"", "\\\"") + "\"";
        }

        /// <summary>
        /// Gets the current Unity project root path.
        /// </summary>
        static string ProjectRoot
        {
            get
            {
                return Path.GetDirectoryName(Application.dataPath)
                       ?? Directory.GetCurrentDirectory();
            }
        }

        // [CallerFilePath] resolves to the absolute path of THIS file at compile time.
        // We then walk up its directory chain looking for a sibling
        // "UnityCLI~/UnityCLI/UnityCLI.csproj". This makes the window agnostic to where
        // the script lives inside the package (Editor/, Editor/Sub/, etc.) and to where
        // the package itself is mounted (LocalPackages/, Packages/, embedded, ...).
        /// <summary>
        /// Resolves the standalone CLI csproj from this source file's location.
        /// </summary>
        /// <param name="scriptPath">Compiler-supplied path to this script.</param>
        /// <returns>The resolved csproj path, or null when the source project is absent.</returns>
        static string ResolveCsProjPath([CallerFilePath] string scriptPath = null)
        {
            if (string.IsNullOrEmpty(scriptPath))
            {
                return null;
            }

            var dir = Path.GetDirectoryName(scriptPath);
            while (!string.IsNullOrEmpty(dir))
            {
                var candidate = Path.Combine(dir,
                    UnityCliFolderName,
                    NormalizeRel(UnityCliProjectSubPath));

                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                var parent = Path.GetDirectoryName(dir);
                if (parent == dir)
                {
                    break;
                }
                dir = parent;
            }

            return null;
        }

        /// <summary>
        /// Resolves the bundled unity-cli skill folder from this source file's location.
        /// </summary>
        /// <param name="scriptPath">Compiler-supplied path to this script.</param>
        /// <returns>The bundled UnityCLI skill path.</returns>
        static string ResolveBundledUnityCliSkillPath([CallerFilePath] string scriptPath = null)
        {
            foreach (var root in GetUnityCliSkillSearchRoots(scriptPath))
            {
                var skillPath = TryResolveUnityCliSkillFromRoot(root);
                if (!string.IsNullOrEmpty(skillPath))
                {
                    return skillPath;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets candidate roots that may contain the bundled unity-cli skill.
        /// </summary>
        /// <param name="scriptPath">Compiler-supplied path to this script.</param>
        /// <returns>Candidate directories to inspect.</returns>
        static IEnumerable<string> GetUnityCliSkillSearchRoots(string scriptPath)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in EnumerateUnityCliSkillSearchRoots(scriptPath))
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                string fullPath;
                try
                {
                    fullPath = Path.GetFullPath(path);
                }
                catch
                {
                    continue;
                }

                if (seen.Add(fullPath))
                {
                    yield return fullPath;
                }
            }
        }

        /// <summary>
        /// Enumerates raw candidate roots that may contain the bundled skill.
        /// </summary>
        /// <param name="scriptPath">Compiler-supplied path to this script.</param>
        /// <returns>Raw candidate paths.</returns>
        static IEnumerable<string> EnumerateUnityCliSkillSearchRoots(string scriptPath)
        {
            if (!string.IsNullOrEmpty(scriptPath))
            {
                var dir = Path.GetDirectoryName(scriptPath);
                while (!string.IsNullOrEmpty(dir))
                {
                    yield return dir;

                    var parent = Path.GetDirectoryName(dir);
                    if (parent == dir)
                    {
                        break;
                    }

                    dir = parent;
                }
            }

            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(UnityCLIWindow).Assembly);
            if (packageInfo != null && !string.IsNullOrWhiteSpace(packageInfo.resolvedPath))
            {
                yield return packageInfo.resolvedPath;
            }

            var projectRoot = ProjectRoot;
            yield return Path.Combine(projectRoot, "Packages", "com.oathx.unitycli");
            yield return Path.Combine(projectRoot, "LocalPackages", "com.oathx.unitycli");
        }

        /// <summary>
        /// Resolves the unity-cli skill folder from a candidate root.
        /// </summary>
        /// <param name="root">A candidate package or ancestor directory.</param>
        /// <returns>The skill folder path, or null.</returns>
        static string TryResolveUnityCliSkillFromRoot(string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return null;
            }

            var candidate = Path.Combine(root, UnityCliSkillFolderName, "SKILL.md");
            if (File.Exists(candidate))
            {
                return Path.GetFullPath(Path.Combine(root, UnityCliSkillFolderName));
            }

            return null;
        }

        sealed class JiraEditorConfig
        {
            public string Site { get; set; } = string.Empty;
            public string ApiToken { get; set; } = string.Empty;
        }
    }
}
