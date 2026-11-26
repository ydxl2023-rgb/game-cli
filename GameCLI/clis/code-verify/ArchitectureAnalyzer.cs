using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Analyze source symbols without loading or executing Unity assemblies.
static class ArchitectureAnalyzer
{
    public static object Analyze(Source[] files)
    {
        var trees = files.Select(f => CSharpSyntaxTree.ParseText(f.Content, path: f.Path)).ToArray();
        var compilation = CSharpCompilation.Create("ArchitectureInspection", trees,
            new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var issues = new List<Issue>();
        var systems = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var seen = new HashSet<string>();
        bool IsSystem(INamedTypeSymbol type)
        {
            var visited = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
            for (var parent = type.BaseType; parent != null && visited.Add(parent); parent = parent.BaseType)
                if (parent.Name is "RuntimeSystem" or "XEventModule") return true;
            return false;
        }
        foreach (var tree in trees)
        {
            var model = compilation.GetSemanticModel(tree);
            foreach (var declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                if (model.GetDeclaredSymbol(declaration) is INamedTypeSymbol type && !type.IsAbstract && IsSystem(type))
                    systems.Add(type.OriginalDefinition);
        }
        string? Owner(string path)
        {
            var parts = path.Replace('\\', '/').Split('/');
            int index = Array.IndexOf(parts, "Systems");
            return index >= 0 && index + 2 < parts.Length ? parts[index + 1] : null;
        }
        IEnumerable<INamedTypeSymbol> Types(ITypeSymbol? type)
        {
            if (type is IArrayTypeSymbol array)
                foreach (var item in Types(array.ElementType)) yield return item;
            if (type is INamedTypeSymbol named)
            {
                yield return named.OriginalDefinition;
                foreach (var argument in named.TypeArguments)
                    foreach (var item in Types(argument)) yield return item;
            }
        }
        foreach (var tree in trees)
        {
            string? owner = Owner(tree.FilePath);
            if (owner == null) continue; // Composition infrastructure outside Systems owns registration.
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();
            void Report(SyntaxNode node, string rule, string message)
            {
                int line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                if (seen.Add($"{tree.FilePath}:{line}:{rule}:{message}"))
                    issues.Add(new Issue(tree.FilePath, line, rule, "error", message));
            }
            foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
                issues.Add(new Issue(tree.FilePath, diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1, "CS_PARSE", "error", diagnostic.GetMessage()));
            foreach (var node in root.DescendantNodes().OfType<ExpressionSyntax>())
            {
                var info = model.GetSymbolInfo(node);
                var symbol = info.Symbol;
                if (symbol is IAliasSymbol alias) symbol = alias.Target;
                var type = symbol switch {
                    INamedTypeSymbol named => named,
                    IMethodSymbol method => method.ReturnType,
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    ILocalSymbol local => local.Type,
                    IParameterSymbol parameter => parameter.Type,
                    _ => model.GetTypeInfo(node).Type
                };
                var referenced = Types(type).Concat(symbol?.ContainingType is {} container ? Types(container) : Array.Empty<INamedTypeSymbol>());
                foreach (var target in referenced.Where(systems.Contains).Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default))
                {
                    var targetOwners = target.DeclaringSyntaxReferences.Select(r => Owner(r.SyntaxTree.FilePath)).Distinct().ToArray();
                    if (targetOwners.Length == 1 && targetOwners[0] == owner) continue;
                    Report(node, "OXA001", $"System module '{owner}' must not reference business system '{target.ToDisplayString()}'; use events and composition-layer lifecycle management.");
                }
                if (node is IdentifierNameSyntax name && info.Symbol == null && systems.Any(t => t.Name == name.Identifier.ValueText))
                    Report(node, "OXA_UNRESOLVED", "A known business-system name could not be resolved; dependency coverage is incomplete.");
                if (node is InvocationExpressionSyntax call && call.Expression.ToString() is "Type.GetType" or "System.Type.GetType" or "Activator.CreateInstance")
                    Report(node, "OXA_DYNAMIC", "Dynamic type lookup requires review; static system isolation cannot be established.");
            }
            if (root.DescendantTrivia().Any(t => t.IsKind(SyntaxKind.DisabledTextTrivia)))
                Report(root, "OXA_CONDITIONAL", "Inactive conditional code requires checking with the target build symbols.");
        }
        return new { policy = "oathx-architecture/1", issues, statistics = new { files = files.Length, systems = systems.Count } };
    }
}
