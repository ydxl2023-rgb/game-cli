using System;
using UnityEngine;
using Oathx.Core;
using Oathx.Rpc;
using Oathx.HttpProtocol;
using Oathx.Bundle.Assets;
using Oathx.Entity;

/// <summary>
/// login system
/// </summary>
public class LoginSystem : RuntimeSystem
{
    /// <summary>
    /// active system
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override bool OnActive()
    {
        // fetch server address, then request login
        FeatchServerAddress(XRes.GetVersion());
        return true;
    }

    /// <summary>
    /// deactive system
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public override void OnDetive()
    {
        UnsubscribeAll();
    }

    /// <summary>
    /// request host response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="response"></param>
    public void DoHost(int code, HostResponse response)
    {
        bool succeed = false;
        // if request host succeed, then request login
        if (code == HttpCode.SUCCEED)
            succeed = Login(response);

        // if request host failure, or request login failure,
        // then fire login failure event, and just launch home system,
        // because maybe player just created,
        // but local data is newer than server data,
        // so just try load from server,
        // if load failure, then just launch home system
        if (!succeed)
        {
            XPluginUtility.FireEvent(new LoginFailureEvent() {
                failure = LoginFailureCode.RequestHostFailure
            });
        }
    }

    /// <summary>
    /// connect server failure
    /// </summary>
    /// <param name="type"></param>
    /// <param name="host"></param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    [RpcServerCallClient(XRpcType.Http)]
    public void ConnectFailure(XRpcType type, string host, string address, int port) 
    {
        XPluginUtility.FireEvent(new LoginFailureEvent() { 
            failure = LoginFailureCode.ConnectFailure
        });
    }

    /// <summary>
    /// connect server succeed
    /// </summary>
    /// <param name="type"></param>
    /// <param name="host"></param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    [RpcServerCallClient(XRpcType.Http)]
    public void ConnectSucceed(XRpcType type, string host, string address, int port)
    {
        // do nothing
    }

    /// <summary>
    /// request login response
    /// </summary>
    /// <param name="code"></param>
    /// <param name="response"></param>
    public void DoLogin(int code, LoginResponse response)
    {
        if (code == HttpCode.SUCCEED)
        {
            var entity = XEntityManager.GetSingleton().QueryEntity<LoginEntity>();
            if (entity)
                entity.Refresh(response);

            // fire login succeed event, then launch home system
            XPluginUtility.FireEvent(new LoginSucceedEvent() {
                pid = response.pid
            });
        }
        else
        {
            // fire login failure event
            // and just launch home system,
            XPluginUtility.FireEvent(new LoginFailureEvent() {
                failure = LoginFailureCode.LoginError
            });
        }
    }

    /// <summary>
    /// request server host
    /// </summary>
    /// <param name="rpcType"></param>
    /// <param name="rpcServerInfo"></param>
    /// <returns></returns>
    private bool FeatchServerAddress(string version)
    {
        var conf = XApp.GetSingleton().GetAppConfigure();
        if (conf)
        {
            if (!string.IsNullOrEmpty(conf.authUrl))
            {
                XRpcServerInfo server = new XRpcServerInfo() {
                    host = conf.authUrl, type = XRpcType.Http
                };

                // parse redirect url, get ip and port
                if (TryParseRedirect(conf.authUrl, out string addr, out int port))
                {
                    server.addr = addr;
                    server.port = port;
                }

                // create http server
                if (!XNetRpc.TryCreateServer(server, out var session))
                    return false;

                var host = new Host()
                {
                    username = SystemInfo.deviceUniqueIdentifier,
                    password = SystemInfo.deviceName,
                    version = version
                };
                // req game server info
                Rpc.ReqHost(host, (code, response) => {
                    DoHost(code, response);
                });
            }
        }

        return false;
    }

    /// <summary>
    /// request login
    /// </summary>
    /// <param name="req"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    private bool Login(HostResponse response)
    {
        if (!TryParseRedirect(response.host, out string addr, out int port))
            return false;

        // we must be destroy old http server
        XNetRpc.DestroyServer(XRpcType.Http);

        // create new http server
        var rpcServerInfo = new XRpcServerInfo
        {
            type = XRpcType.Http,
            host = response.host,
            addr = addr,
            port = port
        };
        if (!XNetRpc.TryCreateServer(rpcServerInfo, out XNetSession session))
            return false;

        var host = new Host()
        {
            username = SystemInfo.deviceUniqueIdentifier,
            password = SystemInfo.deviceName,
            device = SystemInfo.deviceModel,
            graphicsMemorySize = SystemInfo.graphicsMemorySize,
            graphicsDevice = SystemInfo.graphicsDeviceName,
            memoryTotal = SystemInfo.systemMemorySize,
            version = XRes.GetVersion(),
        };

        // req login to game server
        Rpc.ReqLogin(host, (code, response) => {
            DoLogin(code, response);
        });

        return true;
    }

    /// <summary>
    /// Try parse redirect
    /// </summary>
    /// <param name="host"></param>
    /// <param name="addr"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    private bool TryParseRedirect(string host, out string addr, out int port)
    {
        addr = string.Empty;
        port = 0;

        string[] splits = host.Split("//");
        if (splits.Length < 2)
            return false;

        var head = splits[0];
        var text = splits[1];

        var arrs = text.Split(':');
        if (arrs.Length < 2)
            return false;

        addr = head + "//" + arrs[0];
        port = int.Parse(arrs[1]);

        return true;
    }
}