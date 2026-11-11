using System.Text;
using Newtonsoft.Json;

namespace e2cs
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var v = Newtonsoft.Json.JsonConvert.DefaultSettings;

            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Please input -i -o -d -f -l -n");
                    return;
                }

                Context context = new Context() { 
                    InputDirectory = "", OutputDirectory = "", Language = "propLanguage.xlsx", Title = "Data", OutDll = "Config.dll", Namespace="Config", EnumFileName="ExcelEnumDefine"
                };
            
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    switch (arg)
                    {
                        case "-i":
                            context.InputDirectory = args[i + 1];
                            break;

                        case "-o":
                            context.OutputDirectory = args[i + 1];
                            break;

                        case "-d":
                            context.OutDll = args[i + 1];
                            break;

                        case "-f":
                            context.Title = args[i + 1];
                            break;

                        case "-l":
                            context.Language = args[i + 1];
                            break;

                        case "-n":
                            context.Namespace = args[i + 1];
                            break;
                    }
                }

                string lgpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory, 
                    OutputType.CSharp.ToString().ToLower(), DirectoryType.Language.ToString().ToLower());
                if (Directory.Exists(lgpath))
                    Directory.Delete(lgpath, true);

                Directory.CreateDirectory(lgpath);

                string jspath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                    DirectoryType.Jscode.ToString().ToLower());
                if (Directory.Exists(jspath))
                    Directory.Delete(jspath, true);

                Directory.CreateDirectory(jspath);

                string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, context.OutputDirectory,
                    DirectoryType.Json.ToString().ToLower());
                if (Directory.Exists(jsonPath))
                    Directory.Delete(jsonPath, true);

                Directory.CreateDirectory(jsonPath);

                Utility.DoExports(context);
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
