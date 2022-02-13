using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

namespace Chat {

    // TODO: Auto-generate
    public class ChatClientRpcStub : RpcMethodsStub, IChatClientRpc {

        public ChatClientRpcStub(RpcServer localServer, string remoteClientID) : base(localServer, remoteClientID) {
        }

        public Task ReceiveMessage(ChatMessage message) =>
            ExecuteOnRemotePeer("ReceiveMessage", message);

        public Task OnMessageReceived(ChatMessage message) =>
            ExecuteOnRemotePeer("!.MessageReceived", message);

    }

}
