using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

enum BindingType
{
    Binding,
    BindingAttribute,
    Notify,
    NotifyAttribute,
    RpcClientCallServer,
    RpcClientCallServerAttribute,
    RpcInterface,
    RpcInterfaceAttribute,
    ConstFieldDef
}

enum FileSpecial
{
    Auto
}

//RoslynAnalyzer
[Generator]
public class FileOutputBindingGenerator : ISourceGenerator
{
    /// <summary>
    /// RPC types for code generation, used in the [RpcClientCallServer] attribute to specify the RPC type for method calls. This can be extended with more types as needed.
    /// </summary>
    public enum XRpcType
    {
        Local,
        Tcp,
        Kcp,
        Web,
        Http,
        All
    }

    /// <summary>
    /// Constant property names for notify properties, used for change notification and other purposes.
    /// </summary>
    public Dictionary<string, int> ConstNames { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new BindingSyntaxReceiver());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    public void Execute(GeneratorExecutionContext context)
    {  
        if (context.SyntaxReceiver is BindingSyntaxReceiver receiver)
        {
            var directory = Path.Combine(Path.GetTempPath(), typeof(FileOutputBindingGenerator).Name);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(DateTime.Now.ToString());

            try
            {
                foreach (var classDeclaration in receiver.CandidateClasses)
                {
                    var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                    if (model.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
                    {
                        sb.AppendLine(classSymbol.Name);

                        var sourceCode = GeneratePartialClass(classSymbol);
                        if (!string.IsNullOrEmpty(sourceCode))
                        {
                            sb.AppendLine(sourceCode);

                            var filename = MakFileName(classSymbol);
                            context.AddSource(filename,
                                SourceText.From(sourceCode, Encoding.UTF8));
                        }
                    }
                }

                foreach (var classDeclaration in receiver.NotifyClasses)
                {
                    var model = context.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);
                    if (model.GetDeclaredSymbol(classDeclaration) is INamedTypeSymbol classSymbol)
                    {
                        sb.AppendLine(classSymbol.Name);

                        var sourceCode = GenerateNotifyPartialClass(classSymbol);
                        if (!string.IsNullOrEmpty(sourceCode))
                        {
                            sb.AppendLine(sourceCode);

                            var filename = MakFileName(classSymbol);
                            context.AddSource(filename,
                                SourceText.From(sourceCode, Encoding.UTF8));
                        }
                    }
                }

                foreach (var interfaceDeclaration in receiver.RpcInterfaces)
                {
                    var model = context.Compilation.GetSemanticModel(interfaceDeclaration.SyntaxTree);
                    if (model.GetDeclaredSymbol(interfaceDeclaration) is INamedTypeSymbol interfaceSymbol)
                    {
                        if (interfaceSymbol != null)
                        {
                            sb.AppendLine(interfaceSymbol.Name);

                            var sourceCode = GenerateRpcPartialClass(interfaceSymbol);
                            if (!string.IsNullOrEmpty(sourceCode))
                            {
                                sb.AppendLine(sourceCode);

                                var filename = GetInterfaceFileName(interfaceSymbol);
                                context.AddSource(filename,
                                    SourceText.From(sourceCode, Encoding.UTF8));
                            }
                        }
                    }
                }

            }
            catch (Exception exception)
            {
                sb.AppendLine(exception.Message);
                sb.AppendLine(exception.StackTrace);
            }

            // For debugging purposes, you can output the log to a file or use other means.
            File.AppendAllText(Path.Combine(directory, typeof(FileOutputBindingGenerator).Name), sb.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="namedTypeSymbol"></param>
    /// <returns></returns>
    private string MakFileName(INamedTypeSymbol namedTypeSymbol)
    {
        return $"{namedTypeSymbol.Name}.{FileSpecial.Auto}.cs";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfaceSymbol"></param>
    /// <returns></returns>
    private string GetInterfaceFileName(INamedTypeSymbol interfaceSymbol)
    {
        return $"{GetClassNameFromInterface(interfaceSymbol.Name)}.{FileSpecial.Auto}.cs";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <returns></returns>
    private string GeneratePartialClass(INamedTypeSymbol classSymbol)
    {
        var propertyCode = GenerateProperties(classSymbol, out int propertyCount);
        var classGenCode = $@"
using System;
using System.Collections.Generic;

/// <summary>
/// {classSymbol.Name}
/// </summary>
public partial class {classSymbol.Name}
{{
    /// <summary>
    /// 记录各绑定属性是否已交付过至少一次同步：首次赋值即使等于字段默认值也必须下发到组件，
    /// 否则 prefab 默认状态与 VM 初始值不一致的节点（如默认激活的弹窗卡片）永远得不到矫正。
    /// </summary>
    private readonly HashSet<int> __assignedOnce = new HashSet<int>();

    /// <summary>
    /// {classSymbol.Name} construct
    /// </summary>
    public {classSymbol.Name}()
    {{
        Init({propertyCount});
    }}
    
    {propertyCode}
}}";

        return classGenCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <returns></returns>
    private string GenerateRpcPartialClass(INamedTypeSymbol interfaceSymbol)
    {
        StringBuilder md = new StringBuilder();
        StringBuilder rp = new StringBuilder();

        var methods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Ordinary);

        foreach (var method in methods)
        {
            if (TryGetMethodAttribute(method, out AttributeData attr))
            {
                if (attr.ConstructorArguments.Length >= 2)
                {
                    var flag = attr.ConstructorArguments[0];
                    var hash = attr.ConstructorArguments[1];                    
                    
                    if (flag.Kind == TypedConstantKind.Enum)
                    {
                        if (GenerateMethodBody(method, (XRpcType)flag.Value, (int)hash.Value, 
                            out string defMethodCode, out string rpcMethodCode))
                        {
                            md.AppendLine(defMethodCode);
                            rp.AppendLine(rpcMethodCode);
                        }   
                    }
                }
            }
        }

        string className = GetClassNameFromInterface(interfaceSymbol.Name);
        return $@"

using System;
using System.Collections.Generic;
using Oathx.Rpc;

public class {className} : {interfaceSymbol.Name}
{{
    {md}
}}

public partial class Rpc
{{
    static readonly {className} rpc = new {className}();

    {rp}
}}
";
    }

    /// <summary>
    /// Try to get the RPC method attribute from the given method symbol. 
    /// This method checks if the method has an attribute that matches either [RpcClientCallServer] or [RpcClientCallServerAttribute], 
    /// and if found, it returns the attribute data for further processing in code generation.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="attr"></param>
    /// <returns></returns>
    private bool TryGetMethodAttribute(IMethodSymbol method, out AttributeData attr)
    {
        attr = method.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == BindingType.RpcClientCallServer.ToString() ||
                   a.AttributeClass?.ToDisplayString() == BindingType.RpcClientCallServerAttribute.ToString());

        return attr != null;
    }

    /// <summary>
    /// Generate the method body for the RPC call based on the method symbol and the specified RPC type.
    /// This method constructs the appropriate method signature and body for both the interface implementation and the static RPC call method, depending on the RPC type specified in the attribute.
    /// It also handles hashing of method names for certain RPC types if a hash value is provided.
    /// </summary>
    /// <param name="method"></param>
    /// <param name="flag"></param>
    /// <param name="type"></param>
    /// <param name="hash"></param>
    /// <param name="defMethods"></param>
    /// <param name="rpcMehtods"></param>
    /// <returns></returns>
    private bool GenerateMethodBody(IMethodSymbol method, XRpcType flag, int hash, out string defMethods, out string rpcMehtods)
    {
        var parameters = method.Parameters;

        StringBuilder pl = new StringBuilder();
        StringBuilder al = new StringBuilder();

        for (int i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];

            string paramName = param.Name;
            al.Append(paramName);

            string paramType = param.Type.ToDisplayString();
            pl.Append($"{paramType} {paramName}");

            if (i < parameters.Length - 1)
            {
                pl.Append(", ");
                al.Append(", ");
            }
        }

        // Generate the attribute string for the method, which includes the RPC type, hash, and type information.
        // This attribute is used to annotate both the interface method and the static RPC call method, providing metadata for code generation and runtime behavior.
        string attrString = $@"RpcClientCallServer(XRpcType.{flag}, {hash})";

        // Get the RPC call method name based on the specified RPC type flag, which determines how the RPC call will be made to the server.
        // This is used in the generated code to ensure that the correct method is called for the specified RPC type.
        string callMethod = GetRpcCallMethod(flag);
        rpcMehtods = $@"
    [{attrString}]
    public static void {method.Name}({pl.ToString()})
    {{
        rpc.{method.Name}({al});
    }}";

        string body = $@"{callMethod}(""{method.Name}"", {al});";
        if (flag != XRpcType.Http)
        {
            var hashCode = HashString(method.Name);
            if (hash != 0)
                hashCode = hash;

            body = $@"{callMethod}({hashCode}, {al});";
        }

        defMethods = $@"
    [{attrString}]
    public void {method.Name}({pl.ToString()})
    {{
        {body}
    }}";

        return true;
    }

    /// <summary>
    /// Calculate the FNV-1a hash of the given string,
    /// which is commonly used for hashing method names in RPC systems to generate a unique identifier for each method.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private int HashString(string str)
    {
        const uint InitialFNV = 2166136261U;
        const uint FNVMultiple = 16777619;

        uint hash = InitialFNV;
        for (int i = 0; i < str.Length; i++)
        {
            hash ^= str[i];
            hash *= FNVMultiple;
        }

        return (int)(hash & 0x7FFFFFFF);
    }

    /// <summary>
    /// Get the RPC call method name based on the specified RPC type flag. 
    /// This method maps the XRpcType enum values to their corresponding RPC call method names,
    /// which are used in the generated code to make RPC calls to the server.
    /// </summary>
    /// <param name="flag"></param>
    /// <returns></returns>
    private string GetRpcCallMethod(XRpcType flag)
    {
        switch (flag)
        {
            case XRpcType.Http:
                return "XNetRpc.CallHttpServer";
            case XRpcType.Tcp:
                return "XNetRpc.CallTcpHash";
            case XRpcType.Web:
                return "XNetRpc.CallWebHash";
            case XRpcType.Kcp:
                return "XNetRpc.CallKcpHash";

            default:
                return "";
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfaceName"></param>
    /// <returns></returns>
    private string GetClassNameFromInterface(string interfaceName)
    {
        if (interfaceName.StartsWith("I") && interfaceName.Length > 1)
            return interfaceName.Substring(1);

        return interfaceName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <returns></returns>
    private string GenerateNotifyPartialClass(INamedTypeSymbol classSymbol)
    {
        var propertyCode = GenerateNotifyProperties(classSymbol, out int propertyCount, out var constDefNames);

        StringBuilder constDefCode = new StringBuilder();
        foreach (var c in constDefNames)
        {
            constDefCode.AppendLine($"public const int {c.Key.ToUpper()} = {c.Value};");
        }

        var classGenCode = $@"
using System;
using System.Collections.Generic;
using Oathx.Entity;

/// <summary>
/// {classSymbol.Name}
/// </summary>
public partial class {classSymbol.Name}
{{    
    {constDefCode}
    {propertyCode}
}}";

        return classGenCode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="propertyName"></param>
    /// <param name="fieldName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string FormatPropertyCode(string propertyType, string propertyName, string fieldName, int index)
    {
        return $@"
    /// <summary>
    /// {propertyType}.{propertyName}
    /// </summary>
    public {propertyType} {propertyName} 
    {{ 
        get {{ return {fieldName}; }}
        set {{ 
            if (__assignedOnce.Add({index}) || {fieldName} != value)
            {{
                {fieldName} = value;
                Set(value, {index}, ""{fieldName}"");
            }}
        }}
    }}
";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="propertyName"></param>
    /// <param name="fieldName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string FormatNotifyPropertyCode(AttributeData attrData, string propertyType, string propertyName, string fieldName, int index)
    {
        int eventId = HashString(propertyName);

        return $@"
    /// <summary>
    /// {propertyType}.{propertyName}
    /// </summary>
    public {propertyType} {propertyName} 
    {{ 
        get {{ return {fieldName}; }}
        set {{ 
            if ({fieldName} != value)
            {{
                var old = {fieldName};

                // change value
                {fieldName} = value;
                
                NotifyComponents<{propertyType}>({eventId}, old, value);
            }}
        }}
    }}
";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="propertyName"></param>
    /// <param name="fieldName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string FormatDelegatePropertyCode(string propertyType, string propertyName, string fieldName, int index)
    {
        return $@"
    /// <summary>
    /// {propertyType}.{propertyName}
    /// </summary>
    public {propertyType} {propertyName} 
    {{ 
        get {{ return {fieldName}; }}
        set {{ 
            {fieldName} = value;
            Set(value, {index}, ""{fieldName}"");
        }}
    }}
";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private bool? GetSkipEqualsCheck(IFieldSymbol field)
    {
        foreach (var attr in field.GetAttributes())
        {
            if (attr.AttributeClass?.Name == BindingType.Binding.ToString() ||
                attr.AttributeClass?.Name == BindingType.BindingAttribute.ToString())
            {
                var namedArg = attr.NamedArguments.FirstOrDefault(kvp => kvp.Key == "SkipEqualsCheck");
                if (namedArg.Key != null && namedArg.Value.Kind == TypedConstantKind.Primitive && namedArg.Value.Value is bool skip)
                {
                    return skip;
                }

                return false;
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="propertyName"></param>
    /// <param name="fieldName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string GeneratePropertieCode(IFieldSymbol field, string propertyType, string propertyName, string fieldName, int index)
    {
        string code = FormatPropertyCode(propertyType, propertyName, fieldName, index); 

        bool? skipEqualsCheck = GetSkipEqualsCheck(field);
        if (skipEqualsCheck == true) 
            code = FormatDelegatePropertyCode(propertyType, propertyName, fieldName, index);
        
        return $"{code}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <param name="propertyType"></param>
    /// <param name="propertyName"></param>
    /// <param name="fieldName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string GenerateNotifyPropertieCode(IFieldSymbol field, string propertyType, string propertyName, string fieldName, int index)
    {
        AttributeData attrData = field.GetAttributes().FirstOrDefault(attr => attr.AttributeClass?.Name == BindingType.Notify.ToString() ||
                attr.AttributeClass?.Name == BindingType.NotifyAttribute.ToString());

        string code = FormatNotifyPropertyCode(attrData, propertyType, propertyName, fieldName, index);
        return $"{code}";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <param name="propertyCount"></param>
    /// <returns></returns>
    private string GenerateProperties(INamedTypeSymbol classSymbol, out int propertyCount)
    {
        propertyCount = 0;

        var bindingFields = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => HasBindingAttribute(field));

        StringBuilder sb = new StringBuilder();

        var fields = bindingFields.ToList();
        for (int idx = 0; idx < fields.Count; idx++)
        {
            var field = fields[idx];
            if (field != null)
            {
                var propertyName = GetPropertyName(field.Name);
                var propertyType = GetPropertyType(field);

                var code = GeneratePropertieCode(field, propertyType, propertyName, field.Name, idx);
                sb.AppendLine(code);

                propertyCount++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <param name="propertyCount"></param>
    /// <returns></returns>
    private string GenerateNotifyProperties(INamedTypeSymbol classSymbol, out int propertyCount, out Dictionary<string, int> constDefNames)
    {
        propertyCount = 0;
        constDefNames = new Dictionary<string, int>();

        var bindingFields = classSymbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(field => HasNotifyAttribute(field));

        StringBuilder sb = new StringBuilder();

        var fields = bindingFields.ToList();
        for (int idx = 0; idx < fields.Count; idx++)
        {
            var field = fields[idx];
            if (field != null)
            {
                var propertyName = GetPropertyName(field.Name);
                var propertyType = GetPropertyType(field);

                var code = GenerateNotifyPropertieCode(field, propertyType, propertyName, field.Name, idx);
                sb.AppendLine(code);

                if (!constDefNames.ContainsKey(propertyName))
                    constDefNames.Add(propertyName, HashString(propertyName));

                propertyCount++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private bool HasBindingAttribute(IFieldSymbol field)
    {
        return field.GetAttributes()
            .Any(attr =>
                attr.AttributeClass?.Name == BindingType.Binding.ToString() || attr.AttributeClass?.Name == BindingType.BindingAttribute.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private bool HasNotifyAttribute(IFieldSymbol field)
    {
        return field.GetAttributes()
            .Any(attr =>
                attr.AttributeClass?.Name == BindingType.Notify.ToString() || attr.AttributeClass?.Name == BindingType.NotifyAttribute.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private string GetPropertyName(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return str.Substring(0, 1).ToUpper() + str.Substring(1);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fieldSymbol"></param>
    /// <returns></returns>
    private string GetPropertyType(IFieldSymbol fieldSymbol)
    {
        var fieldType = fieldSymbol.Type.ToDisplayString();
        return fieldType;
    }
}

/// <summary>
/// 
/// </summary>
public class BindingSyntaxReceiver : ISyntaxReceiver
{
    public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();
    /// <summary>
    /// 
    /// </summary>
    public List<ClassDeclarationSyntax> NotifyClasses 
    { get; } = new List<ClassDeclarationSyntax>();

    /// <summary>
    /// 
    /// </summary>
    public List<InterfaceDeclarationSyntax> RpcInterfaces
    { get; } = new List<InterfaceDeclarationSyntax>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="syntaxNode"></param>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        var directory = Path.Combine(Path.GetTempPath(), typeof(FileOutputBindingGenerator).Name);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        if (syntaxNode is ClassDeclarationSyntax classDeclaration)
        {
            CollectCandidateClasses(classDeclaration, directory);
            CollectNotifyCandidateClasses(classDeclaration, directory);
        }
        else if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaration)
        {
            CollectRpcCandidateInterfaces(interfaceDeclaration, directory);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfaceDeclaration"></param>
    /// <param name="directory"></param>
    private void CollectRpcCandidateInterfaces(InterfaceDeclarationSyntax interfaceDeclaration, string directory)
    {
        var hasRpcAttribute = interfaceDeclaration.AttributeLists.SelectMany(al => al.Attributes)
                .Any(attr => attr.Name.ToString().Contains(BindingType.RpcInterface.ToString()));
        if (hasRpcAttribute)
        {
            RpcInterfaces.Add(interfaceDeclaration);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classDeclaration"></param>
    /// <param name="directory"></param>
    private void CollectCandidateClasses(ClassDeclarationSyntax classDeclaration, string directory)
    {
        var hasBindingFields = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .Any(field => field.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                    attr.Name.ToString() == BindingType.Binding.ToString() || attr.Name.ToString() == BindingType.BindingAttribute.ToString()));
        if (hasBindingFields)
        {
            File.AppendAllText(Path.Combine(directory, typeof(ClassDeclarationSyntax).Name),
                classDeclaration.Identifier.Text + Environment.NewLine);
            CandidateClasses.Add(classDeclaration);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="classDeclaration"></param>
    /// <param name="directory"></param>
    private void CollectNotifyCandidateClasses(ClassDeclarationSyntax classDeclaration, string directory)
    {
        var hasNotifyFields = classDeclaration.Members
            .OfType<FieldDeclarationSyntax>()
            .Any(field => field.AttributeLists
                .SelectMany(al => al.Attributes)
                .Any(attr =>
                    attr.Name.ToString() == BindingType.Notify.ToString() || attr.Name.ToString() == BindingType.NotifyAttribute.ToString()));
        if (hasNotifyFields)
        {
            File.AppendAllText(Path.Combine(directory, typeof(ClassDeclarationSyntax).Name),
                classDeclaration.Identifier.Text + Environment.NewLine);

            NotifyClasses.Add(classDeclaration);
        }
    }
}