using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Queue of calls to a specific <see cref="TargetPeerID"/>
    /// (one of the clients or the server).
    /// 
    /// When a <see cref="Backlog"/> is registered, enqueued calls are also stored there,
    /// and dequeued (and obsolete) calls are removed.
    /// When the class is initialized, it is filled with the calls which are still in the
    /// <see cref="Backlog"/>, if any.
    /// 
    /// This class is thread-safe.
    /// </summary>
    public class RpcQueue {

        /// <summary>
        /// ID of the client for which this queue collects calls, or null for the server.
        /// </summary>
        public string? TargetPeerID { get; private set; } = null;

        /// <summary>
        /// Permanent storage for retryable calls.
        /// May be null, if this feature is not required.
        /// </summary>
        public IRpcBacklog? Backlog { get; private set; } = null;

        /// <summary>
        /// Creates a new queue for the given client ID or null for the server,
        /// using the optional backlog implementation for permanent storage of remaining calls.
        /// </summary>
        public static async Task<RpcQueue> Create(string? targetPeerID, IRpcBacklog? backlog) {
            var ret = new RpcQueue(targetPeerID, backlog);
            if (backlog != null) {
                // Restore queue from backlog
                await ret.semaphore.WaitAsync();
                foreach (var call in await backlog.ReadAll(targetPeerID)) {
                    call.ResetStartTimeAndResult();
                    ret.queue.Enqueue(call);
                }
                ret.semaphore.Release();
            }
            return ret;
        }

        /// <summary>
        /// Use <see cref="Create"/> for creating instances of this class.
        /// </summary>
        private RpcQueue(string? targetPeerID, IRpcBacklog? backlog) {
            TargetPeerID = targetPeerID;
            Backlog = backlog;
        }

        /// <summary>
        /// Returns the number of calls in the queue.
        /// </summary>
        public int Count =>
            queue.Count;

        /// <summary>
        /// Gets the call from the beginning of this queue without removing it,
        /// or returns null if the queue is empty.
        /// </summary>
        public async Task<RpcCall?> Peek() {
            await semaphore.WaitAsync();
            var call = queue.Count > 0 ? queue.Peek() : null;
            semaphore.Release();
            return call;
        }

        /// <summary>
        /// Removes and returns the call from the beginning of this queue.
        /// Call this method, when the RPC call has been finally finished
        /// (i.e. no failure happened, which allows retrying, or retrying is not enabled for this call).
        /// It will also be removed from the <see cref="Backlog"/>, if there is any.
        /// </summary>
        public async Task<RpcCall?> Dequeue() {
            await semaphore.WaitAsync();
            var call = queue.Dequeue();
            if (Backlog != null && call.IsRetryable()) {
                // Remove from backlog
                await Backlog.RemoveByMethodID(TargetPeerID, call.Method.ID);
            }
            semaphore.Release();
            return call;
        }

        /// <summary>
        /// Adds the given call to the end of this queue.
        /// If it is a retryable call, it will also be saved to the <see cref="Backlog"/>, if there is any,
        /// and it will also remove obsolete calls (see <see cref="RpcRetryStrategy.RetryLatest"/>) from there.
        /// </summary>
        public async Task Enqueue(RpcCall call) {
            // Check target peer ID
            if (TargetPeerID != call.RemotePeerID)
                throw new ArgumentException("Target peer ID does not match");
            await semaphore.WaitAsync();
            queue.Enqueue(call);
            if (Backlog != null && call.IsRetryable()) {
                // Remove obsolete calls
                if (call.RetryStrategy == RpcRetryStrategy.RetryLatest)
                    await Backlog.RemoveByMethodName(TargetPeerID, call.Method.Name);
                // Add to backlog
                await Backlog.Add(call);
            }
            semaphore.Release();
        }

        // Queue of calls. Does not have to be thread-safe, because we protect it using the following semaphore.
        private Queue<RpcCall> queue = new Queue<RpcCall>();
        // Semaphore which allows only one thread to enter it at the same time. The others will wait.
        private SemaphoreSlim semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    }

}
