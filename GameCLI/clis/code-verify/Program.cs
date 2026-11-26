using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// Syntax-only rules: no Unity assemblies are loaded and source files are never modified.
Console.InputEncoding = new System.Text.UTF8Encoding(false, true);
Console.OutputEncoding = new System.Text.UTF8Encoding(false);
var input = JsonSerializer.Deserialize<Input>(Console.In.ReadToEnd(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
    ?? throw new ArgumentException("Expected source input.");
if (input.Mode == "architecture")
{
    Console.WriteLine(JsonSerializer.Serialize(ArchitectureAnalyzer.Analyze(input.Files), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    return;
}
var policy = RulePolicy.Load();
var issues = new List<Issue>();
int lines = 0, blanks = 0, comments = 0, members = 0, documented = 0;
foreach (var file in input.Files)
{
    var tree = CSharpSyntaxTree.ParseText(file.Content, new CSharpParseOptions(documentationMode: DocumentationMode.Parse));
    var root = tree.GetRoot();
    var source = tree.GetText();
    var codeEnds = new Dictionary<int, int>();
    foreach (var token in root.DescendantTokens())
    {
        if (token.Span.IsEmpty)
            continue;
        int first = source.Lines.GetLineFromPosition(token.SpanStart).LineNumber;
        int last = source.Lines.GetLineFromPosition(token.Span.End - 1).LineNumber;
        // Multiline literal bodies are text data, not code that can be safely wrapped.
        if (first != last)
        {
            codeEnds[first] = Math.Max(codeEnds.GetValueOrDefault(first), token.SpanStart + 1);
            codeEnds[last] = Math.Max(codeEnds.GetValueOrDefault(last), token.Span.End);
        }
        else
            codeEnds[first] = Math.Max(codeEnds.GetValueOrDefault(first), token.Span.End);
    }
    foreach (var (line, end) in codeEnds.OrderBy(pair => pair.Key))
    {
        int columns = 0;
        foreach (char character in source.ToString(Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(source.Lines[line].Start, end)))
            columns += character == '\t' ? 4 - columns % 4 : 1;
        if (columns > policy.MaxCodeLineLength)
            issues.Add(new Issue(file.Path, line + 1, "OX019", "error",
                $"Code line has {columns} columns; maximum is {policy.MaxCodeLineLength}. Wrap conditions, arguments or call chains."));
    }
    int Line(SyntaxNode n) => source.Lines.GetLineFromPosition(n.SpanStart).LineNumber;
    void Report(SyntaxNode n, string rule, string message, string severity = "error") =>
        issues.Add(new Issue(file.Path, Line(n) + 1, rule, severity, message));
    bool BlankBefore(int index) => index > 0 && string.IsNullOrWhiteSpace(source.Lines[index - 1].ToString());
    AdditionalRules.Analyze(root, (node, rule, message) => Report(node, rule, message));
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
        if (node is BlockSyntax statements)
        {
            for (int i = 0; i < statements.Statements.Count; i++)
            {
                var statement = statements.Statements[i];
                if (statement is not LocalDeclarationStatementSyntax local ||
                    !local.Declaration.Variables.Any(v => (v.Initializer?.Value switch {
                        BaseObjectCreationExpressionSyntax creation => creation.Initializer,
                        ArrayCreationExpressionSyntax array => array.Initializer,
                        ImplicitArrayCreationExpressionSyntax array => array.Initializer,
                        InitializerExpressionSyntax initializer => initializer,
                        _ => null
                    }) is { } init && source.Lines.GetLineFromPosition(init.SpanStart).LineNumber < source.Lines.GetLineFromPosition(init.Span.End - 1).LineNumber))
                    continue;
                bool HasBlankBetween(StatementSyntax first, StatementSyntax second)
                {
                    int start = source.Lines.GetLineFromPosition(first.Span.End - 1).LineNumber + 1;
                    int end = Line(second);
                    return Enumerable.Range(start, Math.Max(0, end - start)).Any(line => string.IsNullOrWhiteSpace(source.Lines[line].ToString()));
                }
                if (policy.InitializerSpacing.Before && i > 0 && !HasBlankBetween(statements.Statements[i - 1], statement))
                    Report(statement, "OX018", "Separate a multiline initialized local from the preceding operation with a blank line.");
                if (policy.InitializerSpacing.After && i + 1 < statements.Statements.Count && !HasBlankBetween(statement, statements.Statements[i + 1]))
                    Report(statements.Statements[i + 1], "OX018", "Separate a multiline initialized local from the following operation with a blank line.");
            }
        }
        if (node is IfStatementSyntax { Else: null } guard)
        {
            IEnumerable<ExpressionSyntax> Alternatives(ExpressionSyntax condition)
            {
                if (condition is ParenthesizedExpressionSyntax parentheses)
                    return Alternatives(parentheses.Expression);
                if (condition is BinaryExpressionSyntax either && either.IsKind(SyntaxKind.LogicalOrExpression))
                    return Alternatives(either.Left).Concat(Alternatives(either.Right));
                return new[] { condition };
            }
            var parameters = Alternatives(guard.Condition).Select(condition => condition switch
            {
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression)
                    && binary.Right.IsKind(SyntaxKind.NullLiteralExpression) => binary.Left as IdentifierNameSyntax,
                BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.EqualsExpression)
                    && binary.Left.IsKind(SyntaxKind.NullLiteralExpression) => binary.Right as IdentifierNameSyntax,
                IsPatternExpressionSyntax { Expression: IdentifierNameSyntax id, Pattern: ConstantPatternSyntax pattern }
                    when pattern.Expression.IsKind(SyntaxKind.NullLiteralExpression) => id,
                _ => null
            }).OfType<IdentifierNameSyntax>();
            var body = guard.Statement is BlockSyntax guardBlock && guardBlock.Statements.Count == 1
                ? guardBlock.Statements[0] : guard.Statement;
            bool exits = body is ReturnStatementSyntax
                || (body is ThrowStatementSyntax { Expression: ObjectCreationExpressionSyntax error }
                    && error.Type.ToString() is "ArgumentNullException" or "System.ArgumentNullException" or "global::System.ArgumentNullException");
            var signature = guard.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault()?.ParameterList;
            if (exits && signature != null && parameters.Any(id =>
                signature.Parameters.Any(p => p.Identifier.ValueText == id.Identifier.ValueText)))
                Report(guard, "OX020", "Verify null has a defined outcome here; remove guards that mask an invalid required parameter.", "review");
        }
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
                    MethodDeclarationSyntax m when m.Modifiers.Any(SyntaxKind.OverrideKeyword) && new[]{"OnActive","OnDetive","Install","Uninstall","Startup","Shutdown","Update","Awake","Start","OnEnable","OnDisable","OnDestroy","OnValidate","OnCanvasHierarchyChanged","OnTransformParentChanged","OnRectTransformDimensionsChange","Reset"}.Contains(m.Identifier.ValueText) => 2,
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
            if (bases.Any(b => b.Split('.').Last() == "RuntimeSystem"))
            {
                var methods = declaration.Members.OfType<MethodDeclarationSyntax>();
                var activate = methods.FirstOrDefault(m => m.Identifier.ValueText == "OnActive")?.Body;
                var deactivate = methods.FirstOrDefault(m => m.Identifier.ValueText == "OnDetive")?.Body;
                bool Sets(BlockSyntax? body, string name, SyntaxKind value) => body?.Statements
                    .OfType<ExpressionStatementSyntax>().Select(s => s.Expression).OfType<AssignmentExpressionSyntax>()
                    .Any(a => a.Left.ToString().Replace("this.", "") == name && a.Right.IsKind(value)) == true;
                foreach (var check in activate?.Statements.OfType<IfStatementSyntax>() ?? Enumerable.Empty<IfStatementSyntax>())
                {
                    var action = check.Statement is BlockSyntax group && group.Statements.Count == 1
                        ? group.Statements[0] : check.Statement;
                    if (action is not ReturnStatementSyntax || check.Else != null) continue;
                    var condition = check.Condition is PrefixUnaryExpressionSyntax prefix && prefix.IsKind(SyntaxKind.LogicalNotExpression)
                        ? prefix.Operand : check.Condition;
                    string name = condition.ToString().Replace("this.", "");
                    if (name == "IsActive()" || (Sets(activate, name, SyntaxKind.TrueLiteralExpression)
                        && Sets(deactivate, name, SyntaxKind.FalseLiteralExpression)))
                        Report(check, "OX021", "Leave duplicate activation handling to RuntimeSystem.Active().");
                }
            }
            if (bases.Any(b=>b=="VM" || b.EndsWith(".VM")) && !file.Path.Replace('\\','/').Contains("/VM/")) Report(declaration,"OX009","VM classes belong in the system's VM directory.");
            if (bases.Any(b=>b.Contains("UIWidget<")) && !file.Path.Replace('\\','/').Contains("/UI/")) Report(declaration,"OX009","UIWidget classes belong in the system's UI directory.");
        }
        if (node is ReturnStatementSyntax ret && ret.Parent is BlockSyntax block && block.Statements.IndexOf(ret)>0 && !BlankBefore(Line(ret)))
            Report(ret,"OX010","Separate processing from return with a blank line.");
        if (node is ParameterSyntax { Default: not null } param && Regex.IsMatch(param.Default.Value.ToString(),@"[\u4e00-\u9fff]"))
            Report(param,"OX011","Default parameters must not contain Chinese text.");
        if (node is InvocationExpressionSyntax subscription && subscription.ArgumentList.Arguments.Count == 1)
        {
            var called = subscription.Expression switch
            {
                SimpleNameSyntax simple => simple,
                MemberAccessExpressionSyntax qualified => qualified.Name,
                MemberBindingExpressionSyntax conditional => conditional.Name,
                _ => null
            };
            if (called?.Identifier.ValueText == "Subscribe")
            {
                var handler = subscription.ArgumentList.Arguments[0].Expression;
                var handlerName = handler switch
                {
                    SimpleNameSyntax simple => simple.Identifier.ValueText,
                    MemberAccessExpressionSyntax qualified => qualified.Name.Identifier.ValueText,
                    _ => ""
                };
                if (!Regex.IsMatch(handlerName, @"^On[A-Z][A-Za-z0-9]*$"))
                    Report(handler, "OX022", "Subscribe with a named On... handler method group.");
            }
        }
        if (node is InvocationExpressionSyntax call && call.Expression is MemberAccessExpressionSyntax access)
        {
            var methodName = access.Name.Identifier.ValueText;
            if (access.Name is GenericNameSyntax && methodName == "QueryEntity")
            {
                SyntaxNode result = call;
                while (result.Parent is ParenthesizedExpressionSyntax)
                    result = result.Parent;
                if (result.Parent is MemberAccessExpressionSyntax or ConditionalAccessExpressionSyntax or ArgumentSyntax)
                    Report(call, "OX023", "Assign the queried entity to a local before using it.");
            }
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
Console.WriteLine(JsonSerializer.Serialize(new { policy=policy.Version, policySha256=policy.Sha256, rules=policy.Rules, issues=issues.Select(policy.Apply), statistics=new {files=input.Files.Length,lines,blankLines=blanks,commentLines=comments,members,documentedMembers=documented,blankLineRate=lines==0?0:(double)blanks/lines,commentLineRate=lines==0?0:(double)comments/lines,documentedMemberRate=members==0?0:(double)documented/members}},new JsonSerializerOptions {PropertyNamingPolicy=JsonNamingPolicy.CamelCase}));
record Input(Source[] Files, string? Mode = null);
record Source(string Path,string Content);
record Issue(string File,int Line,string Rule,string Severity,string Message);
