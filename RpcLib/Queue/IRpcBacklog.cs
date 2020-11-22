using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Implementations of this interface are used to persistently store
    /// retryable calls. This ensures, that an enqueued call still exists and
    /// is sent when the program is restarted (e.g. because it has crashed before).
    /// 
    /// See <see cref="JsonFileRpcBacklog"/> for a simple, yet inefficient example,
    /// just for demo purposes. For your own implementation, choose a performant
    /// storage, like a database.
    /// 
    /// The methods must be implemented thread-safe, because multiple target peers
    /// may be queried or modified at the same time.
    /// </summary>
    public interface IRpcBacklog {

        /// <summary>
        /// Returns the whole queue stored for the given target peer.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        public Task<Queue<RpcCall>> ReadAll(string? targetPeerID);

        /// <summary>
        /// Saves the given call.
        /// </summary>
        public Task Add(RpcCall call);

        /// <summary>
        /// Removes the call of the given target peer with the given method invocation ID, if it exists.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        /// <param name="ID">The <see cref="RpcMethod.ID"/> of the call to remove</param>
        public Task RemoveByMethodID(string? targetPeerID, ulong methodID);

        /// <summary>
        /// Removes all calls of the given target peer with the given method name.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        /// <param name="methodName"><see cref="RpcMethod.Name"/> of the calls to remove</param>
        public Task RemoveByMethodName(string? targetPeerID, string methodName);

    }

}
