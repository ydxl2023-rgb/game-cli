using Oathx.Core;
using Oathx.Entity;
using Oathx.HttpProtocol;
using Oathx.Rpc;
using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// login succeed event
/// </summary>
public class LoginSucceedEvent : IEvent
{
    /// <summary>
    /// player id
    /// </summary>
    public int pid
    { get; set; }
}

/// <summary>
/// login failure type
/// </summary>
public enum LoginFailureCode
{
    RequestHostFailure,
    ConnectFailure,
    DataError,
    LoginError
}

/// <summary>
/// login failure event
/// </summary>
public class LoginFailureEvent : IEvent
{
    /// <summary>
    /// player id
    /// </summary>
    public LoginFailureCode failure
    { get; set; }
}

/// <summary>
/// master system
/// </summary>
public class MasterSystem : RuntimeSystem
{
    /// <summary>
    /// active system
    /// </summary>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public override bool OnActive()
    {
        Subscribe<LoginSucceedEvent>(OnLoginSucceedEvent);
        Subscribe<LoginFailureEvent>(OnLoginFailureEvent);

        return true;
    }

    /// <summary>
    /// deactive system
    /// </summary>
    /// <exception cref="System.NotImplementedException"></exception>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// proc login succeed event
    /// </summary>
    /// <param name="evt"></param>
    public void OnLoginSucceedEvent(LoginSucceedEvent data)
    {
        Rpc.Load<MasterEntity>(data.pid, (code, master) => {
            if (code == HttpCode.SUCCEED)
            {
                if (master)
                {
                    var pid = master.GetAttr(MasterPropertyDef.PID);
                    if (pid != data.pid)
                        master.SetAttr(MasterPropertyDef.PID, data.pid);

                    // deserialize succeed, force serialize to local storage
                    string local = master.Serialize();
                    if (!string.IsNullOrEmpty(local))
                    {
                        Launch();
                    }
                }
                else
                {
                    // if load failure, then just launch home system, because maybe player just created,
                    // but local data is newer than server data, so just try load from server, if load failure, then just launch home system
                    Launch();
                }
            }
        });   

    }

    /// <summary>
    /// proc login failure event, 
    /// we just launch home system, because maybe player just created, 
    /// but local data is newer than server data, 
    /// so just try load from server, if load failure, then just launch home system
    /// </summary>
    /// <param name="data"></param>
    public void OnLoginFailureEvent(LoginFailureEvent data)
    {
        var master = XEntityManager.GetSingleton().QueryEntity<MasterEntity>();
        if (master)
        {
            var pid = master.GetAttr(MasterPropertyDef.PID);
            if (!master.IsValid())
                master.Serialize();

            Launch();
        }
    }

    /// <summary>
    /// request load response, then launch home system
    /// </summary>
    /// <param name="code"></param>
    /// <param name="response"></param>
    public void DoLoad(int code, LoadResponse response)
    {
        var exist = XEntityManager.GetSingleton().TryGetEntity(response.name, out var entity);
        if (exist)
        {
            if (entity is XEntitySerialize es)
            {
                bool succeed = false;
                if (code == HttpCode.SUCCEED)
                {
                    string archive = response.archive;
                    if (!string.IsNullOrEmpty(archive))
                        succeed = es.Deserialize(archive);
                }

                if (succeed)
                {
                    // deserialize succeed, force serialize to local storage
                    string local = es.Serialize();
                    if (!string.IsNullOrEmpty(local))
                    {
                        if (response.name == typeof(MasterEntity).Name)
                        {
                            Launch();
                        }
                    }
                }
                else
                {
                    // force update pid, because entity maybe just created
                    var pid = entity.GetAttr(MasterPropertyDef.PID);
                    if (pid != response.pid)
                        entity.SetAttr(MasterPropertyDef.PID, response.pid);

                    Rpc.Save(response.pid, es, (code, response) => {
                        if (code == HttpCode.SUCCEED)
                        {
                            if (response.name == typeof(MasterEntity).Name)
                            {
                                Launch();
                            }
                        }
                    });
                }
            }
        }
    }

    /// <summary>
    /// request save response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="response"></param>
    [RpcServerCallClient(XRpcType.Http)]
    public void ReqSave(int code, SaveResponse response)
    {
        if (code !=  HttpCode.SUCCEED)
            Debug.LogError(code);
    }

    /// <summary>
    /// launch game home system
    /// </summary>
    private bool Launch()
    {
        UnloadSystem(typeof(LoginSystem), () => {
            LoadSystem(typeof(HomeSystem));
            LoadSystem(typeof(BackpackSystem));
            LoadSystem(typeof(EmailSystem));
            LoadSystem(typeof(CharacterSystem));
            LoadSystem(typeof(SevenDayLoginSystem));
        });

        return true;
    }

}
