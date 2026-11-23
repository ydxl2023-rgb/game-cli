using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Oathx.Core.Sdk
{
    /// <summary>
    /// tracking api interface, used to track user behavior, such as page view, click event, etc.
    /// </summary>
    public interface ITrackingAPI
    {
        /// <summary>
        /// track user behavior, such as page view, click event, etc.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Track(string eventName, Dictionary<string, object> properties = null, Action<bool> complete = null);
        /// <summary>
        /// track user behavior with enum, such as page view, click event, etc.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Login(Action<bool, string> complete);
        /// <summary>
        /// play ad, used to track play ad,
        /// such as play ad from local, play ad from remote, etc.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool PlayAD(string type, string action, string adUnitId,
            string errorCode, string errorMessage, Action<bool, string> complete);
    }

    /// <summary>
    /// tracking event params, used to track user behavior, such as page view, click event, etc.
    /// </summary>
    [Serializable]
    public class ReportTrackingEvent
    {
        /// <summary>
        /// project id, used to track the project id, 
        /// such as the app id, the channel id, etc.
        /// </summary>
        public string projectId
        { get; set; }

        /// <summary>
        /// player id, used to track the player id, 
        /// such as the device unique identifier, the player id from server, etc.
        /// </summary>
        public string playerId
        { get; set; }

        /// <summary>
        /// event name, used to track the name of the event, such as page view, click event, etc.
        /// </summary>
        public string eventName
        { get; set; }

        /// <summary>
        /// event params, used to track the parameters of the event, 
        /// such as the parameters of the page view, the parameters of the click event, etc.
        /// </summary>
        public object eventParams
        { get; set;}

        /// <summary>
        /// client time, used to track the time of the event, 
        /// such as the time of the page view, the time of the click event, etc.
        /// </summary>
        public string clientTime
        { get; set; }
    }

    /// <summary>
    /// play ad event params,
    /// used to track play ad event, such as play ad from local, play ad from remote, etc.
    /// </summary>
    public class ReportPlayAD
    {
        /// <summary>
        /// project id, used to track the project id,
        /// </summary>
        public string projectId
        { get; set; }

        /// <summary>
        /// player id, used to track the player id,
        /// </summary>
        public string playerId
        { get; set; }

        /// <summary>
        /// ad type, used to track the type of the play ad event, 
        /// such as rewarded video, interstitial, banner, etc.
        /// </summary>
        public string adType
        { get; set; }

        /// <summary>
        /// ad action, used to track the action of the play ad event,
        /// such as play, click, complete, etc.
        /// </summary>
        public string action 
        { get; set; }

        /// <summary>
        /// ad unit id, used to track the ad unit id of the play ad event,
        /// </summary>
        public string adUnitId 
        { get; set; }

        /// <summary>
        /// error code, used to track the error code of the play ad event,
        /// </summary>
        public string errorCode
        { get; set; }

        /// <summary>
        /// error message, used to track the error message of the play ad event,
        /// </summary>
        public string errorMessage
        { get; set; }

        /// <summary>
        /// client time, used to track the time of the event,
        /// </summary>
        public string clientTime
        { get; set; }
    }

    /// <summary>
    /// ad type, used to track the type of the play ad event,
    /// </summary>
    public enum ADType
    {
        /// <summary>
        /// rewarded video, used to track the rewarded video ad type of the play ad event,
        /// </summary>
        rewarded,
        /// <summary>
        /// interstitial, used to track the interstitial ad type of the play ad event,
        /// </summary>
        interstitial,
        /// <summary>
        /// banner, used to track the banner ad type of the play ad event,
        /// </summary>
        banner,
        /// <summary>
        /// native, used to track the native ad type of the play ad event,
        /// </summary>
        native,
        /// <summary>
        /// app open, used to track the app open ad type of the play ad event,
        /// </summary>
        app_open,
        /// <summary>
        /// rewarded interstitial, 
        /// used to track the rewarded interstitial ad type of the play ad event,
        /// </summary>
        rewarded_interstitial
    }

    /// <summary>
    /// ad action, used to track the action of the play ad event,
    /// </summary>
    public enum ADActionType
    {
        /// <summary>
        /// request, used to track the request action of the play ad event,
        /// </summary>
        request,
        /// <summary>
        /// show, used to track the show action of the play ad event,
        /// </summary>
        show,
        /// <summary>
        /// click, used to track the click action of the play ad event,
        /// </summary>
        click,
        /// <summary>
        /// complete, used to track the complete action of the play ad event,
        /// </summary>
        complete,
        /// <summary>
        /// error, used to track the error action of the play ad event,
        /// </summary>
        error
    }

    /// <summary>
    /// default tracking api, used to track user behavior, such as page view, click event, etc.
    /// </summary>
    public class InternalTrackingAPI : ITrackingAPI
    {
        /// <summary>
        /// app configure, used to get app id, channel id, etc.
        /// </summary>
        public XAppConfigure conf
        { get; private set; }

        /// <summary>
        /// initialize tracking api with app configure
        /// </summary>
        /// <param name="appConf"></param>
        public InternalTrackingAPI(XAppConfigure appConf)
        {
            conf = appConf;
        }

        /// <summary>
        /// get tracking url, you can use app id and channel id to construct the url, or return a default url
        /// </summary>
        /// <returns></returns>
        private string GetEventUrl()
        {
            return $"{conf.versionAPI}/api/stats/event";
        }

        /// <summary>
        /// get tracking url for login, you can use app id and channel id to construct the url, or return a default url
        /// </summary>
        /// <returns></returns>
        private string GetLoginUrl()
        {
            return $"{conf.versionAPI}/api/stats/player/login";
        }

        /// <summary>
        /// get play ad url, 
        /// you can use app id and channel id to construct the url, or return a default url
        /// </summary>
        /// <returns></returns>
        private string GetPlayADUrl()
        {
            return $"{conf.versionAPI}/api/ad/event";
        }

        /// <summary>
        /// login, used to track login, such as login from local, login from remote, etc.
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Login(Action<bool, string> complete)
        {
            if (string.IsNullOrEmpty(conf.appId))
                return false;

            XCoroutine.Run(DoLogin(GetLoginUrl(), conf.appId, complete));

            return true;
        }

        /// <summary>
        /// do login, send login data to server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="projectId"></param>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoLogin(string url, string projectId, Action<bool, string> complete)
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { projectId=projectId, playerId = SystemInfo.deviceUniqueIdentifier }));
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

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
        /// track user behavior, such as page view, click event, etc.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Track(string eventName, Dictionary<string, object> properties = null, Action<bool> complete = null)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;

            XCoroutine.Run(DoTrack(GetEventUrl(), eventName, properties, complete));

            return true;
        }

        /// <summary>
        /// track user behavior with enum, such as page view, click event, etc.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Track(Enum eventType, Dictionary<string, object> properties = null, Action<bool> complete = null)
        {
            return Track(eventType.ToString(), properties, complete);
        }

        /// <summary>
        /// create tracking params, used to create tracking params,
        /// such as project id, player id, event name, event params, client time, etc.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        private string CreateTrackingParams(string eventName, Dictionary<string, object> properties)
        {
            var eventParams = new ReportTrackingEvent()
            {
                projectId = conf.appId,
                playerId = SystemInfo.deviceUniqueIdentifier,
                eventName = eventName,
                eventParams = properties ?? new Dictionary<string, object>(),
                clientTime = DateTime.Now.ToString()
            };

            return JsonConvert.SerializeObject(eventParams);
        }

        /// <summary>
        /// do track, send tracking data to server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoTrack(string url, string eventName, Dictionary<string, object> properties, Action<bool> complete)
        {
            string json = CreateTrackingParams(eventName, properties);

            UnityWebRequest req = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

            yield return req.SendWebRequest();

            try
            {
                if (req.result != UnityWebRequest.Result.Success)
                {
                    complete?.Invoke(false);
                }
                else
                {
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        string text = req.downloadHandler.text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            complete?.Invoke(true);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                complete?.Invoke(false);

#if UNITY_EDITOR
                Debug.LogError(exception);
#endif
            }
        }

        /// <summary>
        /// play ad, used to track play ad,
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool PlayAD(string type, string action, string adUnitId,
            string errorCode, string errorMessage, Action<bool, string> complete) 
        {
            if (string.IsNullOrEmpty(type) ||
                string.IsNullOrEmpty(action) || string.IsNullOrEmpty(adUnitId))
            {
                return false;
            }

            XCoroutine.Run(DoPlayAD(GetPlayADUrl(), type, action, adUnitId, errorCode, errorMessage, complete));

            return true;
        }

        /// <summary>
        /// create play ad params, used to create play ad params,
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private string CreatePlayADParams(string type, string action, string adUnitId,
            string errorCode, string errorMessage)
        {
            var report = new ReportPlayAD()
            {
                projectId = conf.appId,
                playerId = SystemInfo.deviceUniqueIdentifier,
                action = action,
                adType = type,
                adUnitId = adUnitId,
                clientTime = DateTime.Now.ToString(),
                errorCode = errorCode,
                errorMessage = errorMessage
            };

            return JsonConvert.SerializeObject(report);
        }

        /// <summary>
        /// do play ad, send play ad data to server
        /// </summary>
        /// <param name="url"></param>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoPlayAD(string url, string type, string action, string adUnitId, 
            string errorCode, string errorMessage, Action<bool, string> complete)
        {
            string json = CreatePlayADParams(type, action, adUnitId, errorCode, errorMessage);

            UnityWebRequest req = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();

            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Accept", "application/json");

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
    }

    /// <summary>
    /// tracking event, used to track user behavior, such as page view, click event, etc.
    /// </summary>
    public enum TrackingEvent
    {
        /// <summary>
        /// check app version event, used to track check app version, such as check app version from local, check app version from remote, etc.
        /// </summary>
        CheckAppVersion,
        /// <summary>
        /// update app event, used to track update app, such as update app from local, update app from remote, etc.
        /// </summary>
        DoUpdate,
        /// <summary>
        /// run assembly event, used to track run assembly, such as run assembly from local, run assembly from remote, etc.
        /// </summary>
        RunAssembly,
        /// <summary>
        /// login event, used to track login, such as login from local, login from remote, etc.
        /// </summary>
        Login,
        /// <summary>
        /// game start event, used to track game start, such as game start from local, game start from remote, etc.
        /// </summary>
        GameStart,
        /// <summary>
        /// page view event
        /// </summary>
        PageHome,
        /// <summary>
        /// click event
        /// </summary>
        Click,
        /// <summary>
        /// purchase event
        /// </summary>
        Purchase,
        /// <summary>
        /// level up event
        /// </summary>
        LevelUp
    }

    /// <summary>
    /// app tracking manager, used to track user behavior, such as page view, click event, etc.
    /// </summary>
    public class XAppTracking : XSimpleSingleton<XAppTracking>
    {
        /// <summary>
        /// tracking api list, used to store all registered tracking api, 
        /// the tracking data will be sent to all registered tracking api
        /// </summary>
        private List<ITrackingAPI> apis = new List<ITrackingAPI>();

        /// <summary>
        /// register tracking api, you can register multiple tracking api, 
        /// and the tracking data will be sent to all registered tracking api
        /// </summary>
        /// <param name="trackingAPI"></param>
        public void RegisterAPI(ITrackingAPI trackingAPI)
        {
            if (!apis.Contains(trackingAPI))
                apis.Add(trackingAPI);
        }

        /// <summary>
        /// unregister tracking api, you can unregister tracking api when you don't need it anymore,
        /// and the tracking data will not be sent to the unregistered tracking api
        /// </summary>
        /// <param name="trackingAPI"></param>
        public void Unregister(ITrackingAPI  trackingAPI)
        {
            if (apis.Contains(trackingAPI))
                apis.Remove(trackingAPI);
        }

        /// <summary>
        /// clear all registered tracking api
        /// </summary>
        public void ClearAPI()
        {
            apis.Clear();   
        }

        /// <summary>
        /// login, used to track login, such as login from local, login from remote, etc.
        /// </summary>
        public void Login(Action<bool, string> complete)
        {
            foreach (var api in apis)
            {
                api.Login(complete);
            }
        }

        /// <summary>
        /// track user behavior, such as page view, click event, etc. 
        /// the tracking data will be sent to all registered tracking api
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        public void Track(string eventName, Dictionary<string, object> properties = null, Action<bool> complete = null)
        {
            foreach (var api in apis)
            {
                api.Track(eventName, properties, complete);
            }
        }

        /// <summary>
        /// track user behavior with enum, such as page view, click event, etc. 
        /// the tracking data will be sent to all registered tracking api
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="properties"></param>
        /// <param name="complete"></param>
        public void Track(Enum eventName, Dictionary<string, object> properties = null, Action<bool> complete = null)
        {
            Track(eventName.ToString(), properties, complete);
        }

        /// <summary>
        /// play ad, used to track play ad, 
        /// such as play ad from local, play ad from remote, etc.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="complete"></param>
        public void PlayAD(string type, string action, string adUnitId,
            string errorCode, string errorMessage, Action<bool, string> complete)
        {
            foreach(var api in apis)
            {
                api.PlayAD(type, action, adUnitId, errorCode, errorMessage, complete);
            }
        }

        /// <summary>
        /// play ad, used to track play ad,
        /// </summary>
        /// <param name="type"></param>
        /// <param name="action"></param>
        /// <param name="adUnitId"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorMessage"></param>
        /// <param name="complete"></param>
        public void PlayAD(ADType type, ADActionType action, string adUnitId,
            string errorCode="", string errorMessage="", Action<bool, string> complete=null)
        {
            PlayAD(type.ToString(), action.ToString(), adUnitId, errorCode, errorMessage, complete);
        }
    }
}
