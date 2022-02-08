// Auto-generated code by Xenoage.RpcLib.Generator

namespace Chat;

using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

public abstract class ChatServerRpcBase : RpcMethods, IChatServerRpc {

    // Abstract method implementations
    public abstract Task SendPublicMessage(string message);
    public abstract Task<bool> SendPrivateMessage(string message, string username);

    /// <summary>
    /// Mapping of <see cref="RpcMethod"/> to real method calls (just boilerplate code).
    /// </summary>
    public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {
        "SendPublicMessage" => SendPublicMessage(method.GetParam<string>(0)).Serialize(),
        "SendPrivateMessage" => SendPrivateMessage(method.GetParam<string>(0), method.GetParam<string>(1)).Serialize(),
        _ => null
    };

}