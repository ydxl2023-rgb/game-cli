using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Deliberately syntactic: uncertain business meaning is reported for review, not inferred.
static class AdditionalRules
{
    public static void Analyze(SyntaxNode root, Action<SyntaxNode, string, string> report)
    {
        foreach (var node in root.DescendantNodes())
        {
            if (node is InvocationExpressionSyntax call)
            {
                string name = call.Expression switch {
                    MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText,
                    MemberBindingExpressionSyntax member => member.Name.Identifier.ValueText,
                    SimpleNameSyntax simple => simple.Identifier.ValueText,
                    _ => ""
                };
                if (name == "DispatchEvent")
                    report(call, "OX024", "Send application events through XPluginUtility.FireEvent or PostEvent.");
                if (name is "SetAnimation" or "FindAnimation" &&
                    call.ArgumentList.Arguments.Any(a => a.Expression.IsKind(SyntaxKind.StringLiteralExpression)))
                    report(call, "OX028", "Review animation literals; use the animation constants declared before the shape class.");
            }
            if (node is BinaryExpressionSyntax comparison &&
                (comparison.IsKind(SyntaxKind.EqualsExpression) || comparison.IsKind(SyntaxKind.NotEqualsExpression)))
            {
                bool Route(ExpressionSyntax expression) => expression switch {
                    IdentifierNameSyntax id => id.Identifier.ValueText == "route",
                    MemberAccessExpressionSyntax member => member.Name.Identifier.ValueText == "route",
                    _ => false
                };
                if (Route(comparison.Left) && comparison.Right.IsKind(SyntaxKind.StringLiteralExpression) ||
                    Route(comparison.Right) && comparison.Left.IsKind(SyntaxKind.StringLiteralExpression))
                    report(comparison, "OX025", "Review business route comparison; fixed destinations use an enum, not text.");
            }
            if (node is VariableDeclarationSyntax variables && variables.Variables.Count > 1)
                report(variables, "OX029", "Declare each field or local variable separately.");
            if (node is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression) &&
                literal.Token.ValueText.StartsWith("Assets/", StringComparison.Ordinal) &&
                literal.Token.ValueText.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                report(literal, "OX027", "Resolve business scene paths through the scene configuration table.");
            if (node is not ClassDeclarationSyntax type ||
                type.BaseList?.Types.Any(b => b.Type.ToString().Split('.').Last() == "RuntimeSystem") != true)
                continue;
            foreach (var method in type.Members.OfType<MethodDeclarationSyntax>())
            {
                string name = method.Identifier.ValueText;
                if (name is "Install" or "Uninstall" && !method.Modifiers.Any(SyntaxKind.OverrideKeyword))
                    report(method, "OX026", "Use paired business-specific names such as InstallHome/UninstallHome; do not hide lifecycle methods.");
                if (name == "OnActive" && method.DescendantNodes().OfType<InvocationExpressionSyntax>()
                    .Any(c => c.Expression is MemberAccessExpressionSyntax m && m.Name.Identifier.ValueText == "Run" &&
                        m.Expression.ToString().Split('.').Last() == "XCoroutine"))
                    report(method, "OX026", "Move initialization into Install<System>(); OnActive subscribes then returns its bool result.");
            }
            foreach (var field in type.Members.OfType<FieldDeclarationSyntax>())
                foreach (var variable in field.Declaration.Variables)
                    if (variable.Identifier.ValueText is "generation" or "requestedActive")
                        report(variable, "OX030", "Review duplicated lifecycle state; prefer framework ownership and coroutine cancellation.");
        }
    }
}
