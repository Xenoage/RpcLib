using System.Threading.Tasks;
using Xenoage.RpcLib.Connections;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Main class of the RPC library on the client side.
    /// 
    /// The "client" is the peer which connects to the server peer over HTTPS to
    /// establish a websocket connection. This does not mean that only the client
    /// may send method calls. These are possible in both directions, once the
    /// connection is established.
    /// </summary>
    public class RpcClient : RpcPeerBase {

        // The RPC engine
        private RpcPeerEngine? peer;
        // The open websocket connection
        private WebSocketRpcConnection? connection;

        protected override RpcPeerEngine GetPeer(string? targetPeerID) {
            // GOON
            throw new System.NotImplementedException();
        }
    }

}
