namespace e2cs
{
    /// <summary>
    /// export file info
    /// </summary>
    public class ExportFileInfo
    {
        /// <summary>
        /// export code
        /// </summary>
        public required string code
        { get; set; }

        /// <summary>
        /// sheet info
        /// </summary>
        public ExcelSheetInfo? sheet
        { get; set; }

        /// <summary>
        /// export file name
        /// </summary>
        public required string name
        { get; set; }

        /// <summary>
        /// export file path
        /// </summary>
        public required string file
        { get; set; }
    }

    /// <summary>
    /// export interface
    /// </summary>
    public interface Export
    {
        /// <summary>
        /// run export
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exportFileInfos"></param>
        /// <returns></returns>
        bool Run(Context context, out List<ExportFileInfo> exportFileInfos);
    }
}
