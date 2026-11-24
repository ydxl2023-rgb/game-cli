namespace UnityCLI
{
    /// <summary>
    /// Entry point for the standalone UnityCLI executable.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Maps command names from the command line to their client-side command handlers.
        /// </summary>
        static readonly Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase)
        {
            { "ping", new PingCommand() },
            { "console", new ConsoleCommand() },
            { "search", new SearchCommand() },
            { "measure", new MeasureCommand() },
            { "modify", new ModifyCommand() },
            { "run", new RunCommand() },
            { "call", new CallCommand() },
            { "compile", new CompileCommand() },
            { "save", new SaveCommand() },
            { "components", new ComponentsCommand() },
            { "asset", new AssetCommand() },
            { "ugui", new UguiCommand() },
            { "ui-resources", new UiResourcesCommand() },
            { "ui-prepare", new UiPrepareCommand() },
            { "widget-metadata", new WidgetMetadataCommand() },
            { "particle", new ParticleCommand() },
            { "prefab", new PrefabCommand() },
            { "animator", new AnimatorCommand() },
            { "excel", new ExcelCommand() },
            { "jira", new JiraCommand() },
            { "terrain", new TerrainCommand() },
            { "screenshot", new ScreenshotCommand() }
        };

        /// <summary>
        /// Parses command-line arguments and dispatches the selected command.
        /// </summary>
        /// <param name="args">The raw process arguments.</param>
        /// <returns>The process exit code.</returns>
        static async Task<int> Main(string[] args)
        {
            if (args.Length == 0 || IsHelp(args[0]))
            {
                PrintUsage();
                return 0;
            }

            if (!TryParse(args, out var options, out var error))
            {
                Console.Error.WriteLine(error);
                PrintUsage();
                return 1;
            }

            if (!Commands.TryGetValue(options.Command, out var command))
            {
                Console.Error.WriteLine("Unknown command: " + options.Command);
                PrintUsage();
                return 1;
            }

            try
            {
                return await command.ExecuteAsync(options);
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(command.Name + " failed: " + exception.Message);
                return 1;
            }
        }

        /// <summary>
        /// Parses global options and command-specific options into a shared command model.
        /// </summary>
        /// <param name="args">The raw process arguments.</param>
        /// <param name="options">The parsed options when parsing succeeds.</param>
        /// <param name="error">The validation error when parsing fails.</param>
        /// <returns>True when parsing succeeds; otherwise false.</returns>
        static bool TryParse(string[] args, out CliOptions options, out string error)
        {
            options = new CliOptions();
            error = string.Empty;

            for (var index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                if (arg == "--source-key")
                {
                    if (++index >= args.Length)
                    {
                        error = "--source-key requires a stable project ID.";
                        return false;
                    }
                    options.ExportSourceKey = args[index];
                    continue;
                }
                if (arg == "--project")
                {
                    if (++index >= args.Length)
                    {
                        error = "--project requires a path.";
                        return false;
                    }

                    if (string.Equals(options.Command, "jira", StringComparison.OrdinalIgnoreCase))
                    {
                        options.JiraProjectKey = args[index];
                    }
                    else
                    {
                        options.ProjectPath = args[index];
                    }
                    continue;
                }

                if (arg == "--format")
                {
                    if (++index >= args.Length)
                    {
                        error = "--format requires json or human.";
                        return false;
                    }

                    var format = args[index];
                    if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
                    {
                        options.JsonOutput = true;
                    }
                    else if (!string.Equals(format, "human", StringComparison.OrdinalIgnoreCase))
                    {
                        error = "Unsupported format: " + format;
                        return false;
                    }

                    continue;
                }

                if (arg == "--count")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var count))
                    {
                        error = "--count requires an integer.";
                        return false;
                    }

                    options.Count = count;
                    continue;
                }

                if (arg == "--type")
                {
                    if (++index >= args.Length)
                    {
                        error = "--type requires all, log, warning, or error.";
                        return false;
                    }

                    if (string.Equals(options.Command, "jira", StringComparison.OrdinalIgnoreCase))
                    {
                        options.JiraIssueType = args[index];
                    }
                    else
                    {
                        options.Type = args[index];
                    }
                    continue;
                }

                if (arg == "--search" || arg == "--serach")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a search pattern.";
                        return false;
                    }

                    options.Search = args[index];
                    continue;
                }

                if (arg == "--name")
                {
                    if (++index >= args.Length)
                    {
                        error = "--name requires a GameObject name or pattern.";
                        return false;
                    }

                    options.Search = args[index];
                    continue;
                }

                if (arg == "--id")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var instanceId))
                    {
                        error = "--id requires an integer instance id.";
                        return false;
                    }

                    options.InstanceId = instanceId;
                    continue;
                }

                if (arg == "--asset")
                {
                    if (++index >= args.Length)
                    {
                        error = "--asset requires an asset path.";
                        return false;
                    }

                    options.AssetPath = args[index];
                    continue;
                }

                if (arg == "--path")
                {
                    if (++index >= args.Length)
                    {
                        error = "--path requires a GameObject hierarchy path.";
                        return false;
                    }

                    options.ObjectPath = args[index];
                    continue;
                }

                if (arg == "--parent-prefab")
                {
                    if (++index >= args.Length)
                    {
                        error = "--parent-prefab requires an Assets/... prefab path.";
                        return false;
                    }

                    options.ParentPrefabPath = args[index];
                    continue;
                }

                if (arg == "--child-prefab")
                {
                    if (++index >= args.Length)
                    {
                        error = "--child-prefab requires an Assets/... prefab path.";
                        return false;
                    }

                    options.ChildPrefabPath = args[index];
                    continue;
                }

                if (arg == "--parent-path")
                {
                    if (++index >= args.Length)
                    {
                        error = "--parent-path requires a prefab-internal hierarchy path.";
                        return false;
                    }

                    options.ParentObjectPath = args[index];
                    continue;
                }

                if (arg == "--child-name")
                {
                    if (++index >= args.Length)
                    {
                        error = "--child-name requires a GameObject name.";
                        return false;
                    }

                    options.ChildName = args[index];
                    continue;
                }

                if (arg == "--group-name")
                {
                    if (++index >= args.Length)
                    {
                        error = "--group-name requires a GameObject name.";
                        return false;
                    }

                    options.GroupName = args[index];
                    continue;
                }

                if (arg == "--children")
                {
                    if (++index >= args.Length)
                    {
                        error = "--children requires a comma-separated node name list.";
                        return false;
                    }

                    options.GroupChildren = args[index];
                    continue;
                }

                if (arg == "--anchor")
                {
                    if (++index >= args.Length)
                    {
                        error = "--anchor requires xmin,ymin,xmax,ymax.";
                        return false;
                    }

                    options.GroupAnchor = args[index];
                    continue;
                }

                if (arg == "--pivot")
                {
                    if (++index >= args.Length)
                    {
                        error = "--pivot requires x,y.";
                        return false;
                    }

                    options.GroupPivot = args[index];
                    continue;
                }

                if (arg == "--group-size")
                {
                    if (++index >= args.Length)
                    {
                        error = "--group-size requires w,h.";
                        return false;
                    }

                    options.GroupSize = args[index];
                    continue;
                }

                if (arg == "--sibling-index")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var siblingIndex))
                    {
                        error = "--sibling-index requires an integer.";
                        return false;
                    }

                    options.GroupSiblingIndex = siblingIndex;
                    continue;
                }

                if (arg == "--replace")
                {
                    options.ReplaceExistingChild = true;
                    options.ReplaceExistingChildSet = true;
                    continue;
                }

                if (arg == "--no-replace")
                {
                    options.ReplaceExistingChild = false;
                    options.ReplaceExistingChildSet = true;
                    continue;
                }

                if (arg == "--layer")
                {
                    if (++index >= args.Length)
                    {
                        error = "--layer requires a layer name or index.";
                        return false;
                    }

                    if (string.Equals(options.Command, "animator", StringComparison.OrdinalIgnoreCase))
                    {
                        options.AnimatorLayer = args[index];
                    }
                    else if (string.Equals(options.Command, "terrain", StringComparison.OrdinalIgnoreCase))
                    {
                        options.TerrainLayerPath = args[index];
                    }
                    else
                    {
                        options.Layer = args[index];
                    }
                    continue;
                }

                if (arg == "--keep-layer")
                {
                    options.KeepLayer = true;
                    continue;
                }

                if (arg == "--component")
                {
                    if (++index >= args.Length)
                    {
                        error = "--component requires a component type.";
                        return false;
                    }

                    options.Component = args[index];
                    continue;
                }

                if (arg == "--property")
                {
                    if (++index >= args.Length)
                    {
                        error = "--property requires a SerializedProperty path.";
                        return false;
                    }

                    options.Property = args[index];
                    continue;
                }

                if (arg == "--value")
                {
                    if (++index >= args.Length)
                    {
                        error = "--value requires a value.";
                        return false;
                    }

                    options.Value = args[index];
                    continue;
                }

                if (arg == "--invoke-type")
                {
                    if (++index >= args.Length)
                    {
                        error = "--invoke-type requires a full CLR type name (e.g. UnityEditor.EditorApplication).";
                        return false;
                    }

                    options.InvokeType = args[index];
                    continue;
                }

                if (arg == "--invoke-member")
                {
                    if (++index >= args.Length)
                    {
                        error = "--invoke-member requires a method name.";
                        return false;
                    }

                    options.InvokeMember = args[index];
                    continue;
                }

                if (arg == "--args-json")
                {
                    if (++index >= args.Length)
                    {
                        error = "--args-json requires a JSON array literal (e.g. [] or [1,\"x\"]).";
                        return false;
                    }

                    options.InvokeArgsJson = args[index];
                    continue;
                }

                if (arg == "--arg")
                {
                    if (++index >= args.Length)
                    {
                        error = "--arg requires a value (repeat for multiple arguments).";
                        return false;
                    }

                    options.InvokeArgList.Add(args[index]);
                    continue;
                }

                if (arg == "--wait")
                {
                    options.CompileWait = true;
                    continue;
                }

                if (arg == "--scene")
                {
                    if (++index >= args.Length)
                    {
                        error = "--scene requires a scene asset path (e.g. Assets/Scenes/SampleScene.unity).";
                        return false;
                    }

                    options.SaveScenePath = args[index];
                    continue;
                }

                if (arg == "--src")
                {
                    if (++index >= args.Length)
                    {
                        error = "--src requires a path.";
                        return false;
                    }

                    options.AssetSrcPath = args[index];
                    continue;
                }

                if (arg == "--dst")
                {
                    if (++index >= args.Length)
                    {
                        error = "--dst requires a path.";
                        return false;
                    }

                    options.AssetDstPath = args[index];
                    continue;
                }

                if (arg == "--output" || arg == "--file")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a path.";
                        return false;
                    }

                    if (string.Equals(options.Command, "excel", StringComparison.OrdinalIgnoreCase) && arg == "--file")
                    {
                        options.ExcelFilePath = args[index];
                    }
                    else
                    {
                        options.ScreenshotPath = args[index];
                    }
                    continue;
                }

                if (arg == "--super-size" || arg == "--supersize")
                {
                    if (++index >= args.Length
                        || !int.TryParse(args[index], out var superSize)
                        || superSize < 1)
                    {
                        error = arg + " requires a positive integer.";
                        return false;
                    }

                    options.ScreenshotSuperSize = superSize;
                    continue;
                }

                if (arg == "--material")
                {
                    if (++index >= args.Length)
                    {
                        error = "--material requires an Assets/... material path.";
                        return false;
                    }

                    options.ParticleMaterialPath = args[index];
                    continue;
                }

                if (arg == "--texture")
                {
                    if (++index >= args.Length)
                    {
                        error = "--texture requires an Assets/... texture path.";
                        return false;
                    }

                    if (string.Equals(options.Command, "terrain", StringComparison.OrdinalIgnoreCase))
                    {
                        options.TerrainTexturePath = args[index];
                    }
                    else
                    {
                        options.ParticleTexturePath = args[index];
                    }
                    continue;
                }

                if (arg == "--data" || arg == "--terrain-data")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires an Assets/... TerrainData path.";
                        return false;
                    }

                    options.TerrainDataPath = args[index];
                    continue;
                }

                if (arg == "--normal")
                {
                    if (++index >= args.Length)
                    {
                        error = "--normal requires an Assets/... texture path.";
                        return false;
                    }

                    options.TerrainNormalPath = args[index];
                    continue;
                }

                if (arg == "--tile-size")
                {
                    if (++index >= args.Length)
                    {
                        error = "--tile-size requires x,y.";
                        return false;
                    }

                    options.TerrainTileSize = args[index];
                    continue;
                }

                if (arg == "--tile-offset")
                {
                    if (++index >= args.Length)
                    {
                        error = "--tile-offset requires x,y.";
                        return false;
                    }

                    options.TerrainTileOffset = args[index];
                    continue;
                }

                if (arg == "--terrain-size")
                {
                    if (++index >= args.Length)
                    {
                        error = "--terrain-size requires x,y,z.";
                        return false;
                    }

                    options.TerrainSize = args[index];
                    continue;
                }

                if (arg == "--smoothness")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.TerrainSmoothness = value;
                    options.TerrainSmoothnessSet = true;
                    continue;
                }

                if (arg == "--metallic")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.TerrainMetallic = value;
                    options.TerrainMetallicSet = true;
                    continue;
                }

                if (arg == "--heightmap-resolution")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var resolution) || resolution < 33)
                    {
                        error = "--heightmap-resolution requires an integer >= 33.";
                        return false;
                    }

                    options.TerrainHeightmapResolution = resolution;
                    options.TerrainHeightmapResolutionSet = true;
                    continue;
                }

                if (arg == "--alphamap-resolution")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var resolution) || resolution < 16)
                    {
                        error = "--alphamap-resolution requires an integer >= 16.";
                        return false;
                    }

                    options.TerrainAlphamapResolution = resolution;
                    options.TerrainAlphamapResolutionSet = true;
                    continue;
                }

                if (arg == "--base-map-resolution")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var resolution) || resolution < 16)
                    {
                        error = "--base-map-resolution requires an integer >= 16.";
                        return false;
                    }

                    options.TerrainBaseMapResolution = resolution;
                    options.TerrainBaseMapResolutionSet = true;
                    continue;
                }

                if (arg == "--append")
                {
                    options.TerrainAppendLayer = true;
                    continue;
                }

                if (arg == "--shape")
                {
                    if (++index >= args.Length)
                    {
                        error = "--shape requires cone, sphere, hemisphere, circle, or box.";
                        return false;
                    }

                    options.ParticleShape = args[index];
                    continue;
                }

                if ((arg == "--position" || arg == "--local-position")
                    && string.Equals(options.Command, "prefab", StringComparison.OrdinalIgnoreCase))
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires x,y,z.";
                        return false;
                    }

                    options.LocalPosition = args[index];
                    continue;
                }

                if (arg == "--position" || arg == "--local-position")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires x,y,z.";
                        return false;
                    }

                    options.ParticlePosition = args[index];
                    continue;
                }

                if (arg == "--local-rotation" || arg == "--rotation")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires x,y,z Euler angles.";
                        return false;
                    }

                    options.LocalRotation = args[index];
                    continue;
                }

                if (arg == "--local-scale" || arg == "--scale")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires x,y,z.";
                        return false;
                    }

                    options.LocalScale = args[index];
                    continue;
                }

                if (arg == "--shape-scale")
                {
                    if (++index >= args.Length)
                    {
                        error = "--shape-scale requires x,y,z.";
                        return false;
                    }

                    options.ParticleShapeScale = args[index];
                    continue;
                }

                if (arg == "--shape-radius")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleShapeRadius = value;
                    options.ParticleShapeRadiusSet = true;
                    continue;
                }

                if (arg == "--shape-angle")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleShapeAngle = value;
                    options.ParticleShapeAngleSet = true;
                    continue;
                }

                if (arg == "--velocity-x" || arg == "--vx")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires min,max.";
                        return false;
                    }

                    options.ParticleVelocityX = args[index];
                    continue;
                }

                if (arg == "--velocity-y" || arg == "--vy")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires min,max.";
                        return false;
                    }

                    options.ParticleVelocityY = args[index];
                    continue;
                }

                if (arg == "--velocity-z" || arg == "--vz")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires min,max.";
                        return false;
                    }

                    options.ParticleVelocityZ = args[index];
                    continue;
                }

                if (arg == "--noise-strength")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleNoiseStrength = value;
                    options.ParticleNoiseStrengthSet = true;
                    continue;
                }

                if (arg == "--noise-frequency")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleNoiseFrequency = value;
                    options.ParticleNoiseFrequencySet = true;
                    continue;
                }

                if (arg == "--noise-scroll-speed")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleNoiseScrollSpeed = value;
                    options.ParticleNoiseScrollSpeedSet = true;
                    continue;
                }

                if (arg == "--rotation-z")
                {
                    if (++index >= args.Length)
                    {
                        error = "--rotation-z requires min,max.";
                        return false;
                    }

                    options.ParticleRotationZ = args[index];
                    continue;
                }

                if (arg == "--size-over-lifetime")
                {
                    if (++index >= args.Length)
                    {
                        error = "--size-over-lifetime requires start,end.";
                        return false;
                    }

                    options.ParticleSizeOverLifetime = args[index];
                    continue;
                }

                if (arg == "--fade-out")
                {
                    options.ParticleFadeOut = true;
                    continue;
                }

                if (arg == "--fade-start")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleFadeStart = value;
                    options.ParticleFadeStartSet = true;
                    continue;
                }

                if (arg == "--sorting-order")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var sortingOrder))
                    {
                        error = "--sorting-order requires an integer.";
                        return false;
                    }

                    options.ParticleSortingOrder = sortingOrder;
                    options.ParticleSortingOrderSet = true;
                    continue;
                }

                if (arg == "--start-color" || arg == "--color")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a color such as #FFAA33 or rgba(255,128,0,0.8).";
                        return false;
                    }

                    options.ParticleStartColor = args[index];
                    continue;
                }

                if (arg == "--duration")
                {
                    if (string.Equals(options.Command, "animator", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!TryReadFloat(args, ref index, arg, out var animatorDuration, out error)) return false;
                        options.AnimatorDuration = animatorDuration;
                        options.AnimatorDurationSet = true;
                        continue;
                    }

                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleDuration = value;
                    options.ParticleDurationSet = true;
                    continue;
                }

                if (arg == "--time")
                {
                    if (!string.Equals(options.Command, "animator", StringComparison.OrdinalIgnoreCase))
                    {
                        error = "--time is only supported by animator commands.";
                        return false;
                    }

                    if (!TryReadFloat(args, ref index, arg, out var animatorSampleTime, out error)) return false;
                    options.AnimatorSampleTime = animatorSampleTime;
                    options.AnimatorSampleTimeSet = true;
                    continue;
                }

                if (arg == "--loop")
                {
                    if (string.Equals(options.Command, "animator", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!TryReadOptionalBool(args, ref index, true, out var animatorLoop, out error)) return false;
                        options.AnimatorLoop = animatorLoop;
                        options.AnimatorLoopSet = true;
                        continue;
                    }

                    if (!TryReadOptionalBool(args, ref index, true, out var value, out error)) return false;
                    options.ParticleLoop = value;
                    options.ParticleLoopSet = true;
                    continue;
                }

                if (arg == "--start-lifetime" || arg == "--lifetime")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleStartLifetime = value;
                    options.ParticleStartLifetimeSet = true;
                    continue;
                }

                if (arg == "--start-speed" || arg == "--speed")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleStartSpeed = value;
                    options.ParticleStartSpeedSet = true;
                    continue;
                }

                if (arg == "--start-size" || arg == "--size")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleStartSize = value;
                    options.ParticleStartSizeSet = true;
                    continue;
                }

                if (arg == "--emission-rate" || arg == "--rate")
                {
                    if (!TryReadFloat(args, ref index, arg, out var value, out error)) return false;
                    options.ParticleEmissionRate = value;
                    options.ParticleEmissionRateSet = true;
                    continue;
                }

                if (arg == "--max-particles")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var maxParticles) || maxParticles <= 0)
                    {
                        error = "--max-particles requires a positive integer.";
                        return false;
                    }

                    options.ParticleMaxParticles = maxParticles;
                    options.ParticleMaxParticlesSet = true;
                    continue;
                }

                if (arg == "--json" || arg == "--json-file")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a visual-ui json path.";
                        return false;
                    }

                    options.JsonFile = args[index];
                    continue;
                }

                if (arg == "--prefab" || arg == "--prefab-path")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a prefab path.";
                        return false;
                    }

                    options.PrefabPath = args[index];
                    continue;
                }

                if (arg == "--vm" || arg == "--vm-name")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a VM type name.";
                        return false;
                    }

                    options.VmName = args[index];
                    continue;
                }

                if (arg == "--manifest" || arg == "--asset-manifest")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires an asset-manifest path.";
                        return false;
                    }

                    options.AssetManifestPath = args[index];
                    continue;
                }

                if (arg == "--whitebox")
                {
                    options.Whitebox = true;
                    continue;
                }
                if (arg == "--dry-run")
                {
                    options.DryRun = true;
                    continue;
                }

                if (arg == "--validate-existing")
                {
                    options.ValidateExisting = true;
                    continue;
                }

                if (arg == "--recursive")
                {
                    options.Recursive = true;
                    continue;
                }

                if (arg == "--profile")
                {
                    if (++index >= args.Length)
                    {
                        error = "--profile requires widget, fancy-view, or fancy-view-cell.";
                        return false;
                    }

                    options.PrefabProfile = args[index];
                    continue;
                }

                if (arg == "--sheet")
                {
                    if (++index >= args.Length)
                    {
                        error = "--sheet requires a worksheet name.";
                        return false;
                    }

                    options.ExcelSheet = args[index];
                    continue;
                }

                if (arg == "--table")
                {
                    if (++index >= args.Length)
                    {
                        error = "--table requires a table name.";
                        return false;
                    }

                    options.ExcelTable = args[index];
                    continue;
                }

                if (arg == "--root")
                {
                    if (++index >= args.Length)
                    {
                        error = "--root requires a folder path.";
                        return false;
                    }

                    options.ExcelRoot = args[index];
                    continue;
                }

                if (arg == "--row")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var row))
                    {
                        error = "--row requires an Excel row number.";
                        return false;
                    }

                    options.ExcelRow = row;
                    options.ExcelRowSet = true;
                    continue;
                }

                if (arg == "--index")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var dataIndex))
                    {
                        error = "--index requires a zero-based data row index.";
                        return false;
                    }

                    if (string.Equals(options.Command, "terrain", StringComparison.OrdinalIgnoreCase))
                    {
                        options.TerrainLayerIndex = dataIndex;
                        options.TerrainLayerIndexSet = true;
                    }
                    else
                    {
                        options.ExcelIndex = dataIndex;
                        options.ExcelIndexSet = true;
                    }
                    continue;
                }

                if (arg == "--col" || arg == "--column")
                {
                    if (++index >= args.Length)
                    {
                        error = arg + " requires a column name or letter.";
                        return false;
                    }

                    options.ExcelColumn = args[index];
                    continue;
                }

                if (arg == "--field")
                {
                    if (++index >= args.Length)
                    {
                        error = "--field requires a header field name.";
                        return false;
                    }

                    options.ExcelField = args[index];
                    continue;
                }

                if (arg == "--key")
                {
                    if (++index >= args.Length)
                    {
                        error = "--key requires a key value.";
                        return false;
                    }

                    options.ExcelKey = args[index];
                    continue;
                }

                if (arg == "--key-field")
                {
                    if (++index >= args.Length)
                    {
                        error = "--key-field requires a header field name.";
                        return false;
                    }

                    options.ExcelKeyField = args[index];
                    continue;
                }

                if (arg == "--where")
                {
                    if (++index >= args.Length)
                    {
                        error = "--where requires an expression, such as \"id=1001\".";
                        return false;
                    }

                    options.ExcelWhere = args[index];
                    continue;
                }

                if (arg == "--values-json")
                {
                    if (++index >= args.Length)
                    {
                        error = "--values-json requires a JSON object.";
                        return false;
                    }

                    options.ExcelValuesJson = args[index];
                    continue;
                }

                if (arg == "--header-row")
                {
                    if (++index >= args.Length || !int.TryParse(args[index], out var headerRow) || headerRow < 1)
                    {
                        error = "--header-row requires a 1-based Excel row number.";
                        return false;
                    }

                    options.ExcelHeaderRow = headerRow;
                    options.ExcelHeaderRowSet = true;
                    continue;
                }

                if (arg == "--controller")
                {
                    if (++index >= args.Length)
                    {
                        error = "--controller requires an Assets/... .controller path.";
                        return false;
                    }

                    options.AnimatorControllerPath = args[index];
                    continue;
                }

                if (arg == "--clip")
                {
                    if (++index >= args.Length)
                    {
                        error = "--clip requires an Assets/... .anim path.";
                        return false;
                    }

                    options.AnimationClipPath = args[index];
                    continue;
                }

                if (arg == "--parameter")
                {
                    if (++index >= args.Length)
                    {
                        error = "--parameter requires a parameter name.";
                        return false;
                    }

                    options.AnimatorParameterName = args[index];
                    continue;
                }

                if (arg == "--default")
                {
                    if (++index >= args.Length)
                    {
                        error = "--default requires a value.";
                        return false;
                    }

                    options.AnimatorDefaultValue = args[index];
                    continue;
                }

                if (arg == "--state")
                {
                    if (++index >= args.Length)
                    {
                        error = "--state requires a state name.";
                        return false;
                    }

                    options.AnimatorStateName = args[index];
                    continue;
                }

                if (arg == "--from")
                {
                    if (++index >= args.Length)
                    {
                        error = "--from requires a state name.";
                        return false;
                    }

                    options.AnimatorFromState = args[index];
                    continue;
                }

                if (arg == "--to")
                {
                    if (++index >= args.Length)
                    {
                        error = "--to requires a state or transition name.";
                        return false;
                    }

                    if (string.Equals(options.Command, "jira", StringComparison.OrdinalIgnoreCase))
                    {
                        options.JiraTransitionTo = args[index];
                    }
                    else
                    {
                        options.AnimatorToState = args[index];
                    }
                    continue;
                }

                if (arg == "--site")
                {
                    if (++index >= args.Length)
                    {
                        error = "--site requires a Jira base url.";
                        return false;
                    }

                    options.JiraSite = args[index];
                    continue;
                }

                if (arg == "--token")
                {
                    if (++index >= args.Length)
                    {
                        error = "--token requires a Jira token.";
                        return false;
                    }

                    options.JiraToken = args[index];
                    continue;
                }

                if (arg == "--jql")
                {
                    if (++index >= args.Length)
                    {
                        error = "--jql requires a JQL expression.";
                        return false;
                    }

                    options.JiraJql = args[index];
                    continue;
                }

                if (arg == "--summary")
                {
                    if (++index >= args.Length)
                    {
                        error = "--summary requires text.";
                        return false;
                    }

                    options.JiraSummary = args[index];
                    continue;
                }

                if (arg == "--description")
                {
                    if (++index >= args.Length)
                    {
                        error = "--description requires text.";
                        return false;
                    }

                    options.JiraDescription = args[index];
                    continue;
                }

                if (arg == "--body")
                {
                    if (++index >= args.Length)
                    {
                        error = "--body requires text.";
                        return false;
                    }

                    options.JiraBody = args[index];
                    continue;
                }

                if (arg == "--condition")
                {
                    if (++index >= args.Length)
                    {
                        error = "--condition requires \"Param Greater 0.1\" or \"Flag If\".";
                        return false;
                    }

                    options.AnimatorCondition = args[index];
                    continue;
                }

                if (arg == "--keys")
                {
                    if (++index >= args.Length)
                    {
                        error = "--keys requires keyframes such as 0:0,0.5:1,1:0.";
                        return false;
                    }

                    options.AnimatorKeys = args[index];
                    continue;
                }

                if (arg == "--no-scene-replace")
                {
                    options.ReplaceSceneInstance = false;
                    continue;
                }

                if (string.IsNullOrEmpty(options.Command))
                {
                    options.Command = arg;
                    continue;
                }

                if (string.Equals(options.Command, "search", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.Search))
                {
                    options.Search = arg;
                    continue;
                }

                if (string.Equals(options.Command, "run", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.Action))
                {
                    options.Action = arg;
                    continue;
                }

                if (string.Equals(options.Command, "components", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.Action))
                {
                    options.Action = arg;
                    continue;
                }

                if (string.Equals(options.Command, "asset", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }

                    // For "asset search <query>" the second positional is the query string.
                    if (string.Equals(options.Action, "search", StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrEmpty(options.Search))
                    {
                        options.Search = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "ugui", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.UiName))
                {
                    options.UiName = arg;
                    continue;
                }

                if (string.Equals(options.Command, "widget-metadata", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.UiName))
                {
                    options.UiName = arg;
                    continue;
                }

                if (string.Equals(options.Command, "particle", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }

                    if (string.Equals(options.Action, "create", StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrEmpty(options.ParticleName))
                    {
                        options.ParticleName = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "prefab", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "animator", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }

                    if (string.IsNullOrEmpty(options.AnimatorOperation))
                    {
                        options.AnimatorOperation = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "excel", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }

                    if (string.IsNullOrEmpty(options.ExcelTable)
                        && string.IsNullOrEmpty(options.ExcelFilePath)
                        && !arg.StartsWith("--", StringComparison.Ordinal))
                    {
                        options.ExcelTable = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "terrain", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "jira", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }

                    if (string.Equals(options.Action, "config", StringComparison.OrdinalIgnoreCase)
                        && string.IsNullOrEmpty(options.JiraOperation))
                    {
                        options.JiraOperation = arg;
                        continue;
                    }

                    if (string.IsNullOrEmpty(options.JiraIssueKey)
                        && !arg.StartsWith("--", StringComparison.Ordinal))
                    {
                        options.JiraIssueKey = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "mesh", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(options.Action))
                    {
                        options.Action = arg;
                        continue;
                    }
                }

                if (string.Equals(options.Command, "screenshot", StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrEmpty(options.ScreenshotPath)
                    && string.IsNullOrEmpty(options.AssetDstPath))
                {
                    options.ScreenshotPath = arg;
                    continue;
                }

                error = "Unexpected argument: " + arg;
                return false;
            }

            if (string.IsNullOrEmpty(options.Command))
            {
                error = "Missing command.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the provided argument asks for usage help.
        /// </summary>
        /// <param name="arg">The argument to inspect.</param>
        /// <returns>True when the argument is a supported help token.</returns>
        static bool IsHelp(string arg)
        {
            return arg == "-h" || arg == "--help" || arg == "help";
        }

        /// <summary>
        /// Writes command usage details to standard output.
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("usage:");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] measure --path <active container path> [--name <direct child>] [--child-name <relative target>]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] ping");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] console [--count <n>] [--type all|log|warning|error] [--search <text|pattern>]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] search <name|pattern> [--count <n>]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] search --name <name|pattern> [--count <n>]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] modify ((--id <id>|--path <path>)|--asset <prefab> --path <path>) --component <type> --property <path> --value <value>");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] run <status|play|pause|resume|stop>");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] compile [--wait]");
            Console.WriteLine("    compile requests a script recompile in the Editor; --wait blocks until compilation finishes (up to 5 minutes).");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] save [--scene <asset path>]");
            Console.WriteLine("    save mirrors File > Save (Ctrl+S): writes every dirty open scene with a known asset path, then AssetDatabase.SaveAssets().");
            Console.WriteLine("    --scene <path> restricts the save to a single loaded scene matched by scene.path (e.g. Assets/Scenes/SampleScene.unity).");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] components <add|remove> ((--id <id>|--path <path>)|--asset <prefab> --path <path>) --component <type>");
            Console.WriteLine("    components add/remove a Component on a scene or prefab GameObject. --component accepts short name (Rigidbody),");
            Console.WriteLine("    full name (UnityEngine.Rigidbody), or assembly-qualified (\"My.Type, My.Asm\"). Goes through Undo and marks the scene dirty.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] asset <action> [options]");
            Console.WriteLine("    actions: search <query> [--type <Type>] [--count <n>]");
            Console.WriteLine("             refresh                                 (synchronous asset database import)");
            Console.WriteLine("             create-folder --dst <Assets/Sub/...>          (creates intermediate folders too)");
            Console.WriteLine("             import        --src <disk path> --dst <Assets/...>");
            Console.WriteLine("             delete        --src <Assets/...>              (--dst also accepted)");
            Console.WriteLine("             move          --src <Assets/...> --dst <Assets/...>");
            Console.WriteLine("             copy          --src <Assets/...> --dst <Assets/...>");
            Console.WriteLine("    All asset paths must live under Assets/ or Packages/ and use forward slashes.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] ugui <UIName> [--json <visual-ui.json>] [--manifest <asset-manifest.json>] [--prefab <Assets/...prefab>] [--dry-run] [--no-scene-replace]");
            Console.WriteLine("    Build or update a UGUI prefab from the unity-cli Web visual-ui workflow. Uses asset-manifest.json and assets_png; does not parse PSD.");
            Console.WriteLine("    Contract assembly: ugui --json <ui-production-contract/1 or prefab-assembly input> --asset-manifest <ui-contract-resources/1.json> [--prefab <window path>] [--dry-run] [--replace].");
            Console.WriteLine("    Add --output <directory> for one-command production, saved-prefab state previews and build-report.json.");
            Console.WriteLine("  unitycli ui-resources --json <contract.json> --output <resources.json> [--prefab <window path>] [--asset-manifest <slot-path-receipts.json>]");
            Console.WriteLine("  unitycli ui-prepare --json <contract.json> --asset-manifest <source-receipts.json> --output <resources.json> [--replace]");
            Console.WriteLine("    Extract resource information, or compute the assembly mapping from actual slotId/path receipts. Does not invoke image generation.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] widget-metadata <UIName> [--vm <VMName>] [--prefab <Assets/...prefab>] [--dry-run|--validate-existing]");
            Console.WriteLine("    Generate Oathx.Widget Metadata, or validate that existing className/classGuid bindings resolve to the expected VM MonoScript.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] particle create <Name> [--path <parent scene path>] [--position x,y,z] [--prefab <Assets/...prefab>] [--material <Assets/...mat>] [--texture <Assets/...png>] [--duration <s>] [--loop [true|false]] [--start-lifetime <s>] [--start-speed <n>] [--start-size <n>] [--emission-rate <n>] [--max-particles <n>] [--shape cone|sphere|hemisphere|circle|box] [--shape-scale x,y,z] [--shape-radius <n>] [--shape-angle <deg>] [--velocity-x min,max] [--velocity-y min,max] [--velocity-z min,max] [--noise-strength <n>] [--noise-frequency <n>] [--noise-scroll-speed <n>] [--rotation-z min,max] [--size-over-lifetime start,end] [--fade-out] [--fade-start 0..1] [--sorting-order <n>] [--start-color <#RRGGBB|#RRGGBBAA>] [--dry-run] [--no-scene-replace]");
            Console.WriteLine("    Create a ParticleSystem prefab and optionally place a fresh instance in the open scene. --path parents the instance under a scene object.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] prefab create --json <task-layout.json> --prefab <Assets/...prefab> [--dry-run] [--replace]");
            Console.WriteLine("    Create task-defined UGUI hierarchy directly in an isolated preview scene; never replaces open scene objects.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] prefab attach --parent-prefab <Assets/...prefab> --parent-path <path> --child-prefab <Assets/...prefab> [--child-name <name>] [--local-position x,y,z] [--local-rotation x,y,z] [--local-scale x,y,z] [--layer <name|index>|--keep-layer] [--replace|--no-replace] [--dry-run] [--no-scene-replace]");
            Console.WriteLine("    Attach one prefab as a nested child under a node inside another prefab asset. Also syncs the matching open scene instance by default.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] prefab group --prefab <Assets/...prefab> --group-name <name> --children <a,b,c> [--parent-path <path>] [--anchor xmin,ymin,xmax,ymax] [--pivot x,y] [--group-size w,h] [--position x,y] [--sibling-index <n>] [--dry-run] [--no-scene-replace]");
            Console.WriteLine("    Create a container node under a parent inside a prefab and move the listed direct children into it, preserving their local transforms. Rect defaults to full-stretch with center pivot.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] prefab visual-import --prefab <Assets/...prefab> --dst <Assets/...prefab> [--scene <Assets/...unity>] [--path <root name>] [--local-position x,y,z] [--local-rotation x,y,z] [--local-scale x,y,z] [--dry-run] [--no-scene-replace]");
            Console.WriteLine("    Save a visual-only copy of a prefab by removing missing/custom MonoBehaviour scripts, then optionally place it as a scene root.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] prefab inspect --prefab <Assets/...prefab> [--recursive] [--profile widget|fancy-view|fancy-view-cell]");
            Console.WriteLine("    Inspect prefab hierarchy, components, serialized object references, missing scripts, and optional Widget/FancyView invariants without modifying the asset.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator controller create --controller <Assets/...controller> [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator controller import-json --controller <new Assets/...controller> --json <animation-controller/1.json> [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator parameter set --controller <Assets/...controller> --parameter <name> --type Float|Int|Bool|Trigger [--default <value>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator clip create --clip <Assets/...anim> [--duration <s>] [--loop [true|false]] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator clip set-curve --clip <Assets/...anim> [--path <relative/path>] --component <type> --property <property> --keys 0:0,1:1 [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator clip sample --clip <Assets/...anim> --path <scene object path> [--time <s>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator state add --controller <Assets/...controller> --layer <name> --state <name> [--clip <Assets/...anim>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator transition add --controller <Assets/...controller> --layer <name> --from <state> --to <state> [--condition \"Param Greater 0.1\"] [--duration <s>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator bind --path <scene object path> --controller <Assets/...controller> [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] animator inspect --controller <Assets/...controller>|--clip <Assets/...anim>");
            Console.WriteLine("    Create and edit AnimatorController assets, AnimationClip curves, transitions, parameters, and scene Animator bindings through Unity Editor APIs.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] excel <list|schema|row|cell|query|set|insert|delete|validate> [--file <xlsx>|--table <name>] [--root <folder>] [--sheet <name>] [--header-row <n>] [--row <n>|--index <n>|--key-field <field> --key <value>] [--field <name>|--col <A>] [--where <expr>] [--value <text>] [--values-json <object>] [--count <n>] [--dry-run]");
            Console.WriteLine("    Read, query, validate, and conservatively edit .xlsx config tables. Write operations support dry-run previews.");
            Console.WriteLine("  unitycli [--format human|json] jira <config|auth|get|search|mine|projects|issue-types|comment|transitions|transition|create|update> [--site <url>] [--token <token>] [--jql <expr>] [--project <key>] [--type <IssueType>] [--summary <text>] [--description <text>] [--body <text>] [--to <transition>] [--count <n>] [--dry-run]");
            Console.WriteLine("    Access Jira through the REST API with only a base address and Bearer access token. Local config is stored outside the Unity project.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] terrain inspect [--scene <Assets/...unity>] [--path <terrain object>|--data <Assets/...asset>]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] terrain create-data --data <Assets/...asset> [--terrain-size x,y,z] [--heightmap-resolution <n>] [--alphamap-resolution <n>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] terrain create-layer --layer <Assets/...terrainlayer> --texture <Assets/...png> [--normal <Assets/...png>] [--tile-size x,y] [--tile-offset x,y] [--smoothness <n>] [--metallic <n>] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] terrain assign-layer --data <Assets/...asset> --layer <Assets/...terrainlayer> [--index <n>|--append] [--dry-run]");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] terrain bind-data --scene <Assets/...unity> --path <terrain object> --data <Assets/...asset> [--dry-run]");
            Console.WriteLine("    Inspect and modify Unity TerrainData, TerrainLayer assets, and scene Terrain bindings through Unity Editor APIs.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] screenshot [--dst <root-relative folder-or-file.png>] [--output <path>] [--super-size <n>]");
            Console.WriteLine("    Capture the current Unity Game view to a PNG under a project-root folder beside Assets. Default: UnityCLI-Screenshots/GameView_<timestamp>.png.");
            Console.WriteLine("  unitycli [--project <path>] [--format human|json] call --invoke-type <CLR type> --invoke-member <method> [--args-json <json array>] [--arg <token> ...]");
            Console.WriteLine("    call requires Play Mode: resolve type from loaded assemblies, then MethodInfo.Invoke; static uses null target, instance uses the single matching scene object.");
            Console.WriteLine("  alias: --serach is accepted for --search");
        }

        static bool TryReadFloat(string[] args, ref int index, string name, out float value, out string error)
        {
            value = 0f;
            error = string.Empty;
            if (++index >= args.Length || !float.TryParse(args[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value))
            {
                error = name + " requires a number.";
                return false;
            }

            return true;
        }

        static bool TryReadOptionalBool(string[] args, ref int index, bool defaultValue, out bool value, out string error)
        {
            value = defaultValue;
            error = string.Empty;
            if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                return true;
            }

            index++;
            if (bool.TryParse(args[index], out value)) return true;
            if (args[index] == "1")
            {
                value = true;
                return true;
            }

            if (args[index] == "0")
            {
                value = false;
                return true;
            }

            error = "--loop requires true or false when a value is provided.";
            return false;
        }
    }
}
