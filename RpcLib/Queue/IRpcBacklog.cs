using System.Collections.Concurrent;
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
    /// Implementations of this class must be thread-safe.
    /// 
    /// See <see cref="MemoryRpcBacklog"/> for a production-ready in-memory implementation,
    /// or <see cref="JsonFileRpcBacklog"/> for a simple example of a persistent backlog, just for demo purposes.
    /// 
    /// The method names and signatures were chosen to be similar to .NET's
    /// <see cref="ConcurrentQueue"/> implementation.
    /// </summary>
    public interface IRpcBacklog {

        /// <summary>
        /// True, iff this implementation persists the queue even over program restarts.
        /// </summary>
        bool IsPersistent { get; }

        /// <summary>
        /// Tries to return the call from the beginning of this queue without removing it.
        /// Returns true and sets the result parameter, if there is one, otherwise returns false.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        bool TryPeek(string? targetPeerID, out RpcCall result);

        /// <summary>
        /// Tries to remove and return the call at the beginning of this queue.
        /// Returns true and sets the result parameter, if there is one, otherwise returns false.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        bool TryDequeue(string? targetPeerID, out RpcCall result);

        /// <summary>
        /// Adds the given call to the end of the queue of the call's target peer.
        /// When the retry strategy of the call allows only a single method call with this name
        /// (<see cref="RpcRetryStrategy.RetryLatest"/>), all other calls with the
        /// same method name, which are still in <see cref="RpcCallState.Enqueued"/>, are removed from the queue.
        /// </summary>
        void Enqueue(RpcCall call);

        /// <summary>
        /// Explicitly clears the whole backlog.
        /// Not used in the library itself, but useful for testing.
        /// </summary>
        void Clear();

    }

}
