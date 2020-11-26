using System.Threading.Tasks;
using Xenoage.RpcLib.Channels;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Main class of the RPC library on the client side.
    /// 
    /// The "client" is the peer which connects to the server peer over HTTPS to
    /// establish a websocket connection. This does not mean that only the client
    /// may send method calls. These are possible in both directions, once the
    /// communication channel is set up.
    /// </summary>
    public class RpcClient : RpcPeerBase {


        // The RPC engine
        private RpcPeerEngine? peer;
        // The open websocket channel
        private WebSocketRpcChannel? channel;

        protected override RpcPeerEngine GetPeer(string? targetPeerID) {
            // GOON
            throw new System.NotImplementedException();
        }
    }

}
