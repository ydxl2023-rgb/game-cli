using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Oathx.UnityCLI.Protocol;
using UnityEditor;
using UnityEngine;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Hosts the Unity Editor side of the UnityCLI named-pipe bridge.
    /// </summary>
    [InitializeOnLoad]
    public static class UnityCliServer
    {
        /// <summary>
        /// Protocol version advertised to CLI clients.
        /// </summary>
        const string ProtocolVersion = "1.0";
        /// <summary>
        /// Prefix used for project-specific named pipes.
        /// </summary>
        const string PipeNamePrefix = "unitycli-";

        /// <summary>
        /// Guards bridge lifecycle state.
        /// </summary>
        static readonly object SyncRoot = new object();

        /// <summary>
        /// Cancels the active listener loop.
        /// </summary>
        static CancellationTokenSource cancellationSource;
        /// <summary>
        /// Background task that accepts pipe connections.
        /// </summary>
        static Task listenTask;
        /// <summary>
        /// Endpoint metadata currently written to disk.
        /// </summary>
        static CLIEndpoint currentEndpoint;

        /// <summary>
        /// Starts the bridge when the editor assembly loads and tears it down on reload or quit.
        /// </summary>
        static UnityCliServer()
        {
            EditorApplication.delayCall += EnsureRunning;
            AssemblyReloadEvents.beforeAssemblyReload += () => Stop(false);
            EditorApplication.quitting += () => Stop(true);
            EnsureRunning();
        }

        /// <summary>
        /// Gets the endpoint file path consumed by the standalone CLI.
        /// </summary>
        public static string EndpointFilePath
        {
            get { return Path.Combine(ProjectRoot, "Library", "UnityCLI", "endpoint.json"); }
        }

        /// <summary>
        /// Starts the bridge if it is not already listening.
        /// </summary>
        public static void EnsureRunning()
        {
            if (IsWorkerProcess())
            {
                return;
            }

            lock (SyncRoot)
            {
                if (listenTask != null && !listenTask.IsCompleted)
                {
                    return;
                }

                StartLocked();
            }
        }

        /// <summary>
        /// Returns whether this editor process is a non-interactive worker that should not expose the CLI endpoint.
        /// </summary>
        /// <returns>True when the current process is an asset import worker or batch mode editor.</returns>
        static bool IsWorkerProcess()
        {
            if (Application.isBatchMode)
            {
                return true;
            }

            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-name", StringComparison.OrdinalIgnoreCase)
                    && i + 1 < args.Length
                    && args[i + 1].IndexOf("AssetImportWorker", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Stops the bridge and optionally removes the published endpoint file.
        /// </summary>
        /// <param name="deleteEndpointFile">True to delete endpoint.json after the listener stops.</param>
        public static void Stop(bool deleteEndpointFile)
        {
            CancellationTokenSource source;
            Task task;
            string pipeName;

            lock (SyncRoot)
            {
                source = cancellationSource;
                task = listenTask;
                pipeName = currentEndpoint != null ? currentEndpoint.pipeName : string.Empty;
                cancellationSource = null;
                listenTask = null;
                currentEndpoint = null;
            }

            UnityCliMainThread.Shutdown();

            if (source != null)
            {
                source.Cancel();
                SignalPipe(pipeName);

                try
                {
                    if (task != null)
                    {
                        task.Wait(TimeSpan.FromMilliseconds(500));
                    }
                }
                catch
                {
                }
                finally
                {
                    source.Dispose();
                }
            }

            if (deleteEndpointFile && File.Exists(EndpointFilePath))
            {
                File.Delete(EndpointFilePath);
            }
        }

        /// <summary>
        /// Creates endpoint metadata and starts the listener. Caller must hold <see cref="SyncRoot"/>.
        /// </summary>
        static void StartLocked()
        {
            UnityCliMainThread.Initialize();

            Directory.CreateDirectory(Path.GetDirectoryName(EndpointFilePath));

            var token = Guid.NewGuid().ToString("N");
            var pipeName = PipeNamePrefix + HashProjectPath(ProjectRoot) + "-" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
            var endpoint = new CLIEndpoint
            {
                protocolVersion = ProtocolVersion,
                transport = CLIEndpoint.TransportNamedPipe,
                pipeName = pipeName,
                token = token,
                pid = System.Diagnostics.Process.GetCurrentProcess().Id,
                projectPath = ProjectRoot,
                startedAtUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture)
            };

            File.WriteAllText(EndpointFilePath, JsonUtility.ToJson(endpoint, true));

            cancellationSource = new CancellationTokenSource();
            currentEndpoint = endpoint;
            string pipeSecurity = LoadPipeSecurity();
            listenTask = Task.Run(() => ListenLoopAsync(pipeName, token, cancellationSource.Token, pipeSecurity));
            Debug.Log("[UnityCLI] Ping bridge started: " + pipeName);
        }

        /// <summary>
        /// Accepts incoming named-pipe connections until cancellation is requested.
        /// </summary>
        /// <param name="pipeName">The named pipe to listen on.</param>
        /// <param name="token">The session token expected in every request.</param>
        /// <param name="cancellationToken">The token used to stop listening.</param>
        /// <returns>A task that completes when the listener exits.</returns>
        [Serializable]
        private sealed class PipeAccessSettings
        {
            public string[] allowedClientSids = Array.Empty<string>();
        }

        private static string LoadPipeSecurity()
        {
            string path = Path.Combine(ProjectRoot, "Library", "UnityCLI", "pipe-access.json");
            if (!File.Exists(path))
            {
                // Without the optional allowlist file, keep the default-equivalent DACL
                // (SYSTEM/owner full, Everyone read-only) plus the low label so
                // sandboxed CLI hosts still connect even after Library is wiped.
                if (Application.platform != RuntimePlatform.WindowsEditor)
                {
                    return null;
                }
                return "S:(ML;;NW;;;S-1-16-4096)D:(A;;GA;;;SY)(A;;GA;;;OW)(A;;GR;;;WD)";
            }
            if (Application.platform != RuntimePlatform.WindowsEditor)
            {
                throw new InvalidOperationException("pipe-access.json is supported only on Windows.");
            }
            PipeAccessSettings settings = JsonUtility.FromJson<PipeAccessSettings>(File.ReadAllText(path));
            if (settings == null || settings.allowedClientSids == null || settings.allowedClientSids.Length > 16)
            {
                throw new InvalidOperationException("Invalid pipe-access.json account allowlist.");
            }
            // Owner Rights preserves the pipe creator's access without Mono WindowsIdentity APIs.
            // Low mandatory label lets sandboxed CLI hosts (low-integrity tokens) open the pipe.
            string security = "S:(ML;;NW;;;S-1-16-4096)D:(A;;GA;;;SY)(A;;GA;;;OW)";
            foreach (string value in settings.allowedClientSids)
            {
                if (value == null || !System.Text.RegularExpressions.Regex.IsMatch(value, @"^S-1-5-21-\d+-\d+-\d+-\d+$"))
                {
                    throw new InvalidOperationException("Pipe clients must use explicit Windows account SIDs.");
                }
                security += "(A;;GA;;;" + value + ")";
            }
            return security;
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(string descriptor, uint revision, out IntPtr security, out uint size);

        [StructLayout(LayoutKind.Sequential)]
        private struct SecurityAttributes
        {
            public int length;
            public IntPtr descriptor;
            public int inheritHandle;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Microsoft.Win32.SafeHandles.SafePipeHandle CreateNamedPipe(string name, uint mode, uint pipeMode, uint instances, uint outBuffer, uint inBuffer, uint timeout, ref SecurityAttributes security);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr memory);

        private static NamedPipeServerStream CreatePipe(string name, string descriptor)
        {
            if (descriptor == null)
            {
                return new NamedPipeServerStream(name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            }
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor(descriptor, 1, out IntPtr security, out _))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            try
            {
                SecurityAttributes attributes = new SecurityAttributes
                {
                    length = Marshal.SizeOf(typeof(SecurityAttributes)),
                    descriptor = security,
                    inheritHandle = 0
                };
                // Install the account allowlist atomically at creation; reject remote clients.
                Microsoft.Win32.SafeHandles.SafePipeHandle handle = CreateNamedPipe(@"\\.\pipe\" + name, 0x40000003, 8, 255, 4096, 4096, 0, ref attributes);
                if (handle.IsInvalid)
                {
                    int error = Marshal.GetLastWin32Error();
                    handle.Dispose();
                    throw new Win32Exception(error);
                }
                try
                {
                    return new NamedPipeServerStream(PipeDirection.InOut, true, false, handle);
                }
                catch
                {
                    handle.Dispose();
                    throw;
                }
            }
            finally
            {
                LocalFree(security);
            }
        }
        static async Task ListenLoopAsync(string pipeName, string token, CancellationToken cancellationToken, string pipeSecurity)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                NamedPipeServerStream server = null;
                try
                {
                    server = CreatePipe(pipeName, pipeSecurity);
                    await server.WaitForConnectionAsync(cancellationToken);
                    _ = HandleConnectionAsync(server, token, cancellationToken);
                    server = null;
                }
                catch (OperationCanceledException)
                {
                    server?.Dispose();
                    break;
                }
                catch (ObjectDisposedException)
                {
                    server?.Dispose();
                    break;
                }
                catch (Exception exception)
                {
                    server?.Dispose();
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning("[UnityCLI] Pipe listener failed: " + exception.Message);
                        // A persistent endpoint error must not spin or flood the Editor log.
                        await Task.Delay(1000);
                    }
                }
            }
        }

        /// <summary>
        /// Reads one request, validates it, invokes the matching route handler, and writes one response.
        /// </summary>
        /// <param name="stream">The connected named-pipe stream.</param>
        /// <param name="token">The session token expected in the request.</param>
        /// <param name="cancellationToken">The token used to cancel request handling.</param>
        /// <returns>A task that completes when the connection has been handled.</returns>
        static async Task HandleConnectionAsync(NamedPipeServerStream stream, string token, CancellationToken cancellationToken)
        {
            using (stream)
            {
                try
                {
                    var requestJson = await CLIPipeProtocol.ReadFrameAsync(stream, cancellationToken);
                    var request = JsonUtility.FromJson<UnityCliBridgeRequest>(requestJson);
                    LogIncomingRequest(request);

                    if (request == null || request.token != token)
                    {
                        await WriteAsync(stream, PingResult.Fail("unauthorized", "Missing or invalid token."), cancellationToken);
                        return;
                    }

                    var path = NormalizePath(request.path);
                    if (!UnityCliCommandRegistry.TryGetCommand(path, out var command))
                    {
                        var implemented = UnityCliCommandRegistry.FormatImplementedRoutes();
                        var method = request.method ?? string.Empty;
                        await WriteAsync(stream, PingResult.Fail("not_found", $"Unknown path \"{path}\" for method \"{method}\". This Editor exposes: {implemented}."), cancellationToken);
                        return;
                    }

                    if (!string.Equals(request.method, command.Method, StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteAsync(stream, PingResult.Fail("method_not_allowed", "Only " + command.Method + " is allowed for " + command.Path + "."), cancellationToken);
                        return;
                    }

                    if (!UnityCliCommandSettings.IsEnabled(command.Path))
                    {
                        await WriteAsync(stream, PingResult.Fail(
                            "command_disabled",
                            "The route " + command.Method + " " + command.Path +
                            " is disabled in this Editor (toggle it under Tools > UnityCLI > Window)."),
                            cancellationToken);
                        return;
                    }

                    var context = new UnityCLICommandContext(currentEndpoint);
                    var response = await command.ExecuteAsync(request, context, cancellationToken);
                    await WriteAsync(stream, response, cancellationToken);
                }
                catch (Exception exception)
                {
                    if (!(exception is OperationCanceledException) && !(exception is ObjectDisposedException))
                    {
                        Debug.LogWarning("[UnityCLI] Request failed: " + exception.Message);
                        await TryWriteErrorAsync(stream, exception, cancellationToken);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes and writes a response DTO to the pipe.
        /// </summary>
        /// <param name="stream">The connected named-pipe stream.</param>
        /// <param name="result">The response DTO to serialize.</param>
        /// <param name="cancellationToken">The token used to cancel the write.</param>
        /// <returns>A task that completes when the response frame has been written.</returns>
        static Task WriteAsync(Stream stream, object result, CancellationToken cancellationToken)
        {
            var json = JsonUtility.ToJson(result, false);
            LogOutgoingResponse(result);
            return CLIPipeProtocol.WriteFrameAsync(stream, json, cancellationToken);
        }

        // Both logging helpers run on the named-pipe listener thread (background).
        // Debug.Log / Debug.LogWarning are thread-safe in modern Unity, and the rest of
        // the file already uses them off-main, so this stays consistent.

        /// <summary>
        /// Logs a compact request summary when verbose bridge logging is enabled.
        /// </summary>
        /// <param name="request">The parsed bridge request.</param>
        static void LogIncomingRequest(UnityCliBridgeRequest request)
        {
            if (!UnityCliBridgeLogSettings.VerboseLogging) return;
            if (request == null)
            {
                Debug.LogWarning("[UnityCLI] CLI -> Editor:  <unparseable request>");
                return;
            }

            var method = string.IsNullOrEmpty(request.method) ? "?" : request.method;
            var path = NormalizePath(request.path);
            var summary = SummarizeRequestParams(request);
            if (summary.Length > 0)
            {
                Debug.Log("[UnityCLI] CLI -> Editor:  " + method + " " + path + "  " + summary);
            }
            else
            {
                Debug.Log("[UnityCLI] CLI -> Editor:  " + method + " " + path);
            }
        }

        /// <summary>
        /// Logs a compact response summary when verbose bridge logging is enabled.
        /// </summary>
        /// <param name="result">The response DTO returned by a command.</param>
        static void LogOutgoingResponse(object result)
        {
            if (!UnityCliBridgeLogSettings.VerboseLogging) return;
            if (result == null) return;

            // Every response DTO in UnityCliBridgeDtos.cs shares ok/status/message fields,
            // so reflect them off rather than coupling to each result type.
            var type = result.GetType();
            var ok = ReadField<bool>(type, result, "ok", true);
            var status = ReadField<string>(type, result, "status", null);
            var message = ReadField<string>(type, result, "message", null);

            var line = "[UnityCLI] Editor -> CLI:  " + (ok ? "ok" : "ERR") +
                       "  status=" + (string.IsNullOrEmpty(status) ? "?" : status) +
                       (string.IsNullOrEmpty(message) ? string.Empty : "  message=\"" + Truncate(message, 200) + "\"");

            if (ok)
            {
                Debug.Log(line);
            }
            else
            {
                Debug.LogWarning(line);
            }
        }

        /// <summary>
        /// Reads a public instance field from a response DTO.
        /// </summary>
        /// <typeparam name="T">The expected field type.</typeparam>
        /// <param name="type">The DTO runtime type.</param>
        /// <param name="instance">The DTO instance.</param>
        /// <param name="name">The public field name.</param>
        /// <param name="fallback">The value returned when the field is absent or not assignable.</param>
        /// <returns>The field value or the fallback value.</returns>
        static T ReadField<T>(Type type, object instance, string name, T fallback)
        {
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) return fallback;
            var raw = field.GetValue(instance);
            if (raw is T typed) return typed;
            return fallback;
        }

        /// <summary>
        /// Builds a short, safe-to-log summary of request parameters.
        /// </summary>
        /// <param name="req">The request to summarize.</param>
        /// <returns>A whitespace-delimited parameter summary.</returns>
        static string SummarizeRequestParams(UnityCliBridgeRequest req)
        {
            var sb = new StringBuilder();

            AppendStr(sb, "type", req.type);
            AppendStr(sb, "search", req.search);
            AppendInt(sb, "count", req.count);
            AppendStr(sb, "assetPath", req.assetPath);
            AppendInt(sb, "instanceId", req.instanceId);
            AppendStr(sb, "objectPath", req.objectPath);
            AppendStr(sb, "component", req.component);
            AppendStr(sb, "property", req.property);
            AppendStr(sb, "value", req.value);
            AppendStr(sb, "action", req.action);
            AppendStr(sb, "invokeType", req.invokeType);
            AppendStr(sb, "invokeMember", req.invokeMember);
            if (req.invokeArgs != null && req.invokeArgs.Length > 0)
            {
                AppendRaw(sb, "invokeArgs", "(" + req.invokeArgs.Length + ")");
            }
            if (!string.IsNullOrEmpty(req.invokeArgsJson))
            {
                AppendRaw(sb, "invokeArgsJson", "(" + req.invokeArgsJson.Length + " chars)");
            }
            if (req.compileWait)
            {
                AppendRaw(sb, "compileWait", "true");
            }
            AppendStr(sb, "saveScenePath", req.saveScenePath);
            AppendStr(sb, "assetSrcPath", req.assetSrcPath);
            AppendStr(sb, "assetDstPath", req.assetDstPath);
            AppendStr(sb, "uiName", req.uiName);
            AppendStr(sb, "vmName", req.vmName);
            AppendStr(sb, "prefabPath", req.prefabPath);
            if (req.dryRun)
            {
                AppendRaw(sb, "dryRun", "true");
            }
            AppendStr(sb, "screenshotPath", req.screenshotPath);
            AppendInt(sb, "screenshotSuperSize", req.screenshotSuperSize);

            return sb.ToString();
        }

        /// <summary>
        /// Appends a non-empty string field to a log summary.
        /// </summary>
        /// <param name="sb">The summary builder.</param>
        /// <param name="name">The field name.</param>
        /// <param name="value">The field value.</param>
        static void AppendStr(StringBuilder sb, string name, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(name).Append('=').Append(Truncate(value, 80));
        }

        /// <summary>
        /// Appends a non-zero integer field to a log summary.
        /// </summary>
        /// <param name="sb">The summary builder.</param>
        /// <param name="name">The field name.</param>
        /// <param name="value">The field value.</param>
        static void AppendInt(StringBuilder sb, string name, int value)
        {
            if (value == 0) return;
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(name).Append('=').Append(value.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Appends an already-formatted field to a log summary.
        /// </summary>
        /// <param name="sb">The summary builder.</param>
        /// <param name="name">The field name.</param>
        /// <param name="value">The formatted field value.</param>
        static void AppendRaw(StringBuilder sb, string name, string value)
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(name).Append('=').Append(value);
        }

        /// <summary>
        /// Shortens long log values.
        /// </summary>
        /// <param name="value">The value to shorten.</param>
        /// <param name="max">The maximum returned length before adding an ellipsis.</param>
        /// <returns>The original value or a shortened value.</returns>
        static string Truncate(string value, int max)
        {
            if (value == null) return string.Empty;
            return value.Length <= max ? value : value.Substring(0, max) + "...";
        }

        /// <summary>
        /// Attempts to return a structured bridge error for unexpected server failures.
        /// </summary>
        /// <param name="stream">The connected named-pipe stream.</param>
        /// <param name="exception">The exception to report.</param>
        /// <param name="cancellationToken">The token used to cancel the write.</param>
        /// <returns>A task that completes after the best-effort write attempt.</returns>
        static async Task TryWriteErrorAsync(Stream stream, Exception exception, CancellationToken cancellationToken)
        {
            try
            {
                if (stream.CanWrite)
                {
                    await WriteAsync(stream, PingResult.Fail("server_error", exception.Message), cancellationToken);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Normalizes a route path to include a leading slash.
        /// </summary>
        /// <param name="path">The incoming route path.</param>
        /// <returns>The normalized route path.</returns>
        static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "/";
            }

            return path[0] == '/' ? path : "/" + path;
        }

        /// <summary>
        /// Produces a stable short hash for the project path portion of the pipe name.
        /// </summary>
        /// <param name="path">The Unity project root.</param>
        /// <returns>A hexadecimal FNV-1a hash string.</returns>
        static string HashProjectPath(string path)
        {
            unchecked
            {
                uint hash = 2166136261;
                for (var index = 0; index < path.Length; index++)
                {
                    hash ^= char.ToUpperInvariant(path[index]);
                    hash *= 16777619;
                }

                return hash.ToString("x8", CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the current Unity project root.
        /// </summary>
        static string ProjectRoot
        {
            get { return Path.GetDirectoryName(Application.dataPath) ?? Directory.GetCurrentDirectory(); }
        }

        /// <summary>
        /// Connects briefly to the listener pipe so cancellation can unblock WaitForConnectionAsync.
        /// </summary>
        /// <param name="pipeName">The pipe name to signal.</param>
        static void SignalPipe(string pipeName)
        {
            if (string.IsNullOrEmpty(pipeName))
            {
                return;
            }

            try
            {
                using (var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                {
                    client.Connect(100);
                }
            }
            catch
            {
            }
        }
    }
}
