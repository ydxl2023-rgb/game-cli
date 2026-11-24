using System;

namespace Oathx.UnityCLI.Editor
{
    /// <summary>
    /// Request payload sent from the standalone CLI process to the editor bridge.
    /// </summary>
    [Serializable]
    public sealed class UnityCliBridgeRequest
    {
        public string exportSourceKey = string.Empty;
        /// <summary>
        /// HTTP-like method used by the route, such as GET or POST.
        /// </summary>
        public string method = string.Empty;
        /// <summary>
        /// Bridge route path, such as /ping or /ugui.
        /// </summary>
        public string path = string.Empty;
        /// <summary>
        /// Endpoint token copied from endpoint.json.
        /// </summary>
        public string token = string.Empty;
        /// <summary>
        /// Maximum number of entries requested by list commands.
        /// </summary>
        public int count = 0;
        /// <summary>
        /// Command-specific filter type.
        /// </summary>
        public string type = string.Empty;
        /// <summary>
        /// Search text or wildcard pattern.
        /// </summary>
        public string search = string.Empty;
        /// <summary>
        /// Target prefab or asset path used by asset-aware commands.
        /// </summary>
        public string assetPath = string.Empty;
        /// <summary>
        /// Target Unity instance id.
        /// </summary>
        public int instanceId = 0;
        /// <summary>
        /// Target scene hierarchy path.
        /// </summary>
        public string objectPath = string.Empty;
        /// <summary>
        /// Target component type name.
        /// </summary>
        public string component = string.Empty;
        /// <summary>
        /// Target serialized property path.
        /// </summary>
        public string property = string.Empty;
        /// <summary>
        /// Serialized property value to apply.
        /// </summary>
        public string value = string.Empty;
        /// <summary>
        /// Command-specific action, such as play, add, import, or delete.
        /// </summary>
        public string action = string.Empty;
        /// <summary>
        /// CLR type name used by the reflection call command.
        /// </summary>
        public string invokeType = string.Empty;
        /// <summary>
        /// Method name used by the reflection call command.
        /// </summary>
        public string invokeMember = string.Empty;
        /// <summary>
        /// JSON array of arguments for the reflection call command.
        /// </summary>
        public string invokeArgsJson = string.Empty;
        /// <summary>
        /// Positional argument tokens for the reflection call command.
        /// </summary>
        public string[] invokeArgs = Array.Empty<string>();
        /// <summary>
        /// Whether the compile command waits for Unity compilation to finish.
        /// </summary>
        public bool compileWait;
        /// <summary>
        /// Whether the compile command only reads the current compilation state.
        /// </summary>
        public bool compileCheckOnly;
        /// <summary>
        /// Scene path to save.
        /// </summary>
        public string saveScenePath = string.Empty;
        /// Source path for asset operations.
        /// </summary>
        public string assetSrcPath = string.Empty;
        /// <summary>
        /// Destination path for asset operations.
        /// </summary>
        public string assetDstPath = string.Empty;
        /// <summary>
        /// UI name used by the UGUI prefab workflow.
        /// </summary>
        public string uiName = string.Empty;
        /// <summary>
        /// VM type name used by widget metadata generation.
        /// </summary>
        public string vmName = string.Empty;
        /// <summary>
        /// visual-ui JSON path used by the UGUI prefab workflow.
        /// </summary>
        public string jsonFile = string.Empty;
        /// <summary>
        /// Prefab output path for the UGUI prefab workflow.
        /// </summary>
        public string prefabPath = string.Empty;
        /// <summary>
        /// Asset manifest path for the UGUI prefab workflow.
        /// </summary>
        public string assetManifestPath = string.Empty;
        /// <summary>
        /// Whether the UGUI workflow validates without writing files.
        /// </summary>
        public bool dryRun;
        public bool whitebox;
        /// <summary>
        /// 是否在不重新生成的情况下校验现有 Widget Metadata。
        /// </summary>
        public bool validateExisting;
        /// <summary>
        /// Prefab 检查是否递归遍历全部子节点。
        /// </summary>
        public bool recursive;
        /// <summary>
        /// 可选的 Prefab 检查档案，例如 widget 或 fancy-view。
        /// </summary>
        public string prefabProfile = string.Empty;
        /// <summary>
        /// Whether a generated prefab replaces an open scene instance.
        /// </summary>
        public bool replaceSceneInstance = true;
        /// <summary>
        /// Screenshot output path.
        /// </summary>
        public string screenshotPath = string.Empty;
        /// <summary>
        /// Stores the screenshot supersize multiplier.
        /// </summary>
        public int screenshotSuperSize = 1;
        /// <summary>
        /// Particle prefab root name.
        /// </summary>
        public string particleName = string.Empty;
        /// <summary>
        /// Optional particle material asset path.
        /// </summary>
        public string particleMaterialPath = string.Empty;
        /// <summary>
        /// Optional particle texture asset path.
        /// </summary>
        public string particleTexturePath = string.Empty;
        /// <summary>
        /// ParticleSystem shape module type.
        /// </summary>
        public string particleShape = string.Empty;
        /// <summary>
        /// ParticleSystem start color.
        /// </summary>
        public string particleStartColor = string.Empty;
        /// <summary>
        /// ParticleSystem main.duration.
        /// </summary>
        public float particleDuration;
        public bool particleDurationSet;
        /// <summary>
        /// ParticleSystem main.loop.
        /// </summary>
        public bool particleLoop = true;
        public bool particleLoopSet;
        /// <summary>
        /// ParticleSystem main.startLifetime.
        /// </summary>
        public float particleStartLifetime;
        public bool particleStartLifetimeSet;
        /// <summary>
        /// ParticleSystem main.startSpeed.
        /// </summary>
        public float particleStartSpeed;
        public bool particleStartSpeedSet;
        /// <summary>
        /// ParticleSystem main.startSize.
        /// </summary>
        public float particleStartSize;
        public bool particleStartSizeSet;
        /// <summary>
        /// ParticleSystem emission.rateOverTime.
        /// </summary>
        public float particleEmissionRate;
        public bool particleEmissionRateSet;
        /// <summary>
        /// ParticleSystem main.maxParticles.
        /// </summary>
        public int particleMaxParticles;
        public bool particleMaxParticlesSet;
        public string particlePosition = string.Empty;
        public string particleShapeScale = string.Empty;
        public float particleShapeRadius;
        public bool particleShapeRadiusSet;
        public float particleShapeAngle;
        public bool particleShapeAngleSet;
        public string particleVelocityX = string.Empty;
        public string particleVelocityY = string.Empty;
        public string particleVelocityZ = string.Empty;
        public float particleNoiseStrength;
        public bool particleNoiseStrengthSet;
        public float particleNoiseFrequency;
        public bool particleNoiseFrequencySet;
        public float particleNoiseScrollSpeed;
        public bool particleNoiseScrollSpeedSet;
        public string particleRotationZ = string.Empty;
        public string particleSizeOverLifetime = string.Empty;
        public bool particleFadeOut;
        public float particleFadeStart;
        public bool particleFadeStartSet;
        public int particleSortingOrder;
        public bool particleSortingOrderSet;
        public string parentPrefabPath = string.Empty;
        public string childPrefabPath = string.Empty;
        public string parentObjectPath = string.Empty;
        public string childName = string.Empty;
        /// <summary>
        /// group 动作新建的容器节点名。
        /// </summary>
        public string groupName = string.Empty;
        /// <summary>
        /// group 动作待迁入容器的直接子节点名，逗号分隔。
        /// </summary>
        public string groupChildren = string.Empty;
        /// <summary>
        /// group 动作容器锚点参数，格式 xmin,ymin,xmax,ymax。
        /// </summary>
        public string groupAnchor = string.Empty;
        /// <summary>
        /// group 动作容器轴心参数，格式 x,y。
        /// </summary>
        public string groupPivot = string.Empty;
        /// <summary>
        /// group 动作容器尺寸参数，格式 w,h。
        /// </summary>
        public string groupSize = string.Empty;
        /// <summary>
        /// group 动作容器插入的兄弟序号，-1 表示自动。
        /// </summary>
        public int groupSiblingIndex = -1;
        public string localPosition = string.Empty;
        public string localRotation = string.Empty;
        public string localScale = string.Empty;
        public string layer = string.Empty;
        public bool keepLayer;
        public bool replaceExistingChild = true;
        public bool replaceExistingChildSet;
        public string animatorOperation = string.Empty;
        public string animatorControllerPath = string.Empty;
        public string animationClipPath = string.Empty;
        public string animatorLayer = string.Empty;
        public string animatorParameterName = string.Empty;
        public string animatorParameterType = string.Empty;
        public string animatorDefaultValue = string.Empty;
        public string animatorStateName = string.Empty;
        public string animatorFromState = string.Empty;
        public string animatorToState = string.Empty;
        public string animatorCondition = string.Empty;
        public string animatorKeys = string.Empty;
        public float animatorDuration;
        public bool animatorDurationSet;
        public float animatorSampleTime;
        public bool animatorSampleTimeSet;
        public bool animatorLoop = true;
        public bool animatorLoopSet;
        public string terrainDataPath = string.Empty;
        public string terrainLayerPath = string.Empty;
        public string terrainTexturePath = string.Empty;
        public string terrainNormalPath = string.Empty;
        public string terrainTileSize = string.Empty;
        public string terrainTileOffset = string.Empty;
        public string terrainSize = string.Empty;
        public int terrainHeightmapResolution;
        public bool terrainHeightmapResolutionSet;
        public int terrainAlphamapResolution;
        public bool terrainAlphamapResolutionSet;
        public int terrainBaseMapResolution;
        public bool terrainBaseMapResolutionSet;
        public int terrainLayerIndex;
        public bool terrainLayerIndexSet;
        public bool terrainAppendLayer;
        public float terrainSmoothness;
        public bool terrainSmoothnessSet;
        public float terrainMetallic;
        public bool terrainMetallicSet;
        public string excelFilePath = string.Empty;
        public string excelSheet = string.Empty;
        public string excelTable = string.Empty;
        public string excelRoot = string.Empty;
        public int excelRow;
        public bool excelRowSet;
        public int excelIndex;
        public bool excelIndexSet;
        public string excelColumn = string.Empty;
        public string excelField = string.Empty;
        public string excelKey = string.Empty;
        public string excelKeyField = string.Empty;
        public string excelWhere = string.Empty;
        public string excelValuesJson = string.Empty;
        public int excelHeaderRow;
        public bool excelHeaderRowSet;
    }
}
