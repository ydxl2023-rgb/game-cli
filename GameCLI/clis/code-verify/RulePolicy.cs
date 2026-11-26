using System.Security.Cryptography;
using System.Text.Json;

// The shipped policy is mandatory: invalid or missing rules must never silently pass.
sealed class RulePolicy
{
    public string Version { get; set; } = "";
    public RuleDefinition[] Rules { get; set; } = [];
    public InitializerSpacing InitializerSpacing { get; set; } = new();
    public int MaxCodeLineLength { get; set; }
    public string Sha256 { get; private set; } = "";

    public static RulePolicy Load()
    {
        var bytes = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "rules.json"));
        var policy = JsonSerializer.Deserialize<RulePolicy>(bytes, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidDataException("Missing code rule policy.");
        var required = Enumerable.Range(1, 30).Select(i => $"OX{i:000}").Append("CS_PARSE").ToHashSet();
        if (string.IsNullOrWhiteSpace(policy.Version) || policy.Rules == null ||
            policy.Rules.Any(r => r == null || !required.Contains(r.Id) || r.Severity is not ("error" or "review") || string.IsNullOrWhiteSpace(r.Description)) ||
            policy.Rules.Length != required.Count || policy.Rules.Select(r => r.Id).Distinct().Count() != required.Count ||
            policy.Rules.Single(r => r.Id == "CS_PARSE").Severity != "error" ||
            policy.InitializerSpacing == null || !policy.InitializerSpacing.Before || !policy.InitializerSpacing.After ||
            policy.MaxCodeLineLength < 80 || policy.MaxCodeLineLength > 200)
            throw new InvalidDataException("Invalid or incomplete code rule policy; required spacing checks cannot be disabled.");
        policy.Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        return policy;
    }

    public Issue Apply(Issue issue)
    {
        return issue with { Severity = Rules.Single(r => r.Id == issue.Rule).Severity };
    }
}

sealed class RuleDefinition
{
    public string Id { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Description { get; set; } = "";
}

sealed class InitializerSpacing
{
    public bool Before { get; set; }
    public bool After { get; set; }
}
