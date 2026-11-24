namespace UnityCLI
{
    /// <summary>
    /// Defines the contract for a command that can be invoked by the UnityCLI executable.
    /// </summary>
    internal interface ICommand
    {
        /// <summary>
        /// Gets the command name used by the command-line dispatcher.
        /// </summary>
        string Name
        { get; }

        /// <summary>
        /// Executes the command by using the parsed command-line options.
        /// </summary>
        /// <param name="options">The parsed command-line options.</param>
        /// <returns>A task that completes with the process exit code.</returns>
        Task<int> ExecuteAsync(CliOptions options);
    }

    /// <summary>
    /// Represents parsed options shared by UnityCLI commands.
    /// </summary>
    internal sealed class CliOptions
    {
        public bool Whitebox
        { get; set; }
        public string? ExportSourceKey { get; set; }
        /// <summary>
        /// Gets or sets the command name selected by the user.
        /// </summary>
        public string Command
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the explicit Unity project root.
        /// </summary>
        public string? ProjectPath
        { get; set; }
        /// <summary>
        /// Gets or sets whether raw JSON should be written to stdout.
        /// </summary>
        public bool JsonOutput
        { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of entries requested by list commands.
        /// </summary>
        public int Count
        { get; set; } = 20;
        /// <summary>
        /// Gets or sets the filter type used by console and asset commands.
        /// </summary>
        public string Type
        { get; set; } = "all";
        /// <summary>
        /// Gets or sets the search query or object name pattern.
        /// </summary>
        public string Search
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the target Unity asset path.
        /// </summary>
        public string AssetPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the target Unity instance id.
        /// </summary>
        public int InstanceId
        { get; set; }
        /// <summary>
        /// Gets or sets the target scene hierarchy path.
        /// </summary>
        public string ObjectPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the target component type name.
        /// </summary>
        public string Component
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the target serialized property path.
        /// </summary>
        public string Property
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the serialized property value to apply.
        /// </summary>
        public string Value
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the subcommand action.
        /// </summary>
        public string Action
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the CLR type used by the reflection call command.
        /// </summary>
        public string InvokeType
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the method name used by the reflection call command.
        /// </summary>
        public string InvokeMember
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the JSON array of reflection-call arguments.
        /// </summary>
        public string InvokeArgsJson
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets the positional arguments passed to a reflected invocation.
        /// </summary>
        public List<string> InvokeArgList
        { get; } = new List<string>();
        /// <summary>
        /// Gets or sets whether the compile command waits for Unity compilation to finish.
        /// </summary>
        public bool CompileWait
        { get; set; }
        /// <summary>
        /// Gets or sets the scene path to save.
        /// </summary>
        public string SaveScenePath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the source path for asset operations.
        /// </summary>
        public string AssetSrcPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the destination path for asset operations.
        /// </summary>
        public string AssetDstPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the UI name used by the UGUI prefab workflow.
        /// </summary>
        public string UiName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the VM type name used by widget metadata generation.
        /// </summary>
        public string VmName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the visual-ui JSON path used by the UGUI prefab workflow.
        /// </summary>
        public string JsonFile
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the prefab output path.
        /// </summary>
        public string PrefabPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the asset manifest path used by the UGUI prefab workflow.
        /// </summary>
        public string AssetManifestPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets whether the UGUI command should validate without writing files.
        /// </summary>
        public bool DryRun
        { get; set; }
        /// <summary>
        /// 获取或设置是否在不重新生成的情况下校验现有 Widget Metadata。
        /// </summary>
        public bool ValidateExisting
        { get; set; }
        /// <summary>
        /// 获取或设置 Prefab 检查是否递归遍历全部子节点。
        /// </summary>
        public bool Recursive
        { get; set; }
        /// <summary>
        /// 获取或设置可选的 Prefab 检查档案。
        /// </summary>
        public string PrefabProfile
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets whether generated prefabs replace the open scene instance.
        /// </summary>
        public bool ReplaceSceneInstance
        { get; set; } = true;
        /// <summary>
        /// Gets or sets the screenshot output path.
        /// </summary>
        public string ScreenshotPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the screenshot supersize multiplier.
        /// </summary>
        public int ScreenshotSuperSize
        { get; set; } = 1;
        /// <summary>
        /// Gets or sets the generated particle object name.
        /// </summary>
        public string ParticleName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the material path used by the particle renderer.
        /// </summary>
        public string ParticleMaterialPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the texture path used to create a particle material.
        /// </summary>
        public string ParticleTexturePath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle shape module type.
        /// </summary>
        public string ParticleShape
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle start color value.
        /// </summary>
        public string ParticleStartColor
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle system duration.
        /// </summary>
        public float ParticleDuration
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle duration was explicitly provided.
        /// </summary>
        public bool ParticleDurationSet
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle system loops.
        /// </summary>
        public bool ParticleLoop
        { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the particle loop option was explicitly provided.
        /// </summary>
        public bool ParticleLoopSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle start lifetime.
        /// </summary>
        public float ParticleStartLifetime
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle start lifetime was explicitly provided.
        /// </summary>
        public bool ParticleStartLifetimeSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle start speed.
        /// </summary>
        public float ParticleStartSpeed
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle start speed was explicitly provided.
        /// </summary>
        public bool ParticleStartSpeedSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle start size.
        /// </summary>
        public float ParticleStartSize
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle start size was explicitly provided.
        /// </summary>
        public bool ParticleStartSizeSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle emission rate over time.
        /// </summary>
        public float ParticleEmissionRate
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle emission rate was explicitly provided.
        /// </summary>
        public bool ParticleEmissionRateSet
        { get; set; }
        /// <summary>
        /// Gets or sets the maximum particle count.
        /// </summary>
        public int ParticleMaxParticles
        { get; set; }
        /// <summary>
        /// Gets or sets whether the maximum particle count was explicitly provided.
        /// </summary>
        public bool ParticleMaxParticlesSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle object local position.
        /// </summary>
        public string ParticlePosition
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle shape scale.
        /// </summary>
        public string ParticleShapeScale
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle shape radius.
        /// </summary>
        public float ParticleShapeRadius
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle shape radius was explicitly provided.
        /// </summary>
        public bool ParticleShapeRadiusSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle shape angle.
        /// </summary>
        public float ParticleShapeAngle
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle shape angle was explicitly provided.
        /// </summary>
        public bool ParticleShapeAngleSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle velocity over lifetime X curve.
        /// </summary>
        public string ParticleVelocityX
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle velocity over lifetime Y curve.
        /// </summary>
        public string ParticleVelocityY
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle velocity over lifetime Z curve.
        /// </summary>
        public string ParticleVelocityZ
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle noise strength.
        /// </summary>
        public float ParticleNoiseStrength
        { get; set; }
        /// <summary>
        /// Gets or sets whether particle noise strength was explicitly provided.
        /// </summary>
        public bool ParticleNoiseStrengthSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle noise frequency.
        /// </summary>
        public float ParticleNoiseFrequency
        { get; set; }
        /// <summary>
        /// Gets or sets whether particle noise frequency was explicitly provided.
        /// </summary>
        public bool ParticleNoiseFrequencySet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle noise scroll speed.
        /// </summary>
        public float ParticleNoiseScrollSpeed
        { get; set; }
        /// <summary>
        /// Gets or sets whether particle noise scroll speed was explicitly provided.
        /// </summary>
        public bool ParticleNoiseScrollSpeedSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle rotation over lifetime Z curve.
        /// </summary>
        public string ParticleRotationZ
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the particle size over lifetime curve.
        /// </summary>
        public string ParticleSizeOverLifetime
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets whether the particle alpha fades out.
        /// </summary>
        public bool ParticleFadeOut
        { get; set; }
        /// <summary>
        /// Gets or sets the normalized point where particle fading starts.
        /// </summary>
        public float ParticleFadeStart
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle fade start was explicitly provided.
        /// </summary>
        public bool ParticleFadeStartSet
        { get; set; }
        /// <summary>
        /// Gets or sets the particle renderer sorting order.
        /// </summary>
        public int ParticleSortingOrder
        { get; set; }
        /// <summary>
        /// Gets or sets whether the particle sorting order was explicitly provided.
        /// </summary>
        public bool ParticleSortingOrderSet
        { get; set; }
        /// <summary>
        /// Gets or sets the parent prefab path for prefab composition.
        /// </summary>
        public string ParentPrefabPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the child prefab path for prefab composition.
        /// </summary>
        public string ChildPrefabPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the parent object path for prefab composition.
        /// </summary>
        public string ParentObjectPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the child object name for prefab composition.
        /// </summary>
        public string ChildName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the local position applied to a composed child.
        /// </summary>
        public string LocalPosition
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the local rotation applied to a composed child.
        /// </summary>
        public string LocalRotation
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the local scale applied to a composed child.
        /// </summary>
        public string LocalScale
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets whether an existing child with the same name should be replaced.
        /// </summary>
        public bool ReplaceExistingChild
        { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the child replacement option was explicitly provided.
        /// </summary>
        public bool ReplaceExistingChildSet
        { get; set; }
        /// <summary>
        /// 获取或设置 prefab group 新建的容器节点名。
        /// </summary>
        public string GroupName
        { get; set; } = string.Empty;
        /// <summary>
        /// 获取或设置 prefab group 待迁入的子节点名列表（逗号分隔）。
        /// </summary>
        public string GroupChildren
        { get; set; } = string.Empty;
        /// <summary>
        /// 获取或设置 prefab group 容器的锚点参数（xmin,ymin,xmax,ymax）。
        /// </summary>
        public string GroupAnchor
        { get; set; } = string.Empty;
        /// <summary>
        /// 获取或设置 prefab group 容器的轴心参数（x,y）。
        /// </summary>
        public string GroupPivot
        { get; set; } = string.Empty;
        /// <summary>
        /// 获取或设置 prefab group 容器的尺寸参数（w,h）。
        /// </summary>
        public string GroupSize
        { get; set; } = string.Empty;
        /// <summary>
        /// 获取或设置 prefab group 容器插入的兄弟序号；-1 表示自动。
        /// </summary>
        public int GroupSiblingIndex
        { get; set; } = -1;
        /// <summary>
        /// Gets or sets the layer assigned to imported or generated objects.
        /// </summary>
        public string Layer
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets whether imported or generated objects keep their original layer.
        /// </summary>
        public bool KeepLayer
        { get; set; }
        /// <summary>
        /// Gets or sets the animator sub-operation name.
        /// </summary>
        public string AnimatorOperation
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the AnimatorController asset path.
        /// </summary>
        public string AnimatorControllerPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the AnimationClip asset path.
        /// </summary>
        public string AnimationClipPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator layer name.
        /// </summary>
        public string AnimatorLayer
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator parameter name.
        /// </summary>
        public string AnimatorParameterName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator parameter type.
        /// </summary>
        public string AnimatorParameterType
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator parameter default value.
        /// </summary>
        public string AnimatorDefaultValue
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator state name.
        /// </summary>
        public string AnimatorStateName
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the source animator state name for a transition.
        /// </summary>
        public string AnimatorFromState
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the destination animator state name for a transition.
        /// </summary>
        public string AnimatorToState
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator transition condition expression.
        /// </summary>
        public string AnimatorCondition
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animation curve key expression.
        /// </summary>
        public string AnimatorKeys
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the animator transition or clip duration.
        /// </summary>
        public float AnimatorDuration
        { get; set; }
        /// <summary>
        /// Gets or sets whether animator duration was explicitly provided.
        /// </summary>
        public bool AnimatorDurationSet
        { get; set; }
        /// <summary>
        /// Gets or sets the animation sample time.
        /// </summary>
        public float AnimatorSampleTime
        { get; set; }
        /// <summary>
        /// Gets or sets whether animation sample time was explicitly provided.
        /// </summary>
        public bool AnimatorSampleTimeSet
        { get; set; }
        /// <summary>
        /// Gets or sets whether generated animation clips should loop.
        /// </summary>
        public bool AnimatorLoop
        { get; set; } = true;
        /// <summary>
        /// Gets or sets whether the animation loop option was explicitly provided.
        /// </summary>
        public bool AnimatorLoopSet
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainData asset path used by terrain commands.
        /// </summary>
        public string TerrainDataPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the TerrainLayer asset path used by terrain commands.
        /// </summary>
        public string TerrainLayerPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the diffuse texture asset path used by terrain layer creation.
        /// </summary>
        public string TerrainTexturePath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the normal texture asset path used by terrain layer creation.
        /// </summary>
        public string TerrainNormalPath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the TerrainLayer tile size as x,y.
        /// </summary>
        public string TerrainTileSize
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the TerrainLayer tile offset as x,y.
        /// </summary>
        public string TerrainTileOffset
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the TerrainData size as x,y,z.
        /// </summary>
        public string TerrainSize
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the TerrainData heightmap resolution.
        /// </summary>
        public int TerrainHeightmapResolution
        { get; set; }
        /// <summary>
        /// Gets or sets whether the TerrainData heightmap resolution was explicitly provided.
        /// </summary>
        public bool TerrainHeightmapResolutionSet
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainData alphamap resolution.
        /// </summary>
        public int TerrainAlphamapResolution
        { get; set; }
        /// <summary>
        /// Gets or sets whether the TerrainData alphamap resolution was explicitly provided.
        /// </summary>
        public bool TerrainAlphamapResolutionSet
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainData base map resolution.
        /// </summary>
        public int TerrainBaseMapResolution
        { get; set; }
        /// <summary>
        /// Gets or sets whether the TerrainData base map resolution was explicitly provided.
        /// </summary>
        public bool TerrainBaseMapResolutionSet
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainLayer assignment index.
        /// </summary>
        public int TerrainLayerIndex
        { get; set; }
        /// <summary>
        /// Gets or sets whether a TerrainLayer assignment index was explicitly provided.
        /// </summary>
        public bool TerrainLayerIndexSet
        { get; set; }
        /// <summary>
        /// Gets or sets whether assign-layer appends the layer.
        /// </summary>
        public bool TerrainAppendLayer
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainLayer smoothness.
        /// </summary>
        public float TerrainSmoothness
        { get; set; }
        /// <summary>
        /// Gets or sets whether TerrainLayer smoothness was explicitly provided.
        /// </summary>
        public bool TerrainSmoothnessSet
        { get; set; }
        /// <summary>
        /// Gets or sets the TerrainLayer metallic value.
        /// </summary>
        public float TerrainMetallic
        { get; set; }
        /// <summary>
        /// Gets or sets whether TerrainLayer metallic was explicitly provided.
        /// </summary>
        public bool TerrainMetallicSet
        { get; set; }
        /// <summary>
        /// Gets or sets the Excel file path.
        /// </summary>
        public string ExcelFilePath
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel sheet name.
        /// </summary>
        public string ExcelSheet
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the configured Excel table name.
        /// </summary>
        public string ExcelTable
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel config root folder.
        /// </summary>
        public string ExcelRoot
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the one-based Excel row number.
        /// </summary>
        public int ExcelRow
        { get; set; }
        /// <summary>
        /// Gets or sets whether an Excel row number was explicitly provided.
        /// </summary>
        public bool ExcelRowSet
        { get; set; }
        /// <summary>
        /// Gets or sets the zero-based Excel data row index.
        /// </summary>
        public int ExcelIndex
        { get; set; }
        /// <summary>
        /// Gets or sets whether an Excel data row index was explicitly provided.
        /// </summary>
        public bool ExcelIndexSet
        { get; set; }
        /// <summary>
        /// Gets or sets the Excel column name or letter.
        /// </summary>
        public string ExcelColumn
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel field name.
        /// </summary>
        public string ExcelField
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel lookup key.
        /// </summary>
        public string ExcelKey
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel lookup key field.
        /// </summary>
        public string ExcelKeyField
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the Excel query filter expression.
        /// </summary>
        public string ExcelWhere
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the JSON object used by Excel write operations.
        /// </summary>
        public string ExcelValuesJson
        { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the one-based Excel header row number.
        /// </summary>
        public int ExcelHeaderRow
        { get; set; }
        /// <summary>
        /// Gets or sets whether an Excel header row was explicitly provided.
        /// </summary>
        public bool ExcelHeaderRowSet
        { get; set; }
        public string JiraOperation { get; set; } = string.Empty;
        public string JiraSite { get; set; } = string.Empty;
        public string JiraToken { get; set; } = string.Empty;
        public string JiraIssueKey { get; set; } = string.Empty;
        public string JiraJql { get; set; } = string.Empty;
        public string JiraProjectKey { get; set; } = string.Empty;
        public string JiraIssueType { get; set; } = string.Empty;
        public string JiraSummary { get; set; } = string.Empty;
        public string JiraDescription { get; set; } = string.Empty;
        public string JiraBody { get; set; } = string.Empty;
        public string JiraTransitionTo { get; set; } = string.Empty;
    }
}
