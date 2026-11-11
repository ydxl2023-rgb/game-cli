using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace e2cs
{
    /// <summary>
    /// 
    /// </summary>
    public class Language
    {
        /// <summary>
        /// 语言HASH值
        /// </summary>
        public required int hash;

        /// <summary>
        /// 语言文本
        /// </summary>
        public required string text;

        /// <summary>
        /// 语言描述
        /// </summary>
        public required string desc;

        /// <summary>
        /// 常量字段名
        /// </summary>
        public required string name;
    }


    /// <summary>
    /// Language utils for export language data
    /// </summary>
    public class LanguageUtils
    {
        static readonly Dictionary<string, Language> languages = new Dictionary<string, Language>();

        /// <summary>
        /// 检测是否包含中文
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static bool ContainsChinese(string input)
        {
            return Regex.IsMatch(input, @"[\u4e00-\u9fff]");
        }

        /// <summary>
        /// convert language text, if text contains Chinese, convert to hash value and add to language list, else return text
        /// </summary>
        /// <param name="excelSheet"></param>
        /// <param name="fieldName"></param>
        /// <param name="text"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="langText"></param>
        /// <returns></returns>
        public static string ConvertLanguageText(ExcelSheetInfo excelSheet, string fileName, int numberId, string fieldName, string text,
            int row, int col, int index = 0)
        {
            string result = text;
            if (!string.IsNullOrEmpty(text) && ContainsChinese(text))
            {
                var fileText = Path.GetFileNameWithoutExtension(fileName);
                var descText = string.Format("LG-{0}-{1}-{2}-{3}-{4}-{5}-{6}",
                    fileText, excelSheet.sheet.SheetName, numberId, fieldName, row, col, index);

                var hash = Math.Abs(Utility.HashString(descText.ToLower()));
                if (languages.TryGetValue(text, out var value))
                {
                    result = value.hash.ToString();
                }
                else
                {
                    languages.Add(text, new Language()
                    {
                        name = MakeAlias(excelSheet.sheet.SheetName, fieldName, row, col, index),
                        hash = hash,
                        text = text,
                        desc = descText
                    });

                    result = hash.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// 创建别名
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="fieldName"></param>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string MakeAlias(string sheetName, string fieldName, int row, int col, int index)
        {
            return $"LG{sheetName}{fieldName}{row}{col}{index}";
        }

        /// <summary>
        /// 语言类型
        /// </summary>
        public enum LanguageType
        {
            ID,
            NumID,
            Desc,
            CN,
            EN,
            CNTC,
            JP,
            KR
        }

        static string[] TypeString = ["int", "int", "string", "string", "string", "string", "string", "string"];

        /// <summary>
        /// 生成语言表xlsx文件
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool GenerateLanguageXlsx(Context context)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.InputDirectory);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            XSSFWorkbook wk = new XSSFWorkbook();
            string file = Path.Combine(path, context.Language);
            if (File.Exists(file))
                File.Delete(file);

            var sheet = wk.CreateSheet(typeof(Language).Name);
            if (sheet != null)
            {
                var descRow = sheet.CreateRow(0);
                if (descRow != null)
                {
                    var names = Enum.GetNames(typeof(LanguageType));
                    for (int i = 0; i < names.Length; i++)
                    {
                        var cell = descRow.CreateCell(i);
                        if (cell != null)
                            cell.SetCellValue(names[i]);
                    }
                }

                var nameRow = sheet.CreateRow(1);
                if (nameRow != null)
                {
                    var names = Enum.GetNames(typeof(LanguageType));
                    for (int i = 0; i < names.Length; i++)
                    {
                        var cell = nameRow.CreateCell(i);
                        if (cell != null)
                        {
                            cell.CellStyle = GetCellStyle(wk, 50);
                            cell.SetCellValue(names[i]);
                        }
                    }
                }

                var typeRow = sheet.CreateRow(2);
                if (typeRow != null)
                {
                    var names = Enum.GetNames(typeof(LanguageType));
                    for (int i = 0; i < names.Length; i++)
                    {
                        var cell = typeRow.CreateCell(i);
                        if (cell != null)
                        {
                            cell.CellStyle = GetCellStyle(wk, 10);
                            cell.SetCellValue(TypeString[i]);
                        }
                    }
                }

                int rowIndex = 3;
                foreach (var language in languages)
                {
                    var dataRow = sheet.CreateRow(rowIndex);
                    if (dataRow != null)
                    {
                        var nameCell = dataRow.CreateCell(0);
                        if (nameCell != null)
                        {
                            nameCell.SetCellValue(language.Value.name);
                        }

                        var hashCell = dataRow.CreateCell(1);
                        if (hashCell != null)
                        {
                            hashCell.SetCellValue(language.Value.hash);
                        }

                        var descCell = dataRow.CreateCell(2);
                        if (descCell != null)
                            descCell.SetCellValue(language.Value.desc);

                        var langCell = dataRow.CreateCell(3);
                        if (langCell != null)
                            langCell.SetCellValue(language.Value.text);
                    }

                    rowIndex++;
                }

                var fs = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                if (fs != null)
                {
                    wk.Clear();
                    wk.Write(fs);
                    wk.Close();
                }

                TryExportLangugaeXLSX(context, file);
            }

            return true;
        }

        /// <summary>
        /// 导出语言表xlsx文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        static bool TryExportLangugaeXLSX(Context context, string file)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
            {
                XSSFWorkbook wk = new XSSFWorkbook(stream);
                if (wk.NumberOfSheets <= 0)
                    throw new FieldAccessException($"You cannot export an empty table {file}");

                var pathName = Path.GetFileName(file);
                Console.WriteLine(pathName);

                DoExportLanguage(context, wk, pathName);
             
                wk.Close();
            }

            return true;
        }

        /// <summary>
        /// 导出语言表数据，生成语言表json文件和语言表常量类
        /// </summary>
        /// <param name="context"></param>
        /// <param name="wk"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        static bool DoExportLanguage(Context context, XSSFWorkbook wk, string filename)
        {
            Dictionary<string, List<Language>> languages = new Dictionary<string, List<Language>>();
            Dictionary<string, LanguageConst> consts = new Dictionary<string, LanguageConst>();

            for (int sheetIndex = 0; sheetIndex < wk.NumberOfSheets; sheetIndex++)
            {
                var sheet = wk.GetSheetAt(sheetIndex);
                if (sheet != null)
                {
                    DoReadSheet(sheet, ref languages, ref consts);
                }
            }

            return DoWriteLanguage(context, languages, consts);
        }

        /// <summary>
        /// 读取语言表数据，生成语言列表和常量列表
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="languages"></param>
        /// <param name="consts"></param>
        /// <returns></returns>
        static bool DoReadSheet(ISheet sheet, ref Dictionary<string, List<Language>> languages, ref Dictionary<string, LanguageConst> consts)
        {
            var desc = sheet.GetRow(0);
            var name = sheet.GetRow(1);
            var type = sheet.GetRow(2);

            for(int i=3; i<=sheet.LastRowNum; i++)
            {
                var data = sheet.GetRow(i);
                if (data != null)
                {
                    TryGetLanguage(data, name, ref languages, ref consts);
                }
            }

            return true;
        }

        /// <summary>
        /// 生成语言表json文件和语言表常量类
        /// </summary>
        /// <param name="context"></param>
        /// <param name="languages"></param>
        /// <param name="consts"></param>
        /// <returns></returns>
        static bool DoWriteLanguage(Context context, Dictionary<string, List<Language>> languages, Dictionary<string, LanguageConst> consts)
        {
            const string code = @"
using System;
using System.Collections.Generic;

namespace {0}
{{
    [System.Serializable]
    public class {1} : IConfigData
    {{
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string name
        {{get; set;}}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, string> data
       {{get; set;}}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {{
            return name;
        }}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public object GetData()
        {{
            return data;
        }}
    }}

    /// <summary>
    /// export excel Effect data
    /// </summary>
    public sealed partial class Excel
    {{
        {2}
        /// <summary>
        /// config data table
        /// </summary>
        public static readonly IReadOnlyDictionary<int, string> {3};

        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = ""nId""></param>
        /// <returns></returns>
        public static string propLanguageById(int nId)
        {{
            if (!{3}.TryGetValue(nId, out var text))
            {{
    #if UNITY_EDITOR
			    UnityEngine.Debug.LogError(nId);
    #endif
            }}

            return text;
        }}

        /// <summary>
        /// return excel config data
        /// </summary>
        /// <param name = ""nId""></param>
        /// <returns></returns>
        public static string propLanguageById(int nId, params object[] args)
        {{
            if (!{3}.TryGetValue(nId, out var text))
                throw new NullReferenceException($""Can't find language id {{nId}}"");
                
            if (args == null || args.Length == 0)
                return text;

            return string.Format(text, args);   
        }}
    }}
}}
";
            const string alias = @"
/// <summary>
/// {0}
/// {1}
/// </summary>
public const int {2} = {3};
";

            Int64 index = 0;
            StringBuilder sb = new StringBuilder();
            foreach(var c in consts)
            {
                var line = string.Format(alias, c.Value.desc, c.Value.text, c.Key, c.Value.hash);
                if (index != 0)
                    sb.Append('\t');

                sb.AppendLine(line);

                index++;
            }

            var className = typeof(Language).Name + context.Title;
            var fieldName = Utility.ConvertFieldName(className);

            var csharp = string.Format(code,
                context.Namespace, className, sb.ToString(), fieldName);
            
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory, OutputType.CSharp.ToString().ToLower(), DirectoryType.Language.ToString().ToLower());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string file = Path.Combine(path, className);
            if (!Path.HasExtension(Extension.cs.ToString()))
                file = Path.ChangeExtension(file, Extension.cs.ToString());

            File.WriteAllText(file, csharp);

            var name = Path.GetFileName(file);
            ExportFileInfo fileInfo = new ExportFileInfo()
            {
                code = csharp,
                file = file,
                name = name,
                sheet = null
            };

            List<ExportFileInfo> files = [fileInfo];

            var commonFileInfo = Utility.GetCommonFileInfo(context);
            files.Add(commonFileInfo);

            return Utility.TryLanguageCompilation(context, files.ToArray(), (ms, fullNormalizeFileInfos) => {
                var assembly = Assembly.Load(ms.ToArray());
                if (assembly != null)
                {
                    foreach(var ef in fullNormalizeFileInfos)
                    {
                        if (ef.file.Contains(typeof(Language).Name))
                            File.WriteAllText(ef.file, ef.code);
                    }

                    foreach (var v in languages)
                    {
                        var instance = Utility.CreateInstance(context, assembly,
                            string.Format("{0}{1}", typeof(Language).Name, context.Title));
                        if (TryFull(context, fileInfo, assembly, instance, v.Value))
                        {
                            WriteJson(context, instance);
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 填充语言表数据到生成的语言表类实例中
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <param name="assembly"></param>
        /// <param name="instance"></param>
        /// <param name="languages"></param>
        /// <returns></returns>
        static bool TryFull(Context context, ExportFileInfo fileInfo, Assembly assembly, object instance, List<Language> languages)
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
                    foreach(var language in languages)
                    {
                        func.Invoke(inst, [
                           language.hash, language.text
                       ]);
                    }

                    data.SetValue(instance, inst);

                    var typeName = Utility.ConvertFieldName(instance.GetType().Name);
                    name.SetValue(instance, typeName);
                }
            }

            return true;
        }

        /// <summary>
        /// 将语言表类实例数据写入json文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classInst"></param>
        /// <returns></returns>
        static bool WriteJson(Context context, object classInst)
        {
            return Utility.WriteJson(context, classInst);
        }

        /// <summary>
        /// 获取语言类型名称
        /// </summary>
        /// <param name="name"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        static string GetLanguageName(IRow name, int col) 
        {
            var cell = name.GetCell(col);
            return cell.StringCellValue;
        }

        /// <summary>
        /// 语言表常量类
        /// </summary>
        public class LanguageConst
        {
            public required Int32 hash;
            public required string text;
            public required string desc;
        }

        /// <summary>
        /// 获取语言数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="name"></param>
        /// <param name="languages"></param>
        /// <param name="consts"></param>
        /// <returns></returns>
        static bool TryGetLanguage(IRow data, IRow name, ref Dictionary<string, List<Language>> languages, ref Dictionary<string, LanguageConst> consts)
        {
            var nick = data.GetCell(0);
            var hash = data.GetCell(1);
            var desc = data.GetCell(2);

            for(int col=3; col<data.LastCellNum; col++)
            {
                var type = GetLanguageName(name, col);
                if (!languages.TryGetValue(type, out var language))
                {
                    language = new List<Language>();
                    languages.Add(type, language);
                }

                var text = data.GetCell(col);
                language.Add(new Language() { 
                    desc = desc.StringCellValue, hash = (int)hash.NumericCellValue, name = nick.StringCellValue, text = text.StringCellValue 
                });

                if (!consts.ContainsKey(nick.StringCellValue))
                {
                    string alias = nick.StringCellValue;
                    consts.Add(alias, new LanguageConst() {
                        hash = (int)hash.NumericCellValue, text = text.StringCellValue, desc = desc.StringCellValue
                    });
                }
            }

            return true;
        }

        /// <summary>
        /// 获取单元格样式
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        static ICellStyle GetCellStyle(XSSFWorkbook wk, int color)
        {
            var cellStyle = wk.CreateCellStyle();
            if (color != 0)
                cellStyle.FillForegroundColor = (short)color;

            cellStyle.FillPattern = FillPattern.SolidForeground;
            cellStyle.BorderDiagonalLineStyle = BorderStyle.DashDot;
           
            return cellStyle;
        }
    }
}
