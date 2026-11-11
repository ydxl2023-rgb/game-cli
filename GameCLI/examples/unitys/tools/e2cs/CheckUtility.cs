using System.Text.RegularExpressions;

namespace e2cs
{
    public static class CheckUtility
    {
        static Dictionary<string, bool> useds = new Dictionary<string, bool>();

        /// <summary>
        /// add used name
        /// </summary>
        /// <param name="name"></param>
        public static void AddUsed(string name)
        {
            if (!useds.ContainsKey(name))
            {
                useds.Add(name, true);
            }
        }

        /// <summary>
        /// del used name
        /// </summary>
        /// <param name="name"></param>
        public static void DelUsed(string name)
        {
            if (useds.ContainsKey(name))
            {
                useds.Remove(name);
            }
        }

        /// <summary>
        /// is used name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsUsed(string name)
        {
            return useds.ContainsKey(name);
        }

        /// <summary>
        /// is valid file path
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsValidFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            bool hasPathSeparators = path.Contains('\\') || path.Contains('/');
            bool hasDriveLetter = Regex.IsMatch(path, @"^[a-zA-Z]:\\", 
                RegexOptions.Compiled);
            
            bool hasFileExtension = Regex.IsMatch(path, 
                @"\.[a-zA-Z0-9]{1,10}$", RegexOptions.Compiled);
            bool hasDirectoryPattern = path.Contains("..") || path.Contains("./") || path.Contains(".\\");

            return hasPathSeparators || hasDriveLetter || hasFileExtension || hasDirectoryPattern;
        }
    }

    /// <summary>
    /// path extensions
    /// </summary>
    public static class PathExtensions
    {
        /// <summary>
        /// is valid file path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidFilePath(this string path)
        {
            return CheckUtility.IsValidFilePath(path);
        }
    }
}
