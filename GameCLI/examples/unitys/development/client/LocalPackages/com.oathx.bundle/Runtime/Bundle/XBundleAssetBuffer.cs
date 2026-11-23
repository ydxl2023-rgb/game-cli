using UnityEngine;

namespace Oathx.Bundle.Assets
{
#pragma warning disable IDE1006
    /// <summary>
    /// Asset Bundle Asset Buffer
    /// </summary>
    public class XBundleAssetBuffer : ScriptableObject
    {
        private byte[] buffer;

        /// <summary>
        /// Get bytes
        /// </summary>
        public byte[] bytes
        {
            get { return buffer; }
        }

        /// <summary>
        /// Initialize buffer
        /// </summary>
        /// <param name="length"></param>
        public void Init(int length)
        {
            buffer = new byte[length];
        }

        /// <summary>
        /// Initialize buffer from TextAsset
        /// </summary>
        /// <param name="text"></param>
        public void Init(TextAsset text)
        {
            if (text != null)
                buffer = text.bytes;
        }

        /// <summary>
        /// Initialize buffer from byte array
        /// </summary>
        /// <param name="bytes"></param>
        public void Init(byte[] bytes)
        {
            buffer = bytes;
        }
    }
#pragma warning restore IDE1006
}
