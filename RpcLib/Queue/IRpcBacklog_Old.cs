using System.Collections.Concurrent;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Interface for saving calls that should be retried until they were
    /// executed on the remote peer or finally failed because of a
    /// non-<see cref="RpcFailureTypeEx.IsRetryable"/> failure.
    /// See also <see cref="RpcRetryStrategy"/>.
    /// 
    /// When the implementation of this interface is using persistent storage (file, database, ...),
    /// the retry strategy still works after reboot.
    /// Implementations of this class must be thread-safe, but keep in mind that if multiple threads
    /// access the same target peer's queue, a <see cref="Peek"/> and a following <see cref="Dequeue"/>
    /// may return different items. Thus, only one thread should process one target peer at a time.
    /// 
    /// See <see cref="MemoryRpcBacklog_Old"/> for a production-ready in-memory implementation,
    /// or <see cref="JsonFileRpcBacklog_Old"/> for a simple example of a persistent backlog, just for demo purposes.
    /// 
    /// The method names and signatures were chosen to be similar to .NET's
    /// <see cref="ConcurrentQueue"/> implementation.
    /// </summary>
    public interface IRpcBacklog_Old {

        /// <summary>
        /// True, iff this implementation persists the queues even over program restarts.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// Returns the call from the beginning of the queue of the given peer without removing it.
        /// If there is one, null is returned.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        Task<RpcCall?> Peek(string? targetPeerID);

        /// <summary>
        /// Removes and returns the call at the beginning of the queue of the given peer.
        /// If there is none, null is returned.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        Task<RpcCall?> Dequeue(string? targetPeerID);

        /// <summary>
        /// Adds the given call to the end of the queue of the call's target peer.
        /// When the retry strategy of the call allows only a single method call with this name
        /// (<see cref="RpcRetryStrategy.RetryLatest"/>), all other calls with the
        /// same method name, which are still in <see cref="RpcCallState_Old.Enqueued"/>, are removed from the queue.
        /// </summary>
        Task Enqueue(RpcCall call);

        /// <summary>
        /// Gets the number of currently enqueued calls of the given peer.
        /// </summary>
        Task<int> GetCount(string? targetPeerID);

    }

}
