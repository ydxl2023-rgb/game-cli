using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
using UnityCLI.Abstractions;

namespace UnityCLI;

/// <summary>Creates XLSX workbooks and edits configuration tables through the Editor bridge.</summary>
public sealed class ExcelPlugin : IPlugin
{
    public string Name => "ExcelPlugin";
    public IReadOnlyCollection<string> Commands => new[] { "excel" };
    public string Help => """
        excel create --file path.xlsx --headers-json '["Id","Name"]' [--sheet Sheet1]
        excel delete-file --file path.xlsx
        excel list [--root folder]
        excel schema|query|row|cell|validate --file path.xlsx [options]
        excel set --file path.xlsx --row 2 --field Name --value text
        excel insert --file path.xlsx --values-json '{"Id":1,"Name":"Example"}'
        excel delete --file path.xlsx --row 2
        Select: --sheet name --header-row N --row N|--index N|--key-field Id --key value
        Query: --where expression --count N; cell/set: --field name|--col A
        Bridge file lookup: --file path.xlsx or --table name [--root folder]
        Common: --project UnityRoot --json --dry-run
        create/delete-file use paths relative to --project (otherwise current directory).
        create refuses existing files; delete removes rows, delete-file removes the workbook.
        """;

    /// <summary>Parses workbook commands and executes local file or Editor operations.</summary>
    /// <param name="command">Registered command name.</param>
    /// <param name="args">Action and command options.</param>
    /// <returns>Zero for success, one for execution errors, two for invalid arguments.</returns>
    public async Task<int> ExecuteAsync(string command, string[] args)
    {
        bool json = args.Contains("--json");
        try
        {
            if (args.Contains("--help") || args.Contains("-h")) { Console.WriteLine(Help); return 0; }
            if (args.Length == 0 || !new[] { "create", "delete-file", "list", "schema", "row", "cell", "query", "set", "insert", "delete", "validate" }.Contains(args[0]))
                throw new ArgumentException(Help);
            var options = new Dictionary<string, string>();
            bool dryRun = false;
            var names = new Dictionary<string, string> {
                ["--file"]="excelFilePath", ["--sheet"]="excelSheet", ["--table"]="excelTable", ["--root"]="excelRoot",
                ["--col"]="excelColumn", ["--field"]="excelField", ["--key"]="excelKey", ["--key-field"]="excelKeyField",
                ["--where"]="excelWhere", ["--values-json"]="excelValuesJson", ["--value"]="value",
                ["--row"]="excelRow", ["--index"]="excelIndex", ["--header-row"]="excelHeaderRow", ["--count"]="count" };
            for (int i=1;i<args.Length;i++) {
                string option=args[i];
                if (option=="--json") continue;
                if (option=="--dry-run") { dryRun=true; continue; }
                if (!names.ContainsKey(option) && option is not ("--project" or "--headers-json")) throw new ArgumentException("Unknown option: "+option);
                if (++i>=args.Length || args[i].StartsWith("--")) throw new ArgumentException("Missing value: "+option);
                if (!options.TryAdd(option,args[i])) throw new ArgumentException("Duplicate option: "+option);
            }
            options.TryGetValue("--project",out var project);
            if (args[0] is "create" or "delete-file") {
                if(options.Keys.Any(k=>k is not ("--project" or "--file" or "--sheet" or "--headers-json"))) throw new ArgumentException("Unsupported workbook option.");
                if(!options.TryGetValue("--file",out var file))throw new ArgumentException("--file is required.");
                file=Path.GetFullPath(file,project==null?Directory.GetCurrentDirectory():Path.GetFullPath(project));
                if(!file.EndsWith(".xlsx",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Only .xlsx workbooks are supported.");
                if(args[0]=="delete-file") {
                    if(options.ContainsKey("--sheet")||options.ContainsKey("--headers-json"))throw new ArgumentException("delete-file removes the entire workbook; sheet options are not allowed.");
                    if(!File.Exists(file))throw new FileNotFoundException("Workbook not found.",file);
                    if(!dryRun)File.Delete(file);
                } else {
                    if(File.Exists(file))throw new IOException("Workbook already exists; refusing to overwrite.");
                    string sheet=options.GetValueOrDefault("--sheet","Sheet1");
                    if(string.IsNullOrWhiteSpace(sheet)||sheet.Length>31||sheet.IndexOfAny("[]:*?/\\".ToCharArray())>=0||sheet.StartsWith("'")||sheet.EndsWith("'"))throw new ArgumentException("Invalid worksheet name.");
                    if(!options.TryGetValue("--headers-json",out var headersJson))throw new ArgumentException("--headers-json is required.");
                    string[] headers;
                    try { headers=JsonSerializer.Deserialize<string[]>(headersJson)??Array.Empty<string>(); }
                    catch(JsonException e){throw new ArgumentException("Expected a JSON array of header names.",e);}
                    if(headers.Length==0||headers.Length>16384||headers.Any(string.IsNullOrWhiteSpace)||headers.Distinct(StringComparer.OrdinalIgnoreCase).Count()!=headers.Length)throw new ArgumentException("Headers must be nonempty and unique (maximum 16384).");
                    if(!dryRun)CreateWorkbook(file,sheet,headers);
                }
                Console.WriteLine(JsonSerializer.Serialize(new {ok=true,action=args[0],file,dryRun}));return 0;
            }
            if(options.ContainsKey("--headers-json"))throw new ArgumentException("--headers-json requires create.");
            var payload=new Dictionary<string,object>{{"action",args[0]},{"dryRun",dryRun},{"count",20}};
            foreach(var pair in options.Where(p=>p.Key!="--project")) {
                string key=names[pair.Key];
                if(pair.Key is "--row" or "--index" or "--header-row" or "--count") {
                    if(!int.TryParse(pair.Value,out int number)||number<(pair.Key=="--index"?0:1))throw new ArgumentException("Invalid number: "+pair.Key);
                    payload[key]=number;if(key!="count")payload[key+"Set"]=true;
                } else payload[key]=pair.Value;
            }
            var response=await CLIClient.SendAsync(project,"/excel",payload,"POST");
            return ResponseFormatter.WriteJsonOrHuman(response,json,root=>{
                if(!root.GetProperty("ok").GetBoolean()){ResponseFormatter.WriteError(root);return;}
                Console.WriteLine(JsonSerializer.Serialize(root,new JsonSerializerOptions{WriteIndented=true}));
            });
        }
        catch(Exception e){if(json)Console.WriteLine(JsonSerializer.Serialize(new{ok=false,error=new{message=e.Message}}));else Console.Error.WriteLine(e.Message);return e is ArgumentException?2:1;}
    }

    /// <summary>Writes a minimal standards-based XLSX package, then atomically publishes it.</summary>
    /// <param name="file">New workbook path.</param>
    /// <param name="sheet">Worksheet name.</param>
    /// <param name="headers">First-row column names.</param>
    private static void CreateWorkbook(string file,string sheet,string[] headers)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        string temporary=file+"."+Guid.NewGuid().ToString("N")+".tmp";
        try {
            using(var archive=ZipFile.Open(temporary,ZipArchiveMode.Create)) {
                XNamespace main="http://schemas.openxmlformats.org/spreadsheetml/2006/main",rel="http://schemas.openxmlformats.org/officeDocument/2006/relationships",pkg="http://schemas.openxmlformats.org/package/2006/relationships",ct="http://schemas.openxmlformats.org/package/2006/content-types";
                void Write(string path,XElement xml){using var stream=archive.CreateEntry(path).Open();xml.Save(stream);}
                Write("[Content_Types].xml",new XElement(ct+"Types",new XElement(ct+"Default",new XAttribute("Extension","rels"),new XAttribute("ContentType","application/vnd.openxmlformats-package.relationships+xml")),new XElement(ct+"Default",new XAttribute("Extension","xml"),new XAttribute("ContentType","application/xml")),new XElement(ct+"Override",new XAttribute("PartName","/xl/workbook.xml"),new XAttribute("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),new XElement(ct+"Override",new XAttribute("PartName","/xl/worksheets/sheet1.xml"),new XAttribute("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"))));
                XElement Relationship(string type,string target)=>new(pkg+"Relationships",new XElement(pkg+"Relationship",new XAttribute("Id","rId1"),new XAttribute("Type",rel.NamespaceName+type),new XAttribute("Target",target)));
                Write("_rels/.rels",Relationship("/officeDocument","xl/workbook.xml"));
                Write("xl/_rels/workbook.xml.rels",Relationship("/worksheet","worksheets/sheet1.xml"));
                Write("xl/workbook.xml",new XElement(main+"workbook",new XElement(main+"sheets",new XElement(main+"sheet",new XAttribute("name",sheet),new XAttribute("sheetId",1),new XAttribute(rel+"id","rId1")))));
                Write("xl/worksheets/sheet1.xml",new XElement(main+"worksheet",new XElement(main+"sheetData",new XElement(main+"row",new XAttribute("r",1),headers.Select((h,i)=>new XElement(main+"c",new XAttribute("r",Column(i+1)+"1"),new XAttribute("t","inlineStr"),new XElement(main+"is",new XElement(main+"t",h))))))));
            }
            File.Move(temporary,file);
        } finally {if(File.Exists(temporary))File.Delete(temporary);}
    }

    /// <summary>Converts a one-based column index to an Excel column name.</summary>
    private static string Column(int index){string result="";while(index>0){index--;result=(char)('A'+index%26)+result;index/=26;}return result;}
}
