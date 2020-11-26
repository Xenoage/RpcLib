using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Interface for the main RPC library class, used to talk to the library
    /// from the application. This interface is both used for the
    /// <see cref="RpcClient"/> and the <see cref="RpcServer"/>.
    /// </summary>
    public interface IRpcPeer {

        /// <summary>
        /// Runs the given RPC method on the given remote peer as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        public Task<T> ExecuteOnRemotePeer<T>(string? remotePeerID,
            string methodName, params object[] methodParameters);

    }

}