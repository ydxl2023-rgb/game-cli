
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Config 
{
    #pragma warning disable IDE0090
    #pragma warning disable IDE0066

    public interface IConfigData
    {
        string GetName();
        object GetData();
    }

    /// <summary>
    /// 
    /// </summary>
    public class Config<T> : IConfigData
    {
        /// <summary>
        ///  field name
        /// </summary>
        public string name
        {get; set;}
        
        /// <summary>
        /// all datas
        /// </summary>
        public Dictionary<int, T> data
        { get; set; }

        /// <summary>
        /// get field name
        /// </summary>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// get config data
        /// </summary>
        public object GetData()
        {
            return data;
        }
    }

    public partial class Excel
    {
    
    }

    #pragma warning restore IDE0066
    #pragma warning restore IDE0090
}
