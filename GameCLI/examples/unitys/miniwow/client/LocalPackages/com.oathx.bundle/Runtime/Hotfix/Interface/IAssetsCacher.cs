using System;

namespace Oathx.Bundle.Assets
{
    /// <summary>
    /// assets cacher
    /// </summary>
    public interface IAssetsCacher
    {
        /// <summary>
        /// find cacher asset
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        Object Find(string name);
        /// <summary>
        /// add asset to cacher
        /// </summary>
        /// <param name="name"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        bool Add(string name, Object asset);
        /// <summary>
        /// remove cacher asset
        /// </summary>
        /// <param name="name"></param>
        void Remove(string name);
        /// <summary>
        /// clear all cacher assets
        /// </summary>
        void Clear();
    }
}
