using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class ChatServerRpcStub : RpcMethodsStub, IChatServerRpc {

        public ChatServerRpcStub(RpcClient localClient) : base(localClient) {
        }

        public Task<bool> SendPrivateMessage(string message, string username) =>
            ExecuteOnRemotePeer<bool>("SendPrivateMessage", message, username);

        public Task SendPublicMessage(string message) =>
            ExecuteOnRemotePeer("SendPrivateMessage", message);
    }

}
