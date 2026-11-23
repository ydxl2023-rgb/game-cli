using System;
using UnityEngine;

namespace Oathx.HttpProtocol
{
    /// <summary>
    /// HTTP Code Definitions
    /// </summary>
    public static class HttpCode
    {
        public const int SUCCEED = 0;
        public const int FAILURE = 1;
        public const int OFFLINE = 2;
    }

    /// <summary>
    /// Host Information
    /// </summary>
    [Serializable]
    public class Host
    {
        /// <summary>
        /// Username
        /// </summary>
        public string username
        { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string password
        { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public string version
        { get; set; }

        /// <summary>
        /// Device
        /// </summary>
        public string device
        { get; set; }

        /// <summary>
        /// Graphics memory size
        /// </summary>
        public long graphicsMemorySize
        { get; set; }

        /// <summary>
        /// Graphics Device
        /// </summary>
        public string graphicsDevice
        { get ; set; }

        /// <summary>
        /// Total Memory
        /// </summary>
        public long memoryTotal
        { get; set; }
    }

    /// <summary>
    /// Host Response Information
    /// </summary>
    [Serializable]
    public class HostResponse
    {
        /// <summary>
        /// Host ID
        /// </summary>
        public int id
        { get; set; }

        /// <summary>
        /// Host Address
        /// </summary>
        public string host
        { get; set; }
    }

    /// <summary>
    /// Login Response Information
    /// </summary>
    [Serializable]
    public class LoginResponse
    {
        /// <summary>
        /// Player ID
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// Login Time
        /// </summary>
        public long loginAt
        { get; set;}

        /// <summary>
        /// Account Creation Time
        /// </summary>
        public long tmcreate
        { get; set;}
    }

    /// <summary>
    /// load player data
    /// </summary>
    [Serializable]
    public class Load
    {
        /// <summary>
        /// player uuid
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// entity name
        /// </summary>
        public string name
        { get; set; }
    }

    /// <summary>
    /// server return player data
    /// </summary>
    public class LoadResponse
    {
        /// <summary>
        /// player id
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// entity name
        /// </summary>
        public string name
        { get; set; }

        /// <summary>
        /// player archive, base 64 string
        /// </summary>
        public string archive
        { get; set; }
    }

    /// <summary>
    /// save player data
    /// </summary>
    public class Save
    {
        /// <summary>
        /// player id
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// entity name
        /// </summary>
        public string name
        { get; set; }

        /// <summary>
        /// entity archive, base 64 string
        /// </summary>
        public string archive
        { get; set; }
    }

    /// <summary>
    /// save response
    /// </summary>
    [Serializable]
    public class SaveResponse
    {
        /// <summary>
        /// player id
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// entity name
        /// </summary>
        public string name
        { get; set; }
    }

    /// <summary>
    /// server time request
    /// </summary>
    [Serializable]
    public class ServerTime
    { 
        public int pid; 
    }

    /// <summary>
    /// server time response
    /// </summary>
    [Serializable]
    public class ServerTimeResponse
    {
        public long time; 
    }
}