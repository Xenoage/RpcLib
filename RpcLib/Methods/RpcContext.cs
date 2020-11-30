using System.Collections.Generic;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Methods {

    /// <summary>
    /// The calling context within a local <see cref="RpcMethods"/> method call.
    /// </summary>
    public class RpcContext {

        /// <summary>
        /// Information on the calling peer. For example, the server can find out
        /// which client is calling the current method.
        /// </summary>
        public RpcPeerInfo RemotePeer { get; }

        /// <summary>
        /// On the server side, returns the current list of connected clients.
        /// On the client side, returns null.
        /// </summary>
        public List<RpcPeerInfo>? Clients { get; }

        public static RpcContext OnClient(RpcPeerInfo serverPeer) =>
            new RpcContext(serverPeer, clients: null);

        public static RpcContext OnServer(RpcPeerInfo clientPeer, List<RpcPeerInfo> clients) =>
            new RpcContext(clientPeer, clients);

        public RpcContext(RpcPeerInfo remotePeer, List<RpcPeerInfo>? clients) {
            RemotePeer = remotePeer;
            Clients = clients;
        }

    }

}
