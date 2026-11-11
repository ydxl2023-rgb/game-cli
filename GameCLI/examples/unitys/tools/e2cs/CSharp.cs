using System.Text;

namespace e2cs
{
#pragma warning disable IDE0290
    public class CSharp : Export
    {
        /// <summary>
        /// excel info for export, if excel info is null, not generate code, if excel info is not null, generate code for each sheet in excel info,
        /// generate const define code, ref type define code and main class code for each sheet, main class code is used for excel data, 
        /// ref type define code is used for ref type define, const define code is used for const define
        /// </summary>
        protected ExcelInfo? excel;

        /// <summary>
        /// constructor for csharp export, if excelInfo is null, not generate code, 
        /// if excelInfo is not null, generate code for each sheet in excelInfo, generate const define code, 
        /// ref type define code and main class code for each sheet, main class code is used for excel data, 
        /// ref type define code is used for ref type define, const define code is used for const define
        /// </summary>
        /// <param name="excelInfo"></param>
        public CSharp(ExcelInfo? excelInfo)
        {
            excel = excelInfo;
        }

        /// <summary>
        /// export excel data to csharp code, generate const define code,
        /// ref type define code and main class code for each excel sheet, main class code is used for excel data, 
        /// ref type define code is used for ref type define, const define code is used for const define
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exportFileInfos"></param>
        /// <returns></returns>
        public bool Run(Context context, out List<ExportFileInfo> exportFileInfos)
        {
            exportFileInfos = new List<ExportFileInfo>();
            if (excel != null)
            {
                string cspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                    OutputType.CSharp.ToString().ToLower(), DirectoryType.Client.ToString().ToLower());
                if (!Directory.Exists(cspath))
                    Directory.CreateDirectory(cspath);

                foreach (var sheet in excel.sheetInfos)
                {
                    var code = GenerateCSharpClass(context, sheet);
                    if (!string.IsNullOrEmpty(code))
                    {
                        var name = sheet.sheet.SheetName + context.Title;
                        if (!string.IsNullOrEmpty(context.OutputDirectory))
                        {
                            string file = Path.Combine(cspath, name);
                            if (!Path.HasExtension(Extension.cs.ToString()))
                                file = Path.ChangeExtension(file, Extension.cs.ToString());

                            File.WriteAllText(file, code);
                        }

                        exportFileInfos.Add(new ExportFileInfo() {
                            code=code, name=name, sheet=sheet, file=name 
                        });
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// generate csharp enum code for excel enum define, only generate enum code, not generate main type define code
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GenerateEnums(Context context)
        {
            const string csharpFormat = @"
using System.Collections.Generic;

namespace {0}
{{
    {1}
}}
";

            const string enumData = @"
/// <summary>
/// {0}
/// </summary>
 public enum {1}
{{
    {2}
}}
";
            StringBuilder sb = new StringBuilder();

            var refEnums = ExcelGlobalTypeUtils.refEnumInfos;
            foreach (var enumInfo in refEnums) 
            {
                StringBuilder enumType = new StringBuilder();
                if (enumInfo.Value.enums.Count > 0)
                {
                    StringBuilder enumField = new StringBuilder();
                    foreach(var it in enumInfo.Value.enums)
                    {
                        enumField.AppendLine($"{it.Key} = {it.Value},");
                    }

                    enumType.AppendLine(string.Format(enumData, enumInfo.Value.desc, enumInfo.Value.name, enumField.ToString()));
                }

                sb.AppendLine(enumType.ToString());
            }

            return string.Format(csharpFormat, context.Namespace, sb.ToString());
        }

        /// <summary>
        /// generate csharp class code for excel sheet, generate const define code, 
        /// ref type define code and main class code, main class code is used for excel data, 
        /// ref type define code is used for ref type define, const define code is used for const define
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public string GenerateCSharpClass(Context context, ExcelSheetInfo sheet)
        {
            var constCode = GenerateConstDefine(context, sheet);
            var outClassTypeName = string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var refTypeDefine in sheet.refTypeInfos)
            {
                if (refTypeDefine.source == RefTypeSrc.Dependencies)
                {
                    string refTypeDefineCode = GenerateRefParamTypeDefineCode(context, refTypeDefine);
                    sb.AppendLine(refTypeDefineCode);
                }
                else
                {
                    string refTypeMainCode = GenerateRefMainTypeDefineCode(context, refTypeDefine, out outClassTypeName);
                    sb.AppendLine(refTypeMainCode);
                }
            }

            var sheetName = sheet.sheet.SheetName;
            var fieldName = Utility.ConvertFieldName(outClassTypeName);

            const string excelData = @"
    /// <summary>
    /// export excel {1} data
    /// </summary>
    public sealed partial class Excel
	{{
		{0}

		/// <summary>
		/// config {4} table
		/// </summary>
		public static readonly IReadOnlyDictionary<int, {1}> {3};

		/// <summary>
		/// return excel config data
		/// </summary>
		/// <param name=""nId""></param>
		/// <returns></returns>
		public static {1} prop{1}ById(int nId)
		{{
			{1} data;
			if (!{3}.TryGetValue(nId, out data))
			{{
#if UNITY_EDITOR
				var error = string.Format(""Can't find config {{0}} id {{1}}"", typeof({1}).Name, nId);
				UnityEngine.Debug.LogError(error);
#endif
			}}

			return data;
		}}
	}}
";
            string excelCode = string.Format(excelData,
                               constCode, sheetName, context.Title, fieldName, outClassTypeName);
            sb.AppendLine(excelCode);

            var csharpFormat = @"
using System.Collections.Generic;
using Newtonsoft.Json;

namespace {0}
{{
    {1}
}}
";
            var csharpCode = string.Format(csharpFormat, context.Namespace,
                sb.ToString());

            return csharpCode;
        }

        /// <summary>
        /// generate ref type define code for main type, generate class and main type define code, main type define code is used for excel data, class is used for ref type define
        /// </summary>
        /// <param name="context"></param>
        /// <param name="typeDefine"></param>
        /// <param name="outClassTypeName"></param>
        /// <returns></returns>
        private string GenerateRefMainTypeDefineCode(Context context, RefTypeInfo typeDefine, out string outClassTypeName)
        {
            outClassTypeName = string.Format("{0}{1}", typeDefine.name, context.Title);

            StringBuilder cc = new StringBuilder();
            const string propertyCode = @"		
/// <summary>
/// {0}
/// </summary>
[JsonProperty]
public {1} {2} 
{{get; private set;}}";

            foreach (var field in typeDefine.fields)
            {
                string code = string.Format(propertyCode, field.desc, field.type, field.name);
                cc.AppendLine(code);
            }

            const string classTypeCode = @"
[System.Serializable]
public class {0}
{{
	{1}

    /// <summary>
    /// override bool operator
    /// </summary>
    public static implicit operator bool({0} exists)
    {{
        return exists != null;
    }}
}}

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class {3} : {2}<{0}>
{{

}}";
            return string.Format(classTypeCode,
                typeDefine.name, cc.ToString(), context.Title, outClassTypeName);
        }

        /// <summary>
        /// generate const define code for excel sheet, only generate const define code, not generate main type define code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private string GenerateConstDefine(Context context, ExcelSheetInfo sheet)
        {
            StringBuilder cc = new StringBuilder();

            const string code = @"
public const int {0} = {1};";
            foreach (var c in sheet.consts)
            {
                var line = string.Format(code, c.Key, c.Value);
                cc.AppendLine(line);
            }

            return cc.ToString();
        }

        /// <summary>
        /// generate ref type define code for dependencies, only generate class, not generate main type define code
        /// </summary>
        /// <param name="context"></param>
        /// <param name="typeDefine"></param>
        /// <returns></returns>
        public string GenerateRefParamTypeDefineCode(Context context, RefTypeInfo typeDefine)
        {
            StringBuilder cc = new StringBuilder();

            const string propertyCode = @"		
/// <summary>
/// {0}
/// </summary>
[JsonProperty]
public {1} {2} 
{{get; private set;}}";

            foreach (var field in typeDefine.fields)
            {
                string code = string.Format(propertyCode, field.desc, field.type, field.name);
                cc.AppendLine(code);
            }


            const string paramClassCode = @"	
[System.Serializable]
public class {0}
{{
	{1}
}}";
            return string.Format(paramClassCode, typeDefine.name, cc.ToString());
        }
    }
#pragma warning restore IDE0290
}
