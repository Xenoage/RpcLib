using Chat;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class ChatClientRpcStub : RpcMethodsStub, IChatClientRpc {

        public ChatClientRpcStub(RpcServer localServer, string remoteClientID) : base(localServer, remoteClientID) {
        }

        public Task ReceiveMessage(string message, string username) =>
            ExecuteOnRemotePeer<bool>("ReceiveMessage", message, username);

    }

}
