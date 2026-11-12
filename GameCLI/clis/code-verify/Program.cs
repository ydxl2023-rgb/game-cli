using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Syntax-only rules: no Unity assemblies are loaded and source files are never modified.
var input = JsonSerializer.Deserialize<Input>(Console.In.ReadToEnd(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new ArgumentException("Expected source input.");
var issues = new List<Issue>();
int lines = 0, blanks = 0, comments = 0, members = 0, documented = 0;
foreach (var file in input.Files)
{
    var tree = CSharpSyntaxTree.ParseText(file.Content, new CSharpParseOptions(documentationMode: DocumentationMode.Parse));
    var root = tree.GetRoot();
    var source = tree.GetText();
    int Line(SyntaxNode n) => source.Lines.GetLineFromPosition(n.SpanStart).LineNumber;
    void Report(SyntaxNode n, string rule, string message, string severity = "error") =>
        issues.Add(new Issue(file.Path, Line(n) + 1, rule, severity, message));
    bool BlankBefore(int index) => index > 0 && string.IsNullOrWhiteSpace(source.Lines[index - 1].ToString());
    void Name(SyntaxNode n, string value, bool upper)
    {
        if (!Regex.IsMatch(value, upper ? "^[A-Z][A-Za-z0-9]*$" : "^[a-z][A-Za-z0-9]*$"))
            Report(n, "OX001", upper ? "Use PascalCase without underscores." : "Use camelCase without prefixes or underscores.");
    }
    foreach (var diagnostic in tree.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error))
        issues.Add(new Issue(file.Path, diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1, "CS_PARSE", "error", diagnostic.GetMessage()));
    lines += source.Lines.Count;
    blanks += source.Lines.Count(l => string.IsNullOrWhiteSpace(l.ToString()));
    var commentLines = new HashSet<int>();
    foreach (var trivia in root.DescendantTrivia().Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) || t.IsKind(SyntaxKind.MultiLineCommentTrivia) || t.HasStructure && t.GetStructure() is DocumentationCommentTriviaSyntax))
    {
        int start = source.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;
        int end = source.Lines.GetLineFromPosition(Math.Max(trivia.SpanStart, trivia.Span.End - 1)).LineNumber;
        for (int i = start; i <= end; i++) commentLines.Add(i);
    }
    comments += commentLines.Count;
    foreach (var node in root.DescendantNodes())
    {
        if (node is BaseTypeDeclarationSyntax type) Name(type, type.Identifier.ValueText, true);
        if (node is MethodDeclarationSyntax method) Name(method, method.Identifier.ValueText, true);
        if (node is PropertyDeclarationSyntax property) Name(property, property.Identifier.ValueText, false);
        if (node is ParameterSyntax namedParameter) Name(namedParameter, namedParameter.Identifier.ValueText, false);
        if (node is VariableDeclaratorSyntax variable) Name(variable, variable.Identifier.ValueText, false);
        if (node is SingleVariableDesignationSyntax designation) Name(designation, designation.Identifier.ValueText, false);
        if (node is EnumMemberDeclarationSyntax enumMember) Name(enumMember, enumMember.Identifier.ValueText, true);
        if (node is ArrowExpressionClauseSyntax && node.Parent is not AccessorDeclarationSyntax)
            Report(node, "OX002", "Use a traditional method/property body, not an expression body.");
        if (node is ArrowExpressionClauseSyntax && node.Parent is AccessorDeclarationSyntax)
            Report(node, "OX002", "Use a traditional accessor body.");
        if (node is AttributeListSyntax attributes)
        {
            var names = attributes.Attributes.Select(a => a.Name.ToString().Split('.').Last().Replace("Attribute", "")).ToArray();
            if (names.Contains("SerializeField") && attributes.Target?.Identifier.ValueText == "field")
                Report(node, "OX003", "Use [SerializeField] on a private field, not [field: SerializeField].");
            if (names.Contains("SerializeField") && attributes.Parent is FieldDeclarationSyntax field && !field.Modifiers.Any(SyntaxKind.PrivateKeyword))
                Report(node, "OX003", "Serialized UI fields must explicitly be private.");
            if (names.Contains("Binding") && attributes.Parent is FieldDeclarationSyntax binding && Line(binding.Declaration) == Line(attributes))
                Report(node, "OX004", "Put the Binding field declaration on the next line.");
        }
        if (node is MemberDeclarationSyntax member && member is BaseTypeDeclarationSyntax or BaseMethodDeclarationSyntax or BasePropertyDeclarationSyntax or BaseFieldDeclarationSyntax or EnumMemberDeclarationSyntax)
        {
            members++;
            var docs = member.GetLeadingTrivia().Select(t => t.GetStructure()).OfType<DocumentationCommentTriviaSyntax>().ToArray();
            var xml = string.Join("\n", docs.Select(d => d.ToFullString()));
            if (docs.Length == 0 || !Regex.IsMatch(xml, @"<summary>\s*(?:(?!</summary>).)*[A-Za-z](?:(?!</summary>).)*</summary>", RegexOptions.Singleline))
                Report(member, "OX005", "Provide a nonempty English XML summary.");
            else documented++;
            if (Regex.IsMatch(xml, @"[\u4e00-\u9fff]")) Report(member, "OX005", "XML documentation must use English.");
            if (member is BaseMethodDeclarationSyntax callable)
            {
                foreach (var parameter in callable.ParameterList.Parameters)
                    if (!Regex.IsMatch(xml, "<param\\s+name=[\"']" + Regex.Escape(parameter.Identifier.ValueText) + "[\"']>\\s*[^<\\s]"))
                        Report(member, "OX006", "Document parameter: " + parameter.Identifier.ValueText);
                if (member is MethodDeclarationSyntax m && m.ReturnType.ToString() != "void" && !Regex.IsMatch(xml, @"<returns>\s*[^<\s]"))
                    Report(member, "OX006", "Document the return value.");
            }
        }
        if (node is TypeDeclarationSyntax declaration)
        {
            int previousRank = -1;
            foreach (var orderedMember in declaration.Members)
            {
                int rank = orderedMember switch {
                    FieldDeclarationSyntax or PropertyDeclarationSyntax or EventFieldDeclarationSyntax => 0,
                    ConstructorDeclarationSyntax => 1,
                    MethodDeclarationSyntax m when m.Modifiers.Any(SyntaxKind.OverrideKeyword) && new[]{"OnActive","OnDetive","Install","Uninstall","Startup","Shutdown","Update"}.Contains(m.Identifier.ValueText) => 2,
                    MethodDeclarationSyntax m when m.Modifiers.Any(SyntaxKind.PublicKeyword) => 3,
                    MethodDeclarationSyntax m when m.Modifiers.Any(SyntaxKind.ProtectedKeyword) || m.Modifiers.Any(SyntaxKind.InternalKeyword) => 4,
                    MethodDeclarationSyntax => 5,
                    _ => -1
                };
                if (rank >= 0) { if (rank < previousRank) Report(orderedMember,"OX007","Order: fields/properties, constructors, lifecycle overrides, public, protected/internal, private methods."); previousRank=Math.Max(previousRank,rank); }
                var doc = orderedMember.GetLeadingTrivia().FirstOrDefault(t => t.GetStructure() is DocumentationCommentTriviaSyntax);
                if (orderedMember != declaration.Members.First() && doc.RawKind != 0 && !BlankBefore(source.Lines.GetLineFromPosition(doc.SpanStart).LineNumber))
                    Report(orderedMember,"OX008","Add a blank line before the next member documentation.");
            }
            var bases = declaration.BaseList?.Types.Select(t=>t.Type.ToString()).ToArray() ?? Array.Empty<string>();
            if (bases.Any(b=>b=="VM" || b.EndsWith(".VM")) && !file.Path.Replace('\\','/').Contains("/VM/")) Report(declaration,"OX009","VM classes belong in the system's VM directory.");
            if (bases.Any(b=>b.Contains("UIWidget<")) && !file.Path.Replace('\\','/').Contains("/UI/")) Report(declaration,"OX009","UIWidget classes belong in the system's UI directory.");
        }
        if (node is ReturnStatementSyntax ret && ret.Parent is BlockSyntax block && block.Statements.IndexOf(ret)>0 && !BlankBefore(Line(ret)))
            Report(ret,"OX010","Separate processing from return with a blank line.");
        if (node is ParameterSyntax { Default: not null } param && Regex.IsMatch(param.Default.Value.ToString(),@"[\u4e00-\u9fff]"))
            Report(param,"OX011","Default parameters must not contain Chinese text.");
        if (node is InvocationExpressionSyntax call && call.Expression is MemberAccessExpressionSyntax access)
        {
            var methodName = access.Name.Identifier.ValueText;
            if (methodName=="OpenWidget" && (call.ArgumentList.Arguments.Count==0 || call.ArgumentList.Arguments.Any(a=>a.Expression.IsKind(SyntaxKind.NullLiteralExpression))))
                Report(call,"OX012","OpenWidget requires an explicit non-null completion callback.");
            if (methodName=="CloseWidget" && file.Path.EndsWith("System.cs"))
            {
                if (access.Name is GenericNameSyntax || call.ArgumentList.Arguments.FirstOrDefault()?.Expression is not IdentifierNameSyntax { Identifier.ValueText: "widgets" })
                    Report(call,"OX013","Declare a local System.Type[] widgets array, then pass widgets to CloseWidget.");
            }
            if (access.Expression.ToString()=="PlayerPrefs" && call.ArgumentList.Arguments.FirstOrDefault()?.Expression is LiteralExpressionSyntax)
                Report(call,"OX014","Generate persistence keys through StorageKey rather than string literals.");
            if (methodName=="Find" && access.Expression.ToString().Contains("transform",StringComparison.OrdinalIgnoreCase))
                Report(call,"OX015","Review runtime UI lookup; fixed UI references must be serialized.","review");
        }
        if (node is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression) && Regex.IsMatch(literal.Token.ValueText,@"[\u4e00-\u9fff]"))
            Report(literal,"OX016","Review text ownership: fixed UI text belongs in propCommonText; dynamic content/logs need classification.","review");
    }
    if (root.DescendantTrivia().Any(t=>t.IsKind(SyntaxKind.DisabledTextTrivia)))
        issues.Add(new Issue(file.Path,1,"OX017","review","Inactive preprocessor branches were not analyzed; inspect applicable Unity defines."));
}
Console.WriteLine(JsonSerializer.Serialize(new { policy="oathx-code/1", issues, statistics=new {files=input.Files.Length,lines,blankLines=blanks,commentLines=comments,members,documentedMembers=documented,blankLineRate=lines==0?0:(double)blanks/lines,commentLineRate=lines==0?0:(double)comments/lines,documentedMemberRate=members==0?0:(double)documented/members}},new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase}));
record Input(Source[] Files);
record Source(string Path,string Content);
record Issue(string File,int Line,string Rule,string Severity,string Message);
