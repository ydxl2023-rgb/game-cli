using Oathx.Rpc;
using Oathx.HttpProtocol;
using Oathx.Entity;
using System;
using Oathx.Bundle.Assets;

[RpcInterface]
public interface IRpcInterface
{
    #region RpcHttpFunctions
    /// <summary>
    /// request host, this rpc is just for test http rpc, because http rpc is more suitable for request host, 
    /// so just use http rpc to request host in real project, and use tcp rpc to request host in test project
    /// </summary>
    /// <param name="host"></param>
    /// <param name="complete"></param>
    [RpcClientCallServer(XRpcType.Http)]
    void ReqHost(Host host, Action<int, HostResponse> complete);

    /// <summary>
    /// request login, this rpc is more suitable for request login, because http rpc is more suitable for request login,
    /// so just use http rpc to request login in real project, and use tcp rpc to request login in test project
    /// </summary>
    /// <param name="host"></param>
    /// <param name="complete"></param>
    [RpcClientCallServer(XRpcType.Http)]
    void ReqLogin(Host host, Action<int, LoginResponse> complete);

    /// <summary>
    /// request save player data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="complete"></param>
    [RpcClientCallServer(XRpcType.Http)]
    void ReqSave(Save data, Action<int, SaveResponse> complete);

    /// <summary>
    /// request load player data
    /// </summary>
    /// <param name="data"></param>
    /// <param name="complete"></param>
    [RpcClientCallServer(XRpcType.Http)]
    void ReqLoad(Load data, Action<int, LoadResponse> complete);

    /// <summary>
    /// request server time
    /// </summary>
    /// <param name="body"></param>
    /// <param name="complete"></param>
    [RpcClientCallServer(XRpcType.Http)]
    void ReqServerTime(ServerTime body, Action<int, ServerTimeResponse> complete);
    #endregion RpcHttpFunctions
}

/// <summary>
/// 
/// </summary>
public partial class Rpc
{
    /// rpc utility functions
    #region RpcUtility
    /// <summary>
    /// request save entity
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="entity"></param>
    /// <param name="complete"></param>
    public static void Save(int pid, XEntitySerialize entity, Action<int, SaveResponse> complete=null)
    {
        if (entity)
        {
            var version = XRes.GetVersion();
            if (!string.IsNullOrEmpty(version))
                entity.version = version;

            var archive = entity.Serialize();
            if (!string.IsNullOrEmpty(archive))
            {
                ReqSave(new Save() {
                    pid = pid, name = entity.GetType().Name, archive = archive
                }, complete);
            }
        }
    }

    /// <summary>
    /// request update entity, update is just save to local storage, never request save to remote server,
    /// because update is just for local data update, if you want to save to remote server, please use save function
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pid"></param>
    /// <param name="entity"></param>
    public static void Update<T>(int pid, T entity) where T : XEntitySerialize
    {
        Save(pid, entity);
    }

    /// <summary>
    /// request load entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="pid"></param>
    public static void Load<T>(int pid, Action<int, T> complete) where T : XEntitySerialize
    {
        var entity = XEntityManager.GetSingleton().QueryEntity<T>();
        if (entity)
        {
            bool succeed = false;
            if (entity.ExistsSerializeFile())
            {
                succeed = entity.Deserialize();
                if (succeed)
                {
                    complete?.Invoke(HttpCode.SUCCEED, entity);
                }
            }

            if (succeed)
            {
                // deserialize succeed, force serialize to local storage
                // because maybe local data is newer than server data
                // so just try load from server, if load failure, then just launch home system
                Update(pid, entity);
            }
            else
            {
                var req = new Load()
                {
                    pid = pid, name = entity.GetType().Name
                };

                // if load failure then load entity data from remote server
                ReqLoad(req, (code, response) =>
                {
                    if (code == HttpCode.SUCCEED)
                    {
                        if (!string.IsNullOrEmpty(response.archive))
                        {
                            succeed = entity.Deserialize(response.archive);
                            if (succeed)
                                complete?.Invoke(HttpCode.SUCCEED, entity);
                        }
                    }

                    if (!succeed)
                    {
                        complete?.Invoke(code, entity);

                        // only initialize server archive when server has no record yet (FAILURE),
                        // never overwrite existing server data on deserialize failure or other errors
                        if (code == HttpCode.FAILURE)
                        {
                            Save(pid, entity);
                        }
                    }
                });
            }
        }
    }
    #endregion
}