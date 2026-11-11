using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Oathx.Core.Actitvity
{
    /// <summary>
    /// activity info
    /// </summary>
    [System.Serializable]
    public class Activity
    {
        /// <summary>
        /// activity id
        /// </summary>
        public int id
        { get; set; }

        /// <summary>
        /// activity type, for example, 1 for login activity, 2 for consume activity, etc.
        /// </summary>
        public int type
        { get; set; }

        /// <summary>
        /// activity status, for example, 1 for active, 2 for inactive, etc.
        /// </summary>
        public int status
        { get; set; }

        /// <summary>
        /// activity title
        /// </summary>
        public string startTime
        { get; set; }

        /// <summary>
        /// activity description
        /// </summary>
        public string endTime
        { get; set; }

        /// <summary>
        /// activity description
        /// </summary>
        public string description
        { get; set; }

        /// <summary>
        /// activity created time
        /// </summary>
        public string createdAt
        { get; set; }
    }

    /// <summary>
    /// activity api
    /// </summary>
    public interface IActitityAPI
    {
        /// <summary>
        /// try to get the activitys, and the result will be passed to the callback.
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        void GetActivitys(Action<bool, string> complete);
    }

    /// <summary>
    /// activity api result
    /// </summary>
    public class ActivityResult
    {
        /// <summary>
        /// whether the api call is successful or not
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// the activitys data when the api call is successful, otherwise, it will be null or empty.
        /// </summary>
        public List<Activity> data
        { get; set; } = new List<Activity>();

        /// <summary>
        /// the error message when the api call is failed, for example, network error, server error, etc.
        /// </summary>
        public string message
        { get; set; }
    }

    /// <summary>
    /// internal activity api, for example, get the activitys from server, or get the activitys from local config, etc.
    /// </summary>
    public class InternalActivityAPI : IActitityAPI
    {
        /// <summary>
        /// app configure
        /// </summary>
        public XAppConfigure conf
        { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="appConfigure"></param>
        public InternalActivityAPI(XAppConfigure appConfigure)
        {
            conf = appConfigure;
        }

        /// <summary>
        /// try to get the activitys from server, and the result will be passed to the callback.
        /// </summary>
        /// <param name="activitys"></param>
        /// <returns></returns>
        public void GetActivitys(Action<bool, string> complete)
        {
            XCoroutine.Run(DoGetActivitys(GetActivityURL(), complete));
        }

        /// <summary>
        /// get the activitys from server, and the result will be passed to the callback.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoGetActivitys(string url, Action<bool, string> complete)
        {
            UnityWebRequest req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            try
            {
                if (req.result != UnityWebRequest.Result.Success)
                {
                    complete?.Invoke(false, req.error);
                }
                else
                {
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        string text = req.downloadHandler.text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            complete?.Invoke(true, text);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                complete?.Invoke(false, exception.Message);

#if UNITY_EDITOR
                Debug.LogError(exception);
#endif
            }
        }

        /// <summary>
        /// get the activitys from server, and the result will be passed to the callback.
        /// </summary>
        /// <returns></returns>
        private string GetActivityURL()
        {
            return $"{conf.versionAPI}/api/project/{conf.appId}/activities/open";
        }
    }

    /// <summary>
    /// app activity
    /// </summary>
    public class XAppActivity : XSimpleSingleton<XAppActivity>
    {
        private readonly List<IActitityAPI> apis = new List<IActitityAPI>();

        /// <summary>
        /// register api
        /// </summary>
        /// <param name="api"></param>
        public void RegisterAPI(IActitityAPI api)
        {
            if (!apis.Contains(api))
            {
                apis.Add(api);
            }
        }

        /// <summary>
        /// unregister api
        /// </summary>
        /// <param name="api"></param>
        public void UninstallAPI(IActitityAPI api)
        {
            if (apis.Contains(api))
                apis.Remove(api);
        }

        /// <summary>
        /// clear all apis
        /// </summary>
        public void ClearAPI()
        {
            apis.Clear();
        }

        /// <summary>
        /// try to get the activitys, and the result will be passed to the callback.
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool GetActivitys(Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;

            foreach (var api in apis) 
            {
                api.GetActivitys(complete);
            }

            return true;
        }
    }
}
