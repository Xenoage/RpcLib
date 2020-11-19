using System.Collections.Concurrent;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Interface for saving call that should be retried until they were
    /// executed on the remote peer or finally failed because of a
    /// non-<see cref="RpcFailureTypeEx.IsRetryable"/> failure.
    /// See also <see cref="RpcRetryStrategy"/>.
    /// 
    /// When the implementation of this interface is using persistent storage (file, database, ...),
    /// the retry strategy still works after reboot.
    /// Implementations of this class must be thread-safe.
    /// See <see cref="JsonFileRpcBacklog"/> for a simple example, just for demo purposes.
    /// 
    /// The method names and signatures were chosen to be similar to .NET's
    /// <see cref="ConcurrentQueue"/> implementation.
    /// </summary>
    public interface IRpcBacklog {

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
        /// Adds the given call to the end of this queue.
        /// When the retry strategy of the call allows only a single method call with this name
        /// (<see cref="RpcRetryStrategy.RetryLatest"/>), all other calls with the
        /// same method name are removed from the queue.
        /// </summary>
        /// <param name="targetPeerID">The ID of the called peer, i.e. the client ID or null for the server</param>
        void Enqueue(string? targetPeerID, RpcCall call);

    }

}
