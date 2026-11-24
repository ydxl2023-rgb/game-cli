namespace UnityCLI
{
    /// <summary>Report screen-space bounds of active list items and their optional child.</summary>
    internal sealed class MeasureCommand : ICommand
    {
        public string Name => "measure";

        public async Task<int> ExecuteAsync(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ObjectPath))
            {
                Console.Error.WriteLine("measure requires --path <container>.");
                return 1;
            }

            var json = await CLIClient.SendAsync(options.ProjectPath, "/measure", new
            {
                objectPath = options.ObjectPath,
                search = options.Search,
                childName = options.ChildName
            });
            return ResponseFormatter.WriteJsonOrHuman(json, options.JsonOutput, root => Console.WriteLine(root.ToString()));
        }
    }
}
