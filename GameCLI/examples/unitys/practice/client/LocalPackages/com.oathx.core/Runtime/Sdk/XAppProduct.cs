using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Oathx.Core.Sdk
{
    /// <summary>
    /// product info
    /// </summary>
    public class Product
    {
        /// <summary>
        /// product id
        /// </summary>
        public string id
        { get; set; }

        /// <summary>
        /// product name
        /// </summary>
        public string name
        { get; set; }

        /// <summary>
        /// product quantity
        /// </summary>
        public int quantity
        { get; set; }

        /// <summary>
        /// product description
        /// </summary>
        public string description
        { get; set; }

        /// <summary>
        /// product price
        /// </summary>
        public float price
        { get; set; }

        /// <summary>
        /// product discount, 0.0 - 1.0
        /// </summary>
        public float discount
        { get; set; }

        /// <summary>
        /// product status, 0: unavailable, 1: available
        /// </summary>
        public int status
        { get; set; }

        /// <summary>
        /// product rewards
        /// </summary>
        public List<int> rewards
        { get; set; } = new List<int>();

        /// <summary>
        /// product bind name
        /// </summary>
        public string bind
        { get; set; }
    }

    /// <summary>
    /// order status
    /// </summary>
    public enum OrderStatus
    {
        Created = 0,
        Unpay = 1,
        Pay = 2,
        Succeed = 4,
        Failure = 5
    }

    /// <summary>
    /// order info
    /// </summary>
    [Serializable]
    public class Order
    {
        /// <summary>
        /// order status
        /// </summary>
        public OrderStatus status
        { get; set; }

        /// <summary>
        /// project id
        /// </summary>
        public string projectId
        { get; set; }

        /// <summary>
        /// order id
        /// </summary>
        public string orderId
        { get; set; }

        /// <summary>
        /// product id
        /// </summary>
        public int productId
        { get; set; }

        /// <summary>
        /// order payload
        /// </summary>
        public string extra
        { get; set; }

        /// <summary>
        /// order created time
        /// </summary>
        public string createdAt
        { get; set; }

        /// <summary>
        /// update time of order, will update when order status changed
        /// </summary>
        public string updatedAt
        { get; set; }
    }

    /// <summary>
    /// order result
    /// </summary>
    [Serializable]
    public class OrderResult
    {
        /// <summary>
        /// order result
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// order info
        /// </summary>
        public Order data
        { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string message
        { get; set; }
    }

    /// <summary>
    /// order extra info
    /// </summary>
    [Serializable]
    public class OrderExtra
    {
        /// <summary>
        /// player id
        /// </summary>
        public int pid
        { get; set; }

        /// <summary>
        /// extra info
        /// </summary>
        public string extra
        { get; set; }
    }

    /// <summary>
    /// pay result
    /// </summary>
    [Serializable]
    public class Pay
    {
        /// <summary>
        /// order id
        /// </summary>
        public string orderId
        { get; set; }

        /// <summary>
        /// pay status, 0: unpay, 1: pay succeed, 2: pay failure
        /// </summary>
        public int status
        { get; set; }

        /// <summary>
        /// reward info
        /// </summary>
        public string reward
        { get; set; }
    }

    /// <summary>
    /// pay result
    /// </summary>
    [Serializable]
    public class PayResult
    {
        /// <summary>
        /// pay result
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// pay data
        /// </summary>
        public Pay data
        { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string message
        { get; set; }
    }

    /// <summary>
    /// order consume result
    /// </summary>
    [Serializable]
    public class Consume
    {
        /// <summary>
        /// product id
        /// </summary>
        public string productId
        { get; set; }

        /// <summary>
        /// order id
        /// </summary>
        public string orderId
        { get; set; }

        /// <summary>
        /// consume status, 0: unconsume, 1: consume succeed, 2: consume failure
        /// </summary>
        public int status
        { get; set; }

        /// <summary>
        /// reward info
        /// </summary>
        public string reward
        { get; set; }
    }

    [Serializable]
    public class ConsumeResult
    {
        /// <summary>
        /// consume result
        /// </summary>
        public bool succees
        { get; set; }

        /// <summary>
        /// consume data
        /// </summary>
        public Consume data
        { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string message
        { get; set; }
    }

    [Serializable]
    public class BatchDeliver
    {
        /// <summary>
        /// success deliver orders
        /// </summary>
        public Consume[] deliveredOrders
        { get; set; }

        /// <summary>
        /// failed deliver orders
        /// </summary>
        public Consume[] failedOrders
        { get; set; }
    }

    /// <summary>
    /// batch deliver result
    /// </summary>
    [Serializable]
    public class BatchDeliverResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool success
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public BatchDeliver data
        { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string message 
        { get; set; }
    }

    /// <summary>
    /// product configure interface, user can implement this interface to provide custom product configure
    /// </summary>
    public interface IProductAPI
    {        
        /// <summary>
        /// the URL of the product API for the XApp, provided by XAppConfigure,
        /// used to request product API, this is a default implementation, user can implement IProductAPI to provide custom product API, if not set, will use DefaultProductAPI
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool GetProducts(Action<bool, string> complete);
        /// <summary>
        /// try buy product by product id, this is a default implementation,
        /// user can implement IProductAPI to provide custom product API, if not set, will use DefaultProductAPI
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="extra"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool CreateOrder(string productId, OrderExtra extra, Action<bool, string> complete);
        /// <summary>
        /// try pay by order id, this is a default implementation,
        /// user can implement IProductAPI to provide custom product API, if not set, will use DefaultProductAPI
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Pay(int pid, string orderId, Action<bool, string> complete);
        /// <summary>
        /// try consume by order id, this is a default implementation,
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool Consume(int pid, string orderId, Action<bool, string> complete);
        /// <summary>
        /// try batch consume by project id, this is a default implementation,
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        bool BatchDelivers(int pid, Action<bool, string> complete);
    }

    /// <summary>
    /// product configure interface, user can implement this interface to provide custom product configure
    /// </summary>
    public class InternalProductAPI : IProductAPI
    {
        /// <summary>
        /// the URL prefix of the product API, provided by XAppConfigure, used to request product API
        /// </summary>
        public string api
        { get; private set; }

        /// <summary>
        /// application id of the XApp, provided by XAppConfigure, used to request product API
        /// </summary>
        public string pid
        { get; private set; }

        /// <summary>
        /// constructor of DefaultProductAPI
        /// </summary>
        /// <param name="url"></param>
        /// <param name="projectId"></param>
        public InternalProductAPI(string url, string projectId)
        {
            api = url;
            pid = projectId;
        }

        /// <summary>
        /// The URL of the product API for the XApp.
        /// </summary>
        private string GetProductURL()
        {
            return $"{api}/api/project/{pid}/products/available";
        }

        /// <summary>
        /// The URL of the buy product API for the XApp.
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        private string GetBuyProductURL(string productId)
        {
            return $"{api}/api/project/{pid}/products/{productId}/buy";
        }

        /// <summary>
        /// //api/project/:projectId/orders/:orderId/pay
        /// </summary>
        /// <returns></returns>
        private string GetPayURL(string orderId)
        {
            return $"{api}/api/project/{pid}/orders/{orderId}/pay";
        }

        /// <summary>
        /// /api/project/{projectId}/orders/{orderId}/deliver 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private string GetDeliverURL(string orderId)
        {
            return $"{api}/api/project/{pid}/orders/{orderId}/deliver";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        private string GetBatchDeliverURL()
        {
            return $"{api}/api/project/{pid}/orders/batch-deliver";
        }

        /// <summary>
        /// try get product list by project id, 
        /// this is a default implementation, user can implement IProductAPI to provide custom product API, if not set, will use DefaultProductAPI
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool GetProducts(Action<bool, string> complete)
        {
            string url = GetProductURL();
            if (string.IsNullOrEmpty(url))
                return false;

            XCoroutine.Run(DoGetProducts(url, complete));
            return true;
        }

        /// <summary>
        /// do get product list by project id
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoGetProducts(string url, Action<bool, string> complete)
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
        /// try buy product by product id
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool CreateOrder(string productId, OrderExtra extra, Action<bool, string> complete)
        {
            if (string.IsNullOrEmpty(productId))
                return false;

            XCoroutine.Run(DoCreateOrder(GetBuyProductURL(productId), extra, complete));
            return true;
        }

        /// <summary>
        /// do buy product by product id
        /// </summary>
        /// <param name="url"></param>
        /// <param name="productId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoCreateOrder(string url, OrderExtra extra, Action<bool, string> complete)
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(extra));
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
        /// try pay by order id
        /// </summary>
        /// <param name="url"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Pay(int pid, string orderId, Action<bool, string> complete)
        {
            XCoroutine.Run(DoPay(GetPayURL(orderId), pid, complete));
            return true;
        }

        /// <summary>
        /// do pay by order id
        /// </summary>
        /// <param name="url"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoPay(string url, int pid, Action<bool, string> complete)
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { pid }));
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
        /// try consume by order id ConsumeResult
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public bool Consume(int pid, string orderId, Action<bool, string> complete)
        {
            XCoroutine.Run(DoConsume(GetDeliverURL(orderId), pid, complete));
            return true;
        }

        /// <summary>
        /// do pay by order id ConsumeResult
        /// </summary>
        /// <param name="url"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoConsume(string url, int pid, Action<bool, string> complete)
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");
            
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { pid }));
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
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool BatchDelivers(int pid, Action<bool, string> complete)
        {
            XCoroutine.Run(DoBatchDelivers(GetBatchDeliverURL(), pid, complete));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        IEnumerator DoBatchDelivers(string url, int pid, Action<bool, string> complete)
        {
            UnityWebRequest req = new UnityWebRequest(url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { pid }));
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
    /// product manager
    /// </summary>
    public class XAppProduct : XSimpleSingleton<XAppProduct>
    {
        /// <summary>
        /// product API list, user can implement IProductAPI to provide custom product API, 
        /// if not set, will use DefaultProductAPI, this is for future use, currently only support one product API
        /// </summary>
        private readonly List<IProductAPI> apis = new List<IProductAPI>();

        /// <summary>
        /// register product API, user can implement IProductAPI to provide custom product API, 
        /// if not set, will use DefaultProductAPI, this is for future use, currently only support one product API
        /// </summary>
        /// <param name="api"></param>
        public void RegisterAPI(IProductAPI api)
        {
            if (!apis.Contains(api))
                apis.Add(api);
        }

        /// <summary>
        /// unregister product API, user can implement IProductAPI to provide custom product API,
        /// </summary>
        /// <param name="api"></param>
        public void UnregisterAPI(IProductAPI api)
        {
            if (apis.Contains(api))
                apis.Remove(api);
        }

        /// <summary>
        /// clear all product API, user can implement IProductAPI to provide custom product API,
        /// </summary>
        public void ClearAPI()
        {
            apis.Clear();
        }

        /// <summary>
        /// try get product list by project id
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool GetProducts(Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;

            foreach (var api in apis)
            {
                api.GetProducts(complete);
            }

            return true;
        }

        /// <summary>
        /// try buy product by product id OrderResult
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="extra"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool CreateOrder(string productId, OrderExtra extra, Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;
            
            foreach (var api in apis)
            {
                api.CreateOrder(productId, extra, complete);
            }

            return true;
        }

        /// <summary>
        /// PayResult
        /// try pay by order id
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Pay(int pid, string orderId, Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;

            foreach (var api in apis)
            {
                api.Pay(pid, orderId, complete);
            }

            return true;
        }

        /// <summary>
        /// try consume by order id ConsumeResult
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="orderId"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool Consume(int pid, string orderId, Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;

            foreach (var api in apis)
            {
                api.Consume(pid, orderId, complete);
            }

            return true;
        }

        /// <summary>
        /// try batch consume by project id BatchDeliverResult
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public bool BatchDelivers(int pid, Action<bool, string> complete)
        {
            if (apis.Count <= 0)
                return false;
            
            foreach (var api in apis)
            {
                api.BatchDelivers(pid, complete);
            }

            return true;
        }
    }
}