// Auto-generated code by Xenoage.RpcLib.Generator

namespace Chat;

using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

public class ChatClientRpcStub : RpcMethodsStub, IChatClientRpc {

    /// <summary>
    /// Use this constructor for stubs on the server side,
    /// i.e. from server-to-client calls when the real implementation of the interface (RpcMethods) is on the client side.
    /// </summary>
    public ChatClientRpcStub(RpcServer localServer, string remoteClientID) : base(localServer, remoteClientID) {
    }

    /// <summary>
    /// Use this constructor for stubs on the client side,
    /// i.e. from client-to-server calls when the real implementation of the interface (RpcMethods) is on the server side.
    /// </summary>
    public ChatClientRpcStub(RpcClient localClient) : base(localClient) {
    }

    public Task ReceiveMessage(string message, string username) =>
        ExecuteOnRemotePeer("ReceiveMessage", message, username);

}