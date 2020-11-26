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

        public RpcContext(RpcPeerInfo remotePeer) {
            RemotePeer = remotePeer;
        }

    }

}
