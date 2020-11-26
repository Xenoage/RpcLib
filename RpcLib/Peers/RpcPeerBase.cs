using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Base class for both the <see cref="RpcServer"/> and the <see cref="RpcClient"/>.
    /// </summary>
    public abstract class RpcPeerBase : IRpcPeer {

        /// <summary>
        /// GOON
        /// </summary>
        protected abstract RpcPeerEngine GetPeer(string? targetPeerID);

        public async Task<T> ExecuteOnRemotePeer<T>(string? remotePeerID,
                string methodName, params object[] methodParameters) {
            // Only calls to the server are supported
            if (remotePeerID != null)
                throw new RpcException(RpcFailure.Other("Clients can only call the server"));
            // Get 
            // Enqueue and await call
            // GOON
            return default!;
        }

    }

}
