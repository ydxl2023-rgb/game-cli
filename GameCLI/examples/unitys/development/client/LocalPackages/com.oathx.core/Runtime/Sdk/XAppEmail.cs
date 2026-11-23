using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Oathx.Core.Sdk
{
    public interface IEmailAPI
    {
        /// <summary>
        /// get all emails
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        void GetEmails(int pid, Action<bool, string> complete);

        /// <summary>
        /// claim email
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        void Claim(int pid, int emailId, Action<bool, string> complete);
    }

    /// <summary>
    /// 
    /// </summary>
    public class InternalEmailAPI : IEmailAPI
    {
        /// <summary>
        /// 
        /// </summary>
        public XAppConfigure conf
        { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appConfigure"></param>
        public InternalEmailAPI(XAppConfigure appConfigure)
        {
            conf = appConfigure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public void GetEmails(int pid, Action<bool, string> complete)
        {
            XCoroutine.Run(DoGetEmails(GetEmailURL(pid), complete));
        }

        /// <summary>
        /// do get emails
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoGetEmails(string url, Action<bool, string> complete)
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
        /// get email url
        /// </summary>
        /// <returns></returns>
        private string GetEmailURL(int pid)
        {
            return $"{conf.versionAPI}/api/project/{conf.appId}/player/{pid}/emails/available";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        public void Claim(int pid, int emailId, Action<bool, string> complete)
        {
            XCoroutine.Run(DoClaim(GetClaimURL(pid, emailId), pid, complete));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoClaim(string url, int pid, Action<bool, string> complete) 
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new {pid=pid}));
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
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetClaimURL(int pid, int emailId)
        {
            return $"{conf.versionAPI}/api/project/{conf.appId}/player/{pid}/emails/{emailId}/claim";
        }
    }

    /*
     * {
          "success": true,
          "data": {
            "success": true,
            "email": {
              "id": 5,
              "title": "欢迎邮件",
              "content": "欢迎加入游戏！",
              "reward": "金币x100",
              "status": 1
            },
            "claimedAt": "2026-02-12T11:30:00.000Z"
          },
          "message": "邮件领取成功"
        }
     */

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class Email
    {
        /// <summary>
        /// 
        /// </summary>
        public int id
        {  get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        public string projectId
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string title
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string content
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int type
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int status
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int claimStatus
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string reward
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string startTime
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string endTime
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string createdAt
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string updatedAt
        {  get; set; }
    }

    [Serializable]
    public class EmailResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Email> data
        { get; set; } = new List<Email>();

        /// <summary>
        /// 
        /// </summary>
        public string message
        { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ClaimResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Email email
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string claimedAt
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string message
        { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class XAppEmail : XSimpleSingleton<XAppEmail>
    {
        private readonly List<IEmailAPI> apis = new List<IEmailAPI>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAPI"></param>
        public void RegisterAPI(IEmailAPI emailAPI) 
        {
            if (!apis.Contains(emailAPI))
                apis.Add(emailAPI);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailAPI"></param>
        public void UnregisterAPI(IEmailAPI emailAPI) 
        {
            if (apis.Contains(emailAPI))
                apis.Remove(emailAPI);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool GetEmails(int pid, Action<bool, string> complete)
        {
            if (apis.Count == 0)
                return false;

            foreach(IEmailAPI emailAPI in apis)
            {
                emailAPI.GetEmails(pid, complete);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Claim(int pid, int emailId, Action<bool, string> complete)
        {
            if (apis.Count == 0)
                return false;

            foreach (var emailAPI in apis) 
            {
                emailAPI.Claim(pid, emailId, complete);
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ClearAPI()
        {
            apis.Clear();
        }
    }

}
