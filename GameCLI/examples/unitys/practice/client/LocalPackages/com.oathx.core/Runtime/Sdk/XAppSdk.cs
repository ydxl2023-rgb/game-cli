using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core.Sdk
{
    /// <summary>
    /// AppSdk API
    /// </summary>
    public interface IAppSdkAPI
    {
        /// <summary>
        /// Login to app sdk, and get the user info, token, etc. 
        /// The complete callback will be called when the login process is completed, and the result will be passed to the callback.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Login(string username, string password, string extra, Action<string> complete);
        /// <summary>
        /// Logout from app sdk, and clear the user info, token, etc.
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Logout(Action<bool> complete);
    }

    /// <summary>
    /// The AppSdk is responsible for managing the app sdk apis,
    /// and providing a unified interface for the login/logout process.
    /// </summary>
    public class XAppSdk : XSimpleSingleton<XAppSdk>
    {
        /// <summary>
        /// The list of app sdk apis, each api is responsible for a specific app sdk,
        /// and the login/logout process will be handled by the corresponding api.
        /// </summary>
        private readonly List<IAppSdkAPI> apis = new List<IAppSdkAPI>();

        /// <summary>
        /// Register an app sdk api to the app sdk, and the login/logout process will be handled by the registered api.
        /// </summary>
        /// <param name="api"></param>
        public void RegisterAPI(IAppSdkAPI api)
        {
            if (!apis.Contains(api))
            {
                apis.Add(api);
            }
        }

        /// <summary>
        /// Unregister an app sdk api from the app sdk, and the login/logout process will no longer be handled by the unregistered api.
        /// </summary>
        /// <param name="api"></param>
        public void UnregisterAPI(IAppSdkAPI api)
        {
            if (apis.Contains(api))
            {
                apis.Remove(api);
            }
        }

        /// <summary>
        /// Login to app sdk, and get the user info, token, etc.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Login(string username, string password, string extra, Action<string> complete)
        {
            if (apis.Count <= 0)
                return false;

            foreach (var api in apis)
            {
                if (api.Login(username, password, extra, complete))
                {
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Logout from app sdk, and clear the user info, token, etc.
        /// </summary>
        /// <param name="complete"></param>
        public void Logout(Action<bool> complete) 
        {
            if (apis.Count > 0)
            {
                foreach (var api in apis)
                {
                    api.Logout(complete);
                }
            }
        }

        /// <summary>
        /// Clear all registered APIs.
        /// </summary>
        public void ClearAPI()
        {
            apis.Clear();
        }
    }
}

