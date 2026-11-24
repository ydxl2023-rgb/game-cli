using System.Text.Json;

namespace UnityCLI;

internal sealed class ParticleCommand : ICommand
{
    public string Name => "particle";

    public async Task<int> ExecuteAsync(CliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Action))
        {
            Console.Error.WriteLine("particle requires an action: create | remove-instance.");
            return 1;
        }

        if (!string.Equals(options.Action, "create", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(options.Action, "remove-instance", StringComparison.OrdinalIgnoreCase))
        {
            Console.Error.WriteLine("Unsupported particle action: " + options.Action);
            return 1;
        }

        if (string.Equals(options.Action, "create", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(options.ParticleName))
        {
            Console.Error.WriteLine("particle create requires a name.");
            return 1;
        }

        var responseJson = await CLIClient.SendAsync(options.ProjectPath, "/particle", new
        {
            action = options.Action,
            particleName = options.ParticleName,
            objectPath = options.ObjectPath,
            prefabPath = options.PrefabPath,
            assetSrcPath = options.ParticleTexturePath,
            particleMaterialPath = options.ParticleMaterialPath,
            particleTexturePath = options.ParticleTexturePath,
            particleShape = options.ParticleShape,
            particleStartColor = options.ParticleStartColor,
            particleDuration = options.ParticleDuration,
            particleDurationSet = options.ParticleDurationSet,
            particleLoop = options.ParticleLoop,
            particleLoopSet = options.ParticleLoopSet,
            particleStartLifetime = options.ParticleStartLifetime,
            particleStartLifetimeSet = options.ParticleStartLifetimeSet,
            particleStartSpeed = options.ParticleStartSpeed,
            particleStartSpeedSet = options.ParticleStartSpeedSet,
            particleStartSize = options.ParticleStartSize,
            particleStartSizeSet = options.ParticleStartSizeSet,
            particleEmissionRate = options.ParticleEmissionRate,
            particleEmissionRateSet = options.ParticleEmissionRateSet,
            particleMaxParticles = options.ParticleMaxParticles,
            particleMaxParticlesSet = options.ParticleMaxParticlesSet,
            particlePosition = options.ParticlePosition,
            particleShapeScale = options.ParticleShapeScale,
            particleShapeRadius = options.ParticleShapeRadius,
            particleShapeRadiusSet = options.ParticleShapeRadiusSet,
            particleShapeAngle = options.ParticleShapeAngle,
            particleShapeAngleSet = options.ParticleShapeAngleSet,
            particleVelocityX = options.ParticleVelocityX,
            particleVelocityY = options.ParticleVelocityY,
            particleVelocityZ = options.ParticleVelocityZ,
            particleNoiseStrength = options.ParticleNoiseStrength,
            particleNoiseStrengthSet = options.ParticleNoiseStrengthSet,
            particleNoiseFrequency = options.ParticleNoiseFrequency,
            particleNoiseFrequencySet = options.ParticleNoiseFrequencySet,
            particleNoiseScrollSpeed = options.ParticleNoiseScrollSpeed,
            particleNoiseScrollSpeedSet = options.ParticleNoiseScrollSpeedSet,
            particleRotationZ = options.ParticleRotationZ,
            particleSizeOverLifetime = options.ParticleSizeOverLifetime,
            particleFadeOut = options.ParticleFadeOut,
            particleFadeStart = options.ParticleFadeStart,
            particleFadeStartSet = options.ParticleFadeStartSet,
            particleSortingOrder = options.ParticleSortingOrder,
            particleSortingOrderSet = options.ParticleSortingOrderSet,
            dryRun = options.DryRun,
            replaceSceneInstance = options.ReplaceSceneInstance
        }, "POST");

        return ResponseFormatter.WriteJsonOrHuman(responseJson, options.JsonOutput, WriteHuman);
    }

    static void WriteHuman(JsonElement root)
    {
        var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
        if (!ok)
        {
            ResponseFormatter.WriteError(root);
            return;
        }

        Console.WriteLine(root.TryGetProperty("message", out var message) ? message.GetString() : "particle command completed");
        if (!root.TryGetProperty("data", out var data)) return;
        WriteString(data, "action");
        WriteString(data, "particleName");
        WriteString(data, "prefabPath");
        WriteString(data, "materialPath");
        WriteString(data, "texturePath");
        WriteString(data, "shape");
        WriteFloat(data, "duration");
        WriteBool(data, "loop");
        WriteFloat(data, "startLifetime");
        WriteFloat(data, "startSpeed");
        WriteFloat(data, "startSize");
        WriteFloat(data, "emissionRate");
        WriteInt(data, "maxParticles");
        WriteBool(data, "dryRun");
        WriteBool(data, "sceneInstanceReplaced");
    }

    static void WriteString(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            if (!string.IsNullOrEmpty(text)) Console.WriteLine("  " + name + ": " + text);
        }
    }

    static void WriteFloat(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.TryGetDouble(out var number))
        {
            Console.WriteLine("  " + name + ": " + number);
        }
    }

    static void WriteInt(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value) && value.TryGetInt32(out var number))
        {
            Console.WriteLine("  " + name + ": " + number);
        }
    }

    static void WriteBool(JsonElement data, string name)
    {
        if (data.TryGetProperty(name, out var value)
            && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
        {
            Console.WriteLine("  " + name + ": " + value.GetBoolean());
        }
    }
}
