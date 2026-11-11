using System;
namespace e2cs
{
    /// <summary>
    /// context for export
    /// </summary>
    public class Context
    {
        /// <summary>
        /// input directory
        /// </summary>
        public required string InputDirectory
        { get; set; }

        /// <summary>
        /// output directory
        /// </summary>
        public required string OutputDirectory
        { get; set; }

        /// <summary>
        /// output dll name, if empty, not generate dll
        /// </summary>
        public string? OutDll
        { get; set; }

        /// <summary>
        /// title for config, default is "Data", if empty, not generate title
        /// </summary>
        public string? Title
        { get; set; }

        /// <summary>
        /// language file name, default is "propLanguage.xlsx", if empty, not generate language file
        /// </summary>
        public required string Language
        { get; set; }

        /// <summary>
        /// namespace for generated code, default is "Config", if empty, not generate namespace
        /// </summary>
        public required string Namespace
        { get; set; }

        /// <summary>
        /// enum file name, default is "ExcelEnumDefine", if empty, not generate enum file
        /// </summary>
        public required string EnumFileName
        { get; set; }
    }
}
