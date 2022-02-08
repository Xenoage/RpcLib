// Auto-generated code by Xenoage.RpcLib.Generator

namespace Chat;

using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

public abstract class ChatClientRpcBase : RpcMethods, IChatClientRpc {

    // Abstract method implementations
    public abstract Task ReceiveMessage(string message, string username);

    /// <summary>
    /// Mapping of <see cref="RpcMethod"/> to real method calls (just boilerplate code).
    /// </summary>
    public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {
        "ReceiveMessage" => ReceiveMessage(method.GetParam<string>(0), method.GetParam<string>(1)).Serialize(),
        _ => null
    };

}