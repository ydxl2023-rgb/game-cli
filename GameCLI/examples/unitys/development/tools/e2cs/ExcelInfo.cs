using System.IO;
using System.Linq;
using System.Text;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace e2cs
{
#pragma warning disable IDE0063
#pragma warning disable IDE1006
    /// <summary>
    /// 数据字段信息
    /// </summary>
    public class RefField
    {
        /// <summary>
        /// 字段类型
        /// </summary>
        public required string type;

        /// <summary>
        /// 字段名称
        /// </summary>
        public required string name;

        /// <summary>
        /// 字段描述
        /// </summary>
        public required string desc;
    }

    /// <summary>
    /// 数据格信息
    /// </summary>
    public class RefDataCell
    {
        public ICell? name;
        public ICell? type;
        public ICell? data;
    }

    /// <summary>
    /// 数据行信息
    /// </summary>
    public class RefDataInfo
    {
        /// <summary>
        /// 数据格列表
        /// </summary>
        public required List<RefDataCell> cells
        { get; set; }

        /// <summary>
        /// 数字ID
        /// </summary>
        public required int numberId
        { get; set; }
    }

    /// <summary>
    /// 引用类型来源
    /// </summary>
    public enum RefTypeSrc
    {
        Dependencies,
        Main,
    }

    /// <summary>
    /// 结构体字段信息
    /// </summary>
    public class StructField
    {
        public required string type;
        public required string name;
    }

    /// <summary>
    /// Excel表中产生的导出类型信息
    /// </summary>
    public class RefTypeInfo
    {
        /// <summary>
        /// 引用类型名
        /// </summary>
        public required string name
        { get; set; }

        /// <summary>
        /// 引用类型描述
        /// </summary>
        public required string desc
        { get; set; }

        /// <summary>
        /// 引用类型中的全部字段
        /// </summary>
        public required List<RefField> fields
        { get; set; }

        /// <summary>
        /// 引用类型来源
        /// </summary>
        public RefTypeSrc source
        { get; set; }
    }

    /// <summary>
    /// 枚举类型信息
    /// </summary>
    public class RefEnumInfo
    {
        /// <summary>
        /// 枚举类型名
        /// </summary>
        public required string name
        { get; set; }

        /// <summary>
        /// 枚举类型描述
        /// </summary>
        public required string desc
        { get; set; }

        /// <summary>
        /// 枚举值列表
        /// </summary>
        public required Dictionary<string, int> enums
        { get; set; }
    }

    /// <summary>
    /// 数据类型
    /// </summary>
    public enum DataType
    {
        Unknow = 0,
        Int = 1,
        String = 2,
        Struct = 4,
        List = 8,
        Array = 16,
        Bool = 32,
        Float = 64,
        Double = 128,
        Long = 256,
        Ref = 512,
        Enum = 1024,
    }

    /// <summary>
    /// 错误码
    /// </summary>
    public enum ErrorCode
    {
        Undefine,
        Succeed,
        Failure,
        ParseParamsError
    }

    /// <summary>
    /// Excel表中产生的导出表格信息
    /// </summary>
    public class ExcelSheetInfo
    {
        /// <summary>
        /// 表格对象
        /// </summary>
        public required ISheet sheet
        { get; set; }

        /// <summary>
        /// 常量定义列表
        /// </summary>
        public required Dictionary<string, int> consts
        { get; set; }

        /// <summary>
        /// 引用类型列表
        /// </summary>
        public required List<RefTypeInfo> refTypeInfos
        {  get; set; }

        /// <summary>
        /// 数据行信息列表
        /// </summary>
        public required List<RefDataInfo> refDataInfos
        { get; set; }

        /// <summary>
        /// 重写ToString方法，输出表格信息
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach(var c in consts)
            {
                sb.AppendLine($"\t{c.Key}={c.Value}");
            }

            foreach (var r in refTypeInfos)
            {
                sb.AppendLine($"{r.name}");
                foreach(var f in r.fields)
                {
                    sb.AppendLine($"\t{f.type} {f.name} {f.desc}");
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Excel全局类型信息
    /// </summary>
    public class ExcelGlobalTypeUtils
    {
        public static Dictionary<string, RefEnumInfo> refEnumInfos = new Dictionary<string, RefEnumInfo>();

        /// <summary>
        /// 添加枚举类型
        /// </summary>
        /// <param name="info"></param>
        public static void AddEnumType( RefEnumInfo info )
        {
            if (!refEnumInfos.ContainsKey(info.name))
                refEnumInfos.Add(info.name, info);
        }
    }

    /// <summary>
    /// Excel表格信息
    /// </summary>
    public class ExcelInfo
    {
        /// <summary>
        /// 表格信息列表
        /// </summary>
        public readonly List<ExcelSheetInfo> sheetInfos = new List<ExcelSheetInfo>();

        /// <summary>
        /// 加载Excel文件
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        /// <exception cref="FieldAccessException"></exception>
        public virtual bool Load(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    fileStream.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    XSSFWorkbook wk = new XSSFWorkbook(memoryStream);
                    if (wk.NumberOfSheets <= 0)
                        throw new FieldAccessException($"You cannot export an empty table {filename}");

                    var pathName = Path.GetFileName(filename);
                    Console.WriteLine(pathName);

                    if (ReadWorkbookRefTypeDefines(wk, pathName))
                        Dump();

                    wk.Close();
                }
            }

            return true;
        }

        /// <summary>
        /// 输出表格信息
        /// </summary>
        public virtual void Dump()
        {
            foreach (var sheet in sheetInfos)
            {
               Console.WriteLine(sheet.ToString());
            }
        }

        /// <summary>
        /// 读取工作簿中的引用类型定义
        /// </summary>
        /// <param name="wk"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public virtual bool ReadWorkbookRefTypeDefines(XSSFWorkbook wk, string fileName)
        {
            for (int sheetIndex = 0; sheetIndex < wk.NumberOfSheets; sheetIndex++)
            {
                var sheet = wk.GetSheetAt(sheetIndex);
                if (sheet != null)
                {
                    List<RefTypeInfo> refTypeInfos = new List<RefTypeInfo>();
                    List<RefField> typeFields = new List<RefField>();
                 
                    var header = ReadSheetHeader(sheet);
                    for(int i=0; i<header.fieldTypes.Count; i++)
                    {
                        var descCell = header.fieldDescs[i];
                        var nameCell = header.fieldNames[i];
                        var typeCell = header.fieldTypes[i];

                        if (descCell != null && nameCell != null && typeCell != null)
                        {
                            string typeTextFiled = typeCell.StringCellValue;
                            if (!string.IsNullOrEmpty(typeTextFiled))
                            {
                                var dataType = Utility.TryParseDataType(typeTextFiled, out string refParamChunks, out string refChunks);
                                if (dataType.HasFlag(DataType.Struct))
                                {
                                    var result = Utility.TryParseRefParams(refChunks, out var refFields, descCell.StringCellValue);
                                    if (result == ErrorCode.Succeed)
                                    {
                                        string refParamTypeName = sheet.SheetName + nameCell.StringCellValue;
                                        refTypeInfos.Add(new RefTypeInfo()
                                        {
                                            desc = ErrorCode.Undefine.ToString(),
                                            name = refParamTypeName,
                                            fields = refFields,
                                            source = RefTypeSrc.Dependencies
                                        });

                                        if (dataType.HasFlag(DataType.List))
                                        {
                                            typeTextFiled = string.Format("List<{0}>", refParamTypeName);
                                        }
                                        else if (dataType.HasFlag(DataType.Array))
                                        {
                                            typeTextFiled = string.Format("{0}[]", refParamTypeName);
                                        }
                                        else
                                        {
                                            typeTextFiled = refParamTypeName;
                                        }
                                    }
                                }
                                else if (dataType.HasFlag(DataType.Enum))
                                {
                                    typeTextFiled = Utility.ConvertToEnum(nameCell.StringCellValue);
                                }
                           
                                typeFields.Add(new RefField()
                                {
                                    desc = descCell.StringCellValue,
                                    name = nameCell.StringCellValue,
                                    type = typeTextFiled
                                });
                            }
                        }
                    }

                    refTypeInfos.Add(new RefTypeInfo() {
                        desc = sheet.SheetName, name = sheet.SheetName, fields = typeFields, source = RefTypeSrc.Main
                    });

                    Dictionary<string, int> constDefines = new Dictionary<string, int>();
                    if (!TryParseConsts(sheet, ref constDefines, out var alias))
                        return false;

                    if (TryParseEnums(sheet, header.fieldDescs, header.fieldNames, header.fieldTypes, out var refEnumInfos))
                    {
                        foreach(var enumInfo in refEnumInfos)
                        {
                            ExcelGlobalTypeUtils.AddEnumType(enumInfo);
                        }
                    }

                    if (TryParseRowDataInfos(sheet, header.fieldNames, header.fieldTypes, alias, out var refDataInfos))
                    {
                        sheetInfos.Add(new ExcelSheetInfo() {
                            consts=constDefines, refDataInfos=refDataInfos, refTypeInfos=refTypeInfos, sheet=sheet
                        });
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 默认枚举值
        /// </summary>
        public static class DefaultEnum
        {
            public const string None = "None";
        }

        /// <summary>
        /// 解析枚举类型
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="descs"></param>
        /// <param name="names"></param>
        /// <param name="types"></param>
        /// <param name="enumInfos"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public bool TryParseEnums(ISheet sheet, List<ICell> descs,  List<ICell> names,  List<ICell> types, out List<RefEnumInfo> enumInfos)
        {
            enumInfos = new List<RefEnumInfo>();

            for(int index=0; index<types.Count; index++)
            {
                ICell cellType = types[index];

                if (cellType.StringCellValue.Equals(DataType.Enum.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    RefEnumInfo enumInfo = new RefEnumInfo() { 
                        desc= descs[index].StringCellValue, name= Utility.ConvertToEnum(names[index].StringCellValue), enums = new Dictionary<string, int>() 
                    };

                    ICell enumCell = types[index];
                    bool supportXOR = false;

                    List<string> enums = new List<string>();
                    for (int row = 3; row <= sheet.LastRowNum; row++)
                    {
                        IRow rowInfo = sheet.GetRow(row);
                        if (rowInfo != null)
                        {
                            ICell cell = rowInfo.GetCell(enumCell.ColumnIndex);
                            if (cell != null)
                            {
                                if (cell.CellType != CellType.Blank)
                                {
                                    if (cell.CellType == CellType.String)
                                    {
                                        if (!string.IsNullOrEmpty(cell.StringCellValue))
                                        {
                                            var value = cell.StringCellValue;
                                            if (!string.IsNullOrEmpty(value))
                                            {
                                                string[] splits = value.Split('|');
                                                foreach (var name in splits)
                                                {
                                                    if (!enums.Contains(name))
                                                        enums.Add(name);
                                                }

                                                supportXOR = splits.Length > 0;
                                            }
                                        }
                                    }
                                    else if (cell.CellType == CellType.Numeric)
                                    {
                                        throw new NotSupportedException("Enum type must be string");
                                    }
                                }
                            }
                        }
                    }

                    enums.Sort();

                    if (!enums.Contains(DefaultEnum.None))
                        enums.Insert(0, DefaultEnum.None);

                    Dictionary<string, int> enumMap = new Dictionary<string, int>();
                    if (supportXOR)
                    {
                        for (int i = 0; i < enums.Count; i++)
                        {
                            var value = i <= 2 ? i : 2 << (i - 2);
                            enumMap.Add(enums[i], value);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < enums.Count; i++)
                        {
                            enumMap.Add(enums[i], i);
                        }
                    }

                    enumInfo.enums = enumMap;

                    enumInfos.Add(enumInfo);
                }
            }
            
            return enumInfos.Count != 0;
        }

        /// <summary>
        /// 解析数据行信息
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="names"></param>
        /// <param name="types"></param>
        /// <param name="alias"></param>
        /// <param name="dataInfos"></param>
        /// <returns></returns>
        public bool TryParseRowDataInfos(ISheet sheet, List<ICell> names, List<ICell> types, bool alias, out List<RefDataInfo> dataInfos)
        {
            dataInfos = new List<RefDataInfo>();
            for (int row = 3; row <= sheet.LastRowNum; row++)
            {
                var rowInfo = sheet.GetRow(row);
                if (rowInfo != null)
                {
                    var idCell = rowInfo.GetCell(alias ? 1 : 0);
                    if (idCell != null && idCell.CellType != CellType.Blank)
                    {
                        var dataCells = new List<RefDataCell>();
                        for (int col = 0; col < rowInfo.LastCellNum; col++)
                        {
                            var dataCell = rowInfo.GetCell(col);
                            dataCells.Add(new RefDataCell() {
                                data = dataCell, name = names[col], type = types[col]
                            });
                        }
                        
                        if (idCell.CellType == CellType.Formula)
                        {
                            Console.WriteLine($"Warning: The cell alias {alias} row {idCell.RowIndex} col {idCell.ColumnIndex}" +
                                $" in sheet {sheet.SheetName} is formula {idCell.CellFormula} it will be ignore.");
                        }
                            
                        dataInfos.Add(new RefDataInfo() { cells = dataCells, numberId = (int)idCell.NumericCellValue });
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 解析常量定义
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="constDefines"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public bool TryParseConsts(ISheet sheet, ref Dictionary<string, int> constDefines, out bool alias)
        {
            alias = false;

            for (int row = 3; row <= sheet.LastRowNum; row++)
            {
                var dataRow = sheet.GetRow(row);
                if (dataRow != null)
                {
                    ICell nameCell = dataRow.GetCell(0);
                    if (nameCell != null && (nameCell.CellType == CellType.String || nameCell.CellType == CellType.Formula))
                    {
                        ICell idCell = dataRow.GetCell(1);
                        if (idCell != null && idCell.CellType == CellType.Numeric)
                        {
                            if (!constDefines.ContainsKey(nameCell.StringCellValue))
                                constDefines.Add(nameCell.StringCellValue, (int)idCell.NumericCellValue);
                        }
                            
                        alias = true;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 读取表头信息，表头包含字段描述、字段名称、字段类型
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns></returns>
        public (List<ICell> fieldDescs, List<ICell> fieldNames, List<ICell> fieldTypes) ReadSheetHeader(ISheet sheet)
        {
            List<ICell> fieldDescs = new List<ICell>();
            List<ICell> fieldNames = new List<ICell>();
            List<ICell> fieldTypes = new List<ICell>();

            for (var row = 0; row < 3; row++)
            {
                var dataRow = sheet.GetRow(row);
                switch (row)
                {
                    case 0:
                        for (var col = 0; col < dataRow.Cells.Count; col++)
                        {
                            ICell cell = dataRow.Cells[col];
                            fieldDescs.Add(cell);
                        }
                        break;

                    case 1:
                        for (var col = 0; col < dataRow.Cells.Count; col++)
                        {
                            ICell cell = dataRow.Cells[col];
                            fieldNames.Add(cell);
                        }
                        break;

                    case 2:
                        for (var col = 0; col < dataRow.Cells.Count; col++)
                        {
                            ICell cell = dataRow.Cells[col];
                            fieldTypes.Add(cell);
                        }
                        break;
                }
            }

            return (fieldDescs, fieldNames, fieldTypes);
        }

    }
#pragma warning restore IDE1006
#pragma warning restore IDE0063
}
