using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace e2cs
{
    /// <summary>
    /// output type
    /// </summary>
    public enum OutputType
    {
        Unknow = 0,
        CSharp = 1,
        JavaScript = 2,
        Lua = 4,
        Json = 8,
        GoogleProtocBuff = 16,
        MessagePack = 32
    }

    /// <summary>
    /// directory type
    /// </summary>
    public enum DirectoryType
    {
        Data,
        Config,
        Json,
        Excel,
        Client,
        Server,
        Common,
        Js,
        Dlls,
        Language,
        Jscode
    }

    /// <summary>
    /// file extension
    /// </summary>
    public enum Extension
    {
        cs,
        js,
        lua,
        json,
        bytes,
        dll,
    }

    /// <summary>
    /// excel type field
    /// </summary>
    enum ExcelTypeFiled
    {
        name,
        data
    }

    /// <summary>
    /// list method
    /// </summary>
    enum ListMethod
    {
        Add
    }

    /// <summary>
    /// utility class, main process, read xlsx files, generate csharp code, compile and write output files, json,
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// get xlsx files, and filter temp files start with ~, only support .xlsx files, 
        /// and the file name should contain language name, like "config_en.xlsx" for english language
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        public static List<string> GetFiles(Context context)
        {
            List<string> files = new List<string>();
            if (!string.IsNullOrEmpty(context.InputDirectory))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.InputDirectory);
                if (!Directory.Exists(path))
                    throw new FieldAccessException($"Can't find directory {path}");

                string[] inputFiles = Directory.GetFiles(path, "*.xlsx");
                foreach (var file in inputFiles)
                {
                    var index = file.IndexOf('~');
                    if (index < 0)
                        files.Add(file);
                }
            }

            return files;
        }

        const string cs_common_name = @"Excel";
        const string cs_common_code = @"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace {0} 
{{
    #pragma warning disable IDE0090
    #pragma warning disable IDE0066

    public interface IConfigData
    {{
        string GetName();
        object GetData();
    }}

    /// <summary>
    /// 
    /// </summary>
    public class {1}<T> : IConfigData
    {{
        /// <summary>
        ///  field name
        /// </summary>
        public string name
        {{get; set;}}
        
        /// <summary>
        /// all datas
        /// </summary>
        public Dictionary<int, T> data
        {{ get; set; }}

        /// <summary>
        /// get field name
        /// </summary>
        public string GetName()
        {{
            return name;
        }}

        /// <summary>
        /// get config data
        /// </summary>
        public object GetData()
        {{
            return data;
        }}
    }}

    public partial class Excel
    {{
    
    }}

    #pragma warning restore IDE0066
    #pragma warning restore IDE0090
}}
";

        /// <summary>
        /// try write common files
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfos"></param>
        /// <returns></returns>
        public static bool TryWriteCommonFiles(Context context, out List<ExportFileInfo> fileInfos)
        {
            fileInfos = new List<ExportFileInfo>();

            if (!string.IsNullOrEmpty(context.OutputDirectory))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory, OutputType.CSharp.ToString().ToLower(),
                    DirectoryType.Common.ToString().ToLower());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var commonFileInfo = GetCommonFileInfo(context);
                fileInfos.Add(commonFileInfo);

                string file = Path.Combine(path, cs_common_name);
                if (!Path.HasExtension(Extension.cs.ToString()))
                    file = Path.ChangeExtension(file, Extension.cs.ToString());

                File.WriteAllText(file, commonFileInfo.code);
            }

            return true;
        }

        /// <summary>
        /// common file info
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ExportFileInfo GetCommonFileInfo(Context context)
        {
            string code = string.Format(cs_common_code, context.Namespace, context.Title);
            return new ExportFileInfo()
            {
                code = code,
                name = cs_common_name,
                file = cs_common_name,
            };
        }

        /// <summary>
        /// syntax tree info
        /// </summary>
        class SyntaxTreeInfo
        {
            public required SyntaxTree syntaxTree;
            public required string file;
        }

        /// <summary>
        /// try compile csharp code, only for csharp code,
        /// </summary>
        /// <param name="context"></param>
        /// <param name="files"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool TryCompilation(Context context, ExportFileInfo[] files,  Action<MemoryStream, List<ExportFileInfo>> callback)
        {
            List<ExportFileInfo> fullNormalizeString = new List<ExportFileInfo>();

            try
            {
                Dictionary<string, SyntaxTreeInfo> syntaxs = new Dictionary<string, SyntaxTreeInfo>();
                foreach (var file in files)
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(file.code);
                    syntaxs.Add(file.name, new SyntaxTreeInfo()
                    {
                        file = file.file,
                        syntaxTree = syntaxTree
                    });
                }

                List<MetadataReference> metdatas = new List<MetadataReference>();
                var assemblys = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblys)
                {
                    var mr = MetadataReference.CreateFromFile(assembly.Location);
                    if (mr != null)
                        metdatas.Add(mr);
                }

                List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
                foreach (var s in syntaxs)
                {
                    syntaxTrees.Add(s.Value.syntaxTree);
                }

                var compilation = CSharpCompilation.Create(context.OutDll,
                    syntaxTrees.ToArray(), metdatas.ToArray(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);
                    if (result.Success)
                    {
                        foreach (var syntax in syntaxs)
                        {
                            var index = System.Array.FindIndex(files, (v) => v.name == syntax.Key);
                            if (index >= 0)
                            {
                                var normalize = syntax.Value.syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();
                                fullNormalizeString.Add(new ExportFileInfo()
                                {
                                    file = syntax.Value.file,
                                    name = syntax.Key,
                                    code = normalize,
                                    sheet = files[index].sheet
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(context.OutDll))
                            WriteDll(context, ms, context.OutDll);

                        callback?.Invoke(ms, fullNormalizeString);

                        return true;
                    }
                    else
                    {
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            Console.WriteLine(diagnostic.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// try compile csharp code, only for csharp code,
        /// </summary>
        /// <param name="context"></param>
        /// <param name="files"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static bool TryLanguageCompilation(Context context, ExportFileInfo[] files, Action<MemoryStream, List<ExportFileInfo>> callback)
        {
            List<ExportFileInfo> fullNormalizeString = new List<ExportFileInfo>();

            try
            {
                Dictionary<string, SyntaxTreeInfo> syntaxs = new Dictionary<string, SyntaxTreeInfo>();
                foreach (var file in files)
                {
                    var syntaxTree = CSharpSyntaxTree.ParseText(file.code);
                    syntaxs.Add(file.name, new SyntaxTreeInfo()
                    {
                        file = file.file,
                        syntaxTree = syntaxTree
                    });
                }

                List<MetadataReference> metdatas = new List<MetadataReference>() {
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                };

                List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
                foreach (var s in syntaxs)
                {
                    syntaxTrees.Add(s.Value.syntaxTree);
                }

                var compilation = CSharpCompilation.Create(context.OutDll,
                    syntaxTrees.ToArray(), metdatas.ToArray(), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);
                    if (result.Success)
                    {
                        foreach (var syntax in syntaxs)
                        {
                            var index = System.Array.FindIndex(files, (v) => v.name == syntax.Key);
                            if (index >= 0)
                            {
                                var normalize = syntax.Value.syntaxTree.GetRoot().NormalizeWhitespace().ToFullString();
                                fullNormalizeString.Add(new ExportFileInfo()
                                {
                                    file = syntax.Value.file,
                                    name = syntax.Key,
                                    code = normalize,
                                    sheet = files[index].sheet
                                });
                            }
                        }

                        if (!string.IsNullOrEmpty(context.OutDll))
                            WriteDll(context, ms, context.OutDll);

                        callback?.Invoke(ms, fullNormalizeString);

                        return true;
                    }
                    else
                    {
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            Console.WriteLine(diagnostic.ToString());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
            }

            return false;
        }

        /// <summary>
        /// write dll file, only for csharp code,
        /// and the dll will be used to generate json and javascript code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="config"></param>
        /// <param name="ms"></param>
        /// <param name="dllName"></param>
        /// <returns></returns>
        public static bool WriteDll(Context config, MemoryStream ms, string dllName)
        {
            if (string.IsNullOrEmpty(config.OutDll))
                return false;

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.OutputDirectory, DirectoryType.Dlls.ToString().ToLower());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string file = Path.Combine(path, dllName);
            if (!Path.HasExtension(Extension.dll.ToString()))
                file = Path.ChangeExtension(file, Extension.dll.ToString());

            File.WriteAllBytes(file, ms.ToArray());

            return true;
        }

        /// <summary>
        /// main process, read xlsx files, generate csharp code, compile and write output files, json, 
        /// javascript and csharp code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        public static void DoExports(Context context)
        {
            List<Export> exports = new List<Export>();
            var files = GetFiles(context);
            foreach (var file in files)
            {
                if (!file.Contains(context.Language))
                {
                    ExcelInfo info = new ExcelInfo();
                    if (info.Load(file))
                        exports.Add(new CSharp(info));
                }
            }

            if (TryWriteCommonFiles(context, out var commonFileInfos))
            {
                List<ExportFileInfo> exportFileInfos = new List<ExportFileInfo>(commonFileInfos);
                foreach (var export in exports)
                {
                    if (export.Run(context, out var exportFiles))
                    {
                        exportFileInfos.AddRange(exportFiles);
                    }
                }

                string enumCode = CSharp.GenerateEnums(context);
                if (!string.IsNullOrEmpty(enumCode))
                {
                    string cspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                        OutputType.CSharp.ToString().ToLower(), DirectoryType.Client.ToString().ToLower());
                    if (!Directory.Exists(cspath))
                        Directory.CreateDirectory(cspath);

                    string enumFileName = context.EnumFileName + ".cs";
                    File.WriteAllText(Path.Combine(cspath, enumFileName), enumCode);

                    exportFileInfos.Add(new ExportFileInfo() { 
                        code=enumCode, name = enumFileName, file = enumFileName 
                    });
                }

                bool result = TryCompilation(context, exportFileInfos.ToArray(), (ms, fullNormalizeFileInfos) => {
                    WriteFiles(context, ms, fullNormalizeFileInfos, commonFileInfos);
                });
            }
        }

        /// <summary>
        /// write output files, json, javascript and csharp code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ms"></param>
        /// <param name="fullNormalizeString"></param>
        /// <param name="filters"></param>
        static void WriteFiles(Context context, MemoryStream ms, List<ExportFileInfo> fullNormalizeString, List<ExportFileInfo> filters)
        {
            bool succeed = ExportCSharp(context, fullNormalizeString, filters);
            if (succeed)
                OutputFiles(context, ms, fullNormalizeString);   
        }

        /// <summary>
        /// output csharp code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfos"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        static bool ExportCSharp(Context context, List<ExportFileInfo> fileInfos, List<ExportFileInfo> filters)
        {
            if (string.IsNullOrEmpty(context.OutputDirectory))
                return false;

            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory, OutputType.CSharp.ToString().ToLower(),
                    DirectoryType.Client.ToString().ToLower());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                foreach (var fileInfo in fileInfos)
                {
                    var index = filters.FindIndex((v) => v.name == fileInfo.name);
                    if (index < 0)
                    {
                        string file = Path.Combine(path, fileInfo.name);
                        if (!Path.HasExtension(Extension.cs.ToString()))
                            file = Path.ChangeExtension(file, Extension.cs.ToString());

                        File.WriteAllText(file, fileInfo.code);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// output json and javascript code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ms"></param>
        /// <param name="fullNormalizeString"></param>
        static void OutputFiles(Context context, MemoryStream ms, List<ExportFileInfo> fullNormalizeString)
        {
            var assembly = Assembly.Load(ms.ToArray());
            if (assembly != null)
            {
                foreach (var fileInfo in fullNormalizeString)
                {
                    GenerateClassObject(context, assembly, fileInfo, (instance, refTypeDefine) => {
                        WriteJson(context, instance);

                        if (fileInfo.sheet != null)
                            WriteJscode(context, instance, fileInfo.sheet.consts);
                    });
                }

                LanguageUtils.GenerateLanguageXlsx(context);
            }
        }

        /// <summary>
        /// generate javascript code for consts, only support int consts
        /// </summary>
        /// <param name="context"></param>
        /// <param name="consts"></param>
        /// <returns></returns>
        public static string GenerateJsConstCode(Context context, Dictionary<string, int> consts)
        {
            StringBuilder cc = new StringBuilder();

            foreach (var c in consts)
            {
                var line = string.Format("{0}={1}", c.Key, c.Value);
                cc.AppendLine(line);
            }

            return cc.ToString();
        }

        /// <summary>
        /// write javascript code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classInst"></param>
        /// <param name="consts"></param>
        /// <returns></returns>
        public static bool WriteJscode(Context context, object classInst, Dictionary<string, int> consts)
        {
            var constCode = GenerateJsConstCode(context, consts);

            string code = @"
module('resmng');
svnnum('$Id$');

{0}

prop{1} = {{
    {2}
}}
";

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                DirectoryType.Jscode.ToString().ToLower());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var fields = classInst.GetType().GetProperties();
            foreach (var field in fields)
            {
                if (field.Name == ExcelTypeFiled.data.ToString())
                {
                    var jscode = Convert2JavaScriptCode(classInst, field);
                    if (!string.IsNullOrEmpty(jscode))
                    {
                        var jstext = string.Format(code, constCode, classInst.GetType().Name, jscode);

                        string file = Path.Combine(path, classInst.GetType().Name);
                        if (!Path.HasExtension(Extension.bytes.ToString()))
                            file = Path.ChangeExtension(file, Extension.js.ToString());

                        File.WriteAllText(file, jstext);
                    }

                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// property name for dictionary, only support int key, and value of int, float, double, short, long, string and class
        /// </summary>
        public enum PropertyName
        {
            Item,
            Keys,
            Values,
            ToArray,
        }

        /// <summary>
        /// convert class instance to javascript code, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="classInst"></param>
        /// <param name="pr"></param>
        /// <returns></returns>
        public static string Convert2JavaScriptCode(object classInst, PropertyInfo pr)
        {
            StringBuilder sb = new StringBuilder();

            var datas = pr.GetValue(classInst);
            if (datas != null)
            {
                List<string> codes = new List<string>();

                var keys = datas.GetType().GetProperty(PropertyName.Keys.ToString());
                var item = datas.GetType().GetProperty(PropertyName.Item.ToString());
                if (item != null && keys != null)
                {
                    var get = item.GetGetMethod();
                    if (get != null)
                    {
                        ICollection<int>? ids = keys.GetValue(datas) as ICollection<int>;
                        if (ids != null)
                        {
                            foreach (var id in ids)
                            {
                                var val = get.Invoke(datas, [id]);
                                
                                var text = string.Format("[{0}]:{1}", id, FormatObject(val));
                                codes.Add(text);
                            }
                        }
                    }
                }

                for (int idx = 0; idx < codes.Count; idx++)
                {
                    if (idx != 0)
                        sb.Append('\t');

                    sb.Append(codes[idx]);

                    if (idx < codes.Count - 1)
                        sb.Append(',');

                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// format object to json like string, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        public static string FormatObject(object? inst)
        {
            if (inst == null)
                return "null";

            StringBuilder sb = new StringBuilder();

            List<string> fields = new List<string>();

            var properties = inst.GetType().GetProperties();
            foreach (var pr in properties)
            {
                var value = pr.GetValue(inst);
                if (value != null)
                {
                    if (value is int)
                    {
                        fields.Add(FormatValue(pr, value));
                    }
                    else if (value is float)
                    {
                        fields.Add(FormatValue(pr, value));
                    }
                    else if (value is short)
                    {
                        fields.Add(FormatValue(pr, value));
                    }
                    else if (value is double)
                    {
                        fields.Add(FormatValue(pr, value));
                    }
                    else if (value is long)
                    {
                        fields.Add(FormatValue(pr, value));
                    }
                    else if (value is string)
                    {
                        fields.Add(FormatString(pr, value));
                    }
                    else
                    {
                        if (pr.PropertyType.IsGenericType && pr.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            var args = pr.PropertyType.GetGenericArguments();
                            foreach (var arg in args)
                            {
                                var to = value.GetType().GetMethod(PropertyName.ToArray.ToString());
                                if (to == null)
                                    continue;

                                if (arg == typeof(int))
                                {
                                    var text = Convert<int>(pr, value, arg, to);
                                    fields.Add(text);
                                }
                                else if (arg == typeof(float))
                                {
                                    var text = Convert<float>(pr, value, arg, to);
                                    fields.Add(text);
                                }
                                else if (arg == typeof(double))
                                {
                                    var text = Convert<double>(pr, value, arg, to);
                                    fields.Add(text);
                                }
                                else if (arg == typeof(short))
                                {
                                    var text = Convert<short>(pr, value, arg, to);
                                    fields.Add(text);
                                }
                                else if (arg == typeof(long))
                                {
                                    var text = Convert<long>(pr, value, arg, to);
                                    fields.Add(text);
                                }
                                else if (arg == typeof(string))
                                {
                                    List<string> strs = new List<string>();
                                    string[]? array = to.Invoke(value, []) as string[];
                                    if (array != null)
                                    {
                                        foreach (var arr in array)
                                            strs.Add($"\"{arr}\"");

                                        var text = string.Format("{0}:[{1}]", pr.Name, string.Join(",", strs.ToArray()));
                                        fields.Add(text);
                                    }
                                }
                                else if (arg.IsClass)
                                {
                                    List<string> strs = new List<string>();
                                    object[]? array = to.Invoke(value, []) as object[];
                                    if (array != null)
                                    {
                                        foreach (var o in array)
                                        {
                                            string text = FormatObject(o);
                                            strs.Add(text);
                                        }

                                        fields.Add(string.Format("{0}:[{1}]", pr.Name, string.Join(",", strs.ToArray())));
                                    }
                                }
                            }
                        }
                        else if (pr.PropertyType.IsClass)
                        {
                            string text = FormatObject(value);
                            fields.Add(string.Format("{0}:{1}", pr.Name, text));
                        }
                    }
                }
            }

            string line = string.Join(",", fields.ToArray());
            sb.Append(string.Format("{{{0}}}", line));

            return sb.ToString();
        }

        /// <summary>
        /// format list value, int, float, double, short, long, string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pr"></param>
        /// <param name="value"></param>
        /// <param name="arg"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        static string Convert<T>(PropertyInfo pr, object value, Type arg, MethodInfo method)
        {
            if (method != null)
            {
                T[]? array = method.Invoke(value, []) as T[];
                if (array != null)
                {
                    return string.Format("{0}:[{1}]", pr.Name, string.Join(",", array));
                }
            }

            return "";
        }

        /// <summary>
        /// format number value, int, float, double, short, long
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static string FormatValue(PropertyInfo pr, object value)
        {
            return $"{pr.Name}:{value}";
        }

        /// <summary>
        /// format string, escape \ and "
        /// </summary>
        /// <param name="pr"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        static string FormatString(PropertyInfo pr, object value)
        {
            string str = value.ToString() ?? "";
            str = str.Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"{pr.Name}:\"{str}\"";
        }

        /// <summary>
        /// write json file
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classInst"></param>
        /// <returns></returns>
        public static bool WriteJson(Context context, object classInst)
        {
            var text = JsonConvert.SerializeObject(classInst, Formatting.Indented);
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(context.OutputDirectory))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                    DirectoryType.Json.ToString().ToLower());
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string file = Path.Combine(path, classInst.GetType().Name);
                if (!Path.HasExtension(Extension.bytes.ToString()))
                    file = Path.ChangeExtension(file, Extension.bytes.ToString());

                File.WriteAllText(file, text);
            }

            return true;
        }

        /// <summary>
        /// generate class object, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="fileInfo"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static bool GenerateClassObject(Context context, Assembly assembly, ExportFileInfo fileInfo, Action<object, RefTypeInfo> callback)
        {
            var excelSheet = fileInfo.sheet;
            if (excelSheet != null)
            {
                try
                {
                    var refTypeInfos = excelSheet.refTypeInfos;
                    foreach (var refTypeDefine in refTypeInfos)
                    {
                        if (refTypeDefine.source == RefTypeSrc.Main)
                        {
                            var refTypeName = excelSheet.sheet.SheetName + context.Title;
                            if (!string.IsNullOrEmpty(refTypeName))
                            {
                                var classInst = CreateInstance(context, assembly, refTypeName);
                                if (TryFull(context, fileInfo, assembly, excelSheet, classInst, refTypeDefine, refTypeName))
                                {
                                    callback?.Invoke(classInst, refTypeDefine);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.ToString());
                }                
            }

            return true;
        }

        /// <summary>
        /// create class instance, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="typeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public static object CreateInstance(Context context, Assembly assembly, string typeName)
        {
            var classType = assembly.GetType(string.Format("{0}.{1}", context.Namespace, typeName));
            if (classType == null)
                throw new ArgumentNullException($"Can't find ref type {typeName}");

            var classInit = Activator.CreateInstance(classType);
            if (classInit == null)
                throw new NullReferenceException($"Can't find instance {typeName}");

            return classInit;
        }

        /// <summary>
        /// try full fill class instance, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <param name="assembly"></param>
        /// <param name="excelSheet"></param>
        /// <param name="instance"></param>
        /// <param name="refTypeDefine"></param>
        /// <param name="refTypeName"></param>
        /// <returns></returns>
        static bool TryFull(Context context, ExportFileInfo fileInfo, Assembly assembly, ExcelSheetInfo excelSheet, object instance, RefTypeInfo refTypeDefine, string refTypeName)
        {
            try
            {
                var name = instance.GetType().GetProperty(ExcelTypeFiled.name.ToString());
                var data = instance.GetType().GetProperty(ExcelTypeFiled.data.ToString());
                if (name == null || data == null)
                    return false;

                var inst = Activator.CreateInstance(data.PropertyType);
                if (inst != null)
                {
                    var func = inst.GetType().GetMethod(ListMethod.Add.ToString());
                    if (func != null)
                    {
                        foreach (var row in excelSheet.refDataInfos)
                        {
                            var typeName = refTypeDefine.name;
                            if (!string.IsNullOrEmpty(typeName))
                            {
                                var rowInst = CreateInstance(context, assembly, typeName);
                                if (rowInst != null)
                                {
                                    var propertices = rowInst.GetType().GetProperties();

                                    for (int index = 0; index < row.cells.Count; index++)
                                    {
                                        var member = row.cells[index];
                                        var prInfo = propertices[index];

                                        UpdatePropertyInfo(context, fileInfo, assembly, excelSheet, row.numberId, rowInst, prInfo, member);
                                    }

                                    func.Invoke(inst, [
                                        row.numberId, rowInst
                                    ]);
                                }
                            }
                        }
                    }
                }

                name.SetValue(instance, ConvertFieldName(instance.GetType().Name));
                data.SetValue(instance, inst);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.ToString());
            }
            
            return true;
        }

        /// <summary>
        /// convert field name, like "ExcelConfig" to "excelConfig", only for csharp code, and the field name is used for json and javascript code
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ConvertFieldName(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }

        /// <summary>
        /// update property value, only support int, float, double, short, long, string, list and class
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <param name="assembly"></param>
        /// <param name="excelSheet"></param>
        /// <param name="numberId"></param>
        /// <param name="inst"></param>
        /// <param name="prInfo"></param>
        /// <param name="refDataCell"></param>
        /// <exception cref="ArgumentException"></exception>
        static void UpdatePropertyInfo(Context context, ExportFileInfo fileInfo, Assembly assembly, ExcelSheetInfo excelSheet,
            int numberId, object inst, PropertyInfo prInfo, RefDataCell refDataCell)
        {
            if (refDataCell.type != null && refDataCell.name != null)
            {
                var type = refDataCell.type.StringCellValue;
                var name = refDataCell.name.StringCellValue;

                if (prInfo.Name != name)
                    throw new ArgumentException($"Property name mismatch {prInfo.Name}/{name} in {excelSheet.sheet.SheetName}");

                var dataType = TryParseDataType(type, out string refParam, out string refChunk);
                if (dataType.HasFlag(DataType.List))
                {
                    var element = TryParseList(context, fileInfo, assembly, dataType, excelSheet, numberId, prInfo, refChunk, refDataCell);
                    prInfo.SetValue(inst, element);
                }
                else if (dataType.HasFlag(DataType.Int))
                {
                    int value = (int)TryParseNumber(excelSheet, refDataCell);
                    prInfo.SetValue(inst, value);
                }
                else if (dataType.HasFlag(DataType.Float))
                {
                    float value = (float)TryParseNumber(excelSheet, refDataCell);
                    prInfo.SetValue(inst, value);
                }
                else if (dataType.HasFlag(DataType.Double))
                {
                    double value = TryParseNumber(excelSheet, refDataCell);
                    prInfo.SetValue(inst, value);
                }
                else if (dataType.HasFlag(DataType.String))
                {
                    string value = TryParseString(excelSheet, fileInfo, numberId, refDataCell);
                    prInfo.SetValue(inst, value);
                }
                else if (dataType.HasFlag(DataType.Struct))
                {
                    var structInst = TryParseStruct(context, assembly, excelSheet, numberId, refChunk, refDataCell);
                    prInfo.SetValue(inst, structInst);
                }
                else if (dataType.HasFlag(DataType.Enum))
                {
                    var enumValue = TryParseEnum(context, excelSheet, assembly, refDataCell);
                    prInfo.SetValue(inst, enumValue);
                }
            }
        }

        /// <summary>
        /// convert enum name, like "Color" to "EColor", only for csharp code, and the enum name is used for json and javascript code
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string ConvertToEnum(string name)
        {
            return "E" + name;
        }

        /// <summary>
        /// try parse enum value, only support int enum, and the enum name should be like "EColor", and the enum value should be like "Red|Green|Blue", only for csharp code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="excelSheet"></param>
        /// <param name="assembly"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        static object? TryParseEnum(Context context, ExcelSheetInfo excelSheet, Assembly assembly, RefDataCell member)
        {
            var nameCell = member.name;
            var dataCell = member.data;

            if (nameCell != null && dataCell != null)
            {
                string value = ConvertToEnum(nameCell.StringCellValue);

                var type = assembly.GetType($"{context.Namespace}.{value}");
                if (type != null)
                {
                    int enumValue = 0;
                    string[] splits = dataCell.StringCellValue.Split('|');
                    foreach(var name in splits)
                    {
                        if (Enum.TryParse(type, name, out var result))
                        {
                            enumValue |= (int)(result);
                        }
                    }

                    return Enum.ToObject(type, enumValue);
                }
            }

            return null;
        }

        /// <summary>
        /// try parse list value, only support list of int, float, double, short, long, string and struct, and the list value should be like "{1,2,3}", only for csharp code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <param name="assembly"></param>
        /// <param name="dataType"></param>
        /// <param name="excelSheet"></param>
        /// <param name="numberId"></param>
        /// <param name="prInfo"></param>
        /// <param name="refChunk"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        static object? TryParseList(Context context, ExportFileInfo fileInfo, Assembly assembly, DataType dataType, ExcelSheetInfo excelSheet, int numberId,
            PropertyInfo prInfo, string refChunk, RefDataCell member)
        {
            var elements = Activator.CreateInstance(prInfo.PropertyType);
            if (elements != null)
            {
                var dataCell = member.data;
                if (dataCell != null)
                {
                    MethodInfo? add = prInfo.PropertyType.GetMethod(ListMethod.Add.ToString());
                    if (add != null)
                    {
                        if (dataType.HasFlag(DataType.Struct))
                        {
                            TryParsetStructList(context, assembly, excelSheet, numberId, refChunk, member, elements, add);
                        }
                        else if (dataType.HasFlag(DataType.Int))
                        {
                            TryAddList(member, elements, add, int.Parse);
                        }
                        else if (dataType.HasFlag(DataType.Float))
                        {
                            TryAddList(member, elements, add, float.Parse);
                        }
                        else if (dataType.HasFlag(DataType.Double))
                        {
                            TryAddList(member, elements, add, double.Parse);
                        }
                        else if (dataType.HasFlag(DataType.Long))
                        {
                            TryAddList(member, elements, add, long.Parse);
                        }
                        else if (dataType.HasFlag(DataType.Bool))
                        {
                            TryAddList(member, elements, add, bool.Parse);
                        }
                        else if (dataType.HasFlag(DataType.String))
                        {
                            var match = Regex.Match(dataCell.StringCellValue.Trim(), @"\{([^\{\}]+)\}");
                            if (match.Success)
                            {
                                var intList = match.Value.Substring(1, match.Value.Length - 2);
                                string[] splits = intList.Split(',');
                                for (int index = 0; index < splits.Length; index++)
                                {
                                    var value = LanguageUtils.ConvertLanguageText(excelSheet, fileInfo.file, numberId, prInfo.Name, splits[index],
                                        dataCell.RowIndex, dataCell.ColumnIndex, index);

                                    add.Invoke(elements, [value]);
                                }
                            }
                        } 
                    }
                }
            }

            return elements;
        }

        /// <summary>
        /// try add list value, only support list of int, float, double, short, long, string and struct, and the list value should be like "{1,2,3}", only for csharp code
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="member"></param>
        /// <param name="elements"></param>
        /// <param name="add"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        static bool TryAddList<T>(RefDataCell member, object elements, MethodInfo add, Func<string, T> callback)
        {
            var dataCell = member.data;
            if (dataCell == null || dataCell.CellType == CellType.Blank)
                return false;

            var match = Regex.Match(dataCell.StringCellValue.Trim(), @"\{([^\{\}]+)\}");
            if (!match.Success)
                return false;

            var intList = match.Value.Substring(1, match.Value.Length - 2);
            string[] splits = intList.Split(',');
            foreach (var s in splits)
            {
                add.Invoke(elements, [callback(s)]);
            }

            return true;
        }

        /// <summary>
        /// try parse struct list value, only support list of struct, and the list value should be like "{1,2,3}", and the struct field should be like "int:id,string:name", only for csharp code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="excelSheet"></param>
        /// <param name="numberId"></param>
        /// <param name="refChunk"></param>
        /// <param name="memeber"></param>
        /// <param name="elements"></param>
        /// <param name="add"></param>
        /// <exception cref="NotSupportedException"></exception>
        static void TryParsetStructList(Context context, Assembly assembly, ExcelSheetInfo excelSheet,
            int numberId, string refChunk, RefDataCell memeber, object elements, MethodInfo add)
        {
            var nameCell = memeber.name;
            var dataCell = memeber.data;
            if (nameCell != null && dataCell != null)
            {
                var value = dataCell.StringCellValue;
                if (!string.IsNullOrEmpty(value))
                {
                    var matchs = Regex.Matches(value.Trim(), @"\{([^\{\}]+)\}");
                    foreach (var match in matchs)
                    {
                        var current = match.ToString();
                        if (current != null)
                        {
                            var element = current.Substring(1, current.Length - 2);
                            if (!string.IsNullOrEmpty(element))
                            {
                                var inst = CreateInstance(context, assembly, excelSheet.sheet.SheetName + nameCell.StringCellValue);
                                add.Invoke(elements, [inst]);

                                var propertys = inst.GetType().GetProperties();

                                List<StructField> fields = new List<StructField>();
                                string[] splits = refChunk.Split(',');
                                foreach (var v in splits)
                                {
                                    string[] field = v.Split(':');
                                    if (field.Length == 2)
                                    {
                                        fields.Add(new StructField()
                                        {
                                            type = field[0],
                                            name = field[1]
                                        });
                                    }
                                }

                                string[] values = element.Split(',');
                                for (int idx = 0; idx < values.Length; idx++)
                                {
                                    var pr = propertys[idx];

                                    DataType fieldType = TryParseDataType(fields[idx].type,
                                        out string refStructParam, out string refStructChunk);
                                    switch (fieldType)
                                    {
                                        case DataType.Int:
                                            pr.SetValue(inst, int.Parse(values[idx]));
                                            break;
                                        case DataType.Float:
                                            pr.SetValue(inst, float.Parse(values[idx]));
                                            break;
                                        case DataType.Double:
                                            pr.SetValue(inst, double.Parse(values[idx]));
                                            break;
                                        case DataType.Bool:
                                            pr.SetValue(inst, bool.Parse(values[idx]));
                                            break;
                                        case DataType.Long:
                                            pr.SetValue(inst, long.Parse(values[idx]));
                                            break;
                                        case DataType.String:
                                            pr.SetValue(inst, values[idx]);
                                            break;

                                        default:
                                            throw new NotSupportedException(fieldType.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// try parse struct value, only support struct of int, float, double, short, long, string, 
        /// and the struct value should be like "{1,2,3}", and the struct field should be like "int:id,string:name", only for csharp code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="assembly"></param>
        /// <param name="excelSheet"></param>
        /// <param name="numberId"></param>
        /// <param name="refChunk"></param>
        /// <param name="memeber"></param>
        /// <returns></returns>
        static object? TryParseStruct(Context context, Assembly assembly, ExcelSheetInfo excelSheet, int numberId, string refChunk, RefDataCell memeber)
        {
            var nameCell = memeber.name;
            var dataCell = memeber.data;
            if (nameCell != null && dataCell != null)
            {
                string typeName = excelSheet.sheet.SheetName + nameCell.StringCellValue;
                var inst = CreateInstance(context, assembly, typeName);
                if (inst != null)
                {
                    if (UpdateStruct(inst, numberId, memeber, refChunk))
                        return inst;
                }
            }

            return null;
        }

        /// <summary>
        /// try update struct value, only support struct of int, float, double, short, long, string,
        /// </summary>
        /// <param name="inst"></param>
        /// <param name="numberId"></param>
        /// <param name="member"></param>
        /// <param name="refChunk"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        static bool UpdateStruct(object inst, int numberId, RefDataCell member, string refChunk)
        {
            var nameCell = member.name;
            var dataCell = member.data;

            if (nameCell != null && dataCell != null)
            {
                var value = dataCell.StringCellValue;
                if (!string.IsNullOrEmpty(value))
                {
                    var propertys = inst.GetType().GetProperties();

                    List<StructField> fields = new List<StructField>();
                    string[] splits = refChunk.Split(',');
                    foreach (var v in splits)
                    {
                        string[] field = v.Split(':');
                        if (field.Length == 2)
                        {
                            fields.Add(new StructField()
                            {
                                type = field[0],
                                name = field[1]
                            });
                        }
                    }

                    var matchs = Regex.Matches(value.Trim(), @"\{([^\{\}]+)\}");
                    if (matchs.Count <= 0)
                        return false;

                    foreach (var match in matchs)
                    {
                        var current = match.ToString();
                        if (current != null)
                        {
                            var element = current.Substring(1, current.Length - 2);
                            if (!string.IsNullOrEmpty(element))
                            {
                                string[] values = element.Split(',');
                                for (int idx = 0; idx < values.Length; idx++)
                                {
                                    var pr = propertys[idx];

                                    DataType fieldType = TryParseDataType(fields[idx].type,
                                        out string refStructParam, out string refStructChunk);
                                    switch (fieldType)
                                    {
                                        case DataType.Int:
                                            pr.SetValue(inst, int.Parse(values[idx]));
                                            break;
                                        case DataType.Float:
                                            pr.SetValue(inst, float.Parse(values[idx]));
                                            break;
                                        case DataType.Double:
                                            pr.SetValue(inst, double.Parse(values[idx]));
                                            break;
                                        case DataType.Bool:
                                            pr.SetValue(inst, bool.Parse(values[idx]));
                                            break;
                                        case DataType.Long:
                                            pr.SetValue(inst, long.Parse(values[idx]));
                                            break;
                                        case DataType.String:
                                            pr.SetValue(inst, values[idx]);
                                            break;

                                        default:
                                            throw new NotSupportedException(fieldType.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// try parse string value, only support string and date time, and the date time should be like "2024-01-01 12:00:00", only for csharp code
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="fileInfo"></param>
        /// <param name="numberId"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        static string TryParseString(ExcelSheetInfo sheet, ExportFileInfo fileInfo, int numberId, RefDataCell cell)
        {
            var dataCell = cell.data;
            var nameCell = cell.name;

            string value = "";
            if (dataCell != null && nameCell != null)
            {
                switch (dataCell.CellType)
                {
                    case CellType.Numeric:
                        if (DateUtil.IsCellDateFormatted(dataCell))
                        {
                            DateTime? time = dataCell.DateCellValue;

                            string? timeString = time?.ToString("yyyy-MM-dd hh:MM:ss");
                            if (!string.IsNullOrEmpty(timeString))
                                value = timeString;
                        }
                        else
                        {
                            value = dataCell.NumericCellValue.ToString();
                        }
                        break;

                    case CellType.String:
                        value = dataCell.StringCellValue.ToString();
                        break;
                }

                value = LanguageUtils.ConvertLanguageText(sheet, fileInfo.file, numberId, nameCell.StringCellValue, value,
                    dataCell.RowIndex, dataCell.ColumnIndex);
            }

            return value;
        }

        /// <summary>
        /// try parse number value, only support int, float, double, short, long and bool, and the bool value should be like "true" or "false", only for csharp code
        /// </summary>
        /// <param name="excelSheet"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        static double TryParseNumber(ExcelSheetInfo excelSheet, RefDataCell cell)
        {
            var dataCell = cell.data;
            var nameCell = cell.name;

            if (dataCell != null && nameCell != null)
            {
                switch (dataCell.CellType)
                {
                    case CellType.Numeric:
                        return dataCell.NumericCellValue;

                    case CellType.String:
                        bool check = excelSheet.consts.TryGetValue(dataCell.StringCellValue, out int value);
                        if (check)
                            return value;
                        
                        if (!string.IsNullOrEmpty(dataCell.StringCellValue))
                        {
                            if (int.TryParse(dataCell.StringCellValue, out var intValue))
                                return intValue;
                        }

                        return 0;

                    case CellType.Boolean:
                        return dataCell.BooleanCellValue ? 1 : 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// try parse data type, only support int, float, double, short, long, string, list and struct,
        /// and the list value should be like "list(int)", "list<string>" or "list{int:id,string:name}", only for csharp code
        /// </summary>
        static readonly Dictionary<string, DataType> DataTypeMap = new Dictionary<string, DataType>(){
            { "int", DataType.Int}, 
            { "string", DataType.String}, 
            { "bool", DataType.Bool}, 
            { "float", DataType.Float }, 
            { "double", DataType.Double },
            { "long", DataType.Long },
            { "enum", DataType.Enum }
        };

        /// <summary>
        /// try parse data type, only support int, float, double, short, long, string, list and struct,
        /// </summary>
        /// <param name="typeValue"></param>
        /// <param name="refParam"></param>
        /// <param name="refChunk"></param>
        /// <returns></returns>
        public static DataType TryParseDataType(string typeValue, out string refParam, out string refChunk)
        {
            Dictionary<int, string> patterns = new Dictionary<int, string>()
            {
                {1, @"\((.*?)\)" },
                {2, @"\<(.*?)\>" },
            };

            var dataType = DataType.Unknow;

            foreach (var pattern in patterns)
            {
                var param = Regex.Match(typeValue, pattern.Value);
                if (param.Success)
                {
                    refParam = param.Value;
                    refChunk = refParam.Substring(1, param.Value.Length - 2);

                    bool check = DataTypeMap.TryGetValue(refChunk, out var resultType);
                    dataType = check ? resultType : DataType.Struct;

                    var index = typeValue.IndexOf(DataType.List.ToString());
                    if (index >= 0)
                        dataType |= DataType.List;

                    return dataType;
                }
            }

            var match = Regex.Match(typeValue, @"\[(.*?)\]");
            if (match.Success)
            {
                dataType = DataType.Array;

                var typeMatch = Regex.Match(typeValue, @"^\w+");
                if (typeMatch.Success)
                {
                    refParam = typeMatch.Value.ToString();
                    refChunk = typeMatch.Value.ToString();

                    bool check = DataTypeMap.TryGetValue(typeMatch.Value.ToString(), out var resultType);
                    dataType |= check ? resultType : DataType.Struct;

                    return dataType;
                }
            }

            if (typeValue == DataType.Enum.ToString().ToLower())
            {
                refParam = typeValue;
                refChunk = typeValue;
                dataType = DataType.Enum;
            }
            else
            {
                refParam = typeValue;
                refChunk = typeValue;

                if (DataTypeMap.TryGetValue(refChunk.Trim(), out var outType))
                    dataType = outType;
            }

            return dataType;
        }

        /// <summary>
        /// try parse ref params, only support list of int, float, double, short, long, string and struct,
        /// and the ref params should be like "int:id,string:name", only for csharp code
        /// </summary>
        /// <param name="refChunks"></param>
        /// <param name="refParams"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        public static ErrorCode TryParseRefParams(string refChunks, out List<RefField> refParams, string desc = "")
        {
            refParams = new List<RefField>();

            string[] chunks = refChunks.Split(',');
            foreach (var chunk in chunks)
            {
                string[] splits = chunk.Split(':');
                if (splits.Length == 2)
                {
                    refParams.Add(new RefField()
                    {
                        type = splits[0].Trim(),
                        name = splits[1].Trim(),
                        desc = desc
                    });
                }
            }

            if (refParams.Count != 0)
                return ErrorCode.Succeed;

            return ErrorCode.ParseParamsError;
        }

        /// <summary>
        /// hash string to int, only support string with length less than 100, and the hash code is used for json and javascript code
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int HashString(string str)
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
    }

    /// <summary>
    /// contract resolver for escape string, only support string property, and the string value will escape " to \", and the contract resolver is used for json code
    /// </summary>
    public class EscapeStringContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// create json property, only support string property, and the string value will escape " to \"
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (prop.PropertyType == typeof(string))
            {
                var oldValueProvider = prop.ValueProvider;
                if (oldValueProvider != null)
                {
                    prop.ValueProvider = new EscapeStringValueProvider(oldValueProvider);
                }
            }

            return prop;
        }

        /// <summary>
        /// value provider for escape string, only support string property, and the string value will escape " to \"
        /// </summary>
        class EscapeStringValueProvider : IValueProvider
        {
            private readonly IValueProvider _inner;
            public EscapeStringValueProvider(IValueProvider inner) => _inner = inner;

            public object? GetValue(object target)
            {
                var value = _inner.GetValue(target) as string;
                return value?.Replace("\"", "\\\"");
            }

            public void SetValue(object target, object? value) => _inner.SetValue(target, value);
        }
    }
}
