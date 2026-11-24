using System.IO;

namespace Oathx.Bundle.Assets
{
    public class XHotfixUtility
    {
        static readonly System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        static readonly XBundleCrc32 crc32 = new XBundleCrc32();

        /// <summary>
        /// check file checksum
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static uint ComputeFileChecksum(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            crc32.ComputeHash(fs);
            fs.Close();

            return crc32.GetHashResult();
        }

        /// <summary>
        /// generate md5 number
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Md5Sum(string input)
        {
            return Md5Sum(System.Text.Encoding.ASCII.GetBytes(input));
        }

        /// <summary>
        /// generate md5 number
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string Md5Sum(byte[] bytes)
        {
            byte[] hash = md5.ComputeHash(bytes);
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
