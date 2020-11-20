using System.Collections.Generic;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Implementation of a <see cref="IRpcBacklog"/>,
    /// using in-memory <see cref="LinkedList"/>s.
    /// This means, that all enqueued retryable calls will be lost as soon
    /// the program is closed. When they should be retained over program restarts,
    /// use a permanent storage solution as demonstrated in <see cref="JsonFileRpcBacklog"/>.
    /// </summary>
    public class MemoryRpcBacklog : IRpcBacklog {

        public bool IsPersistent => false;

        public bool TryPeek(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: false, out result);

        public bool TryDequeue(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: true, out result);

        private bool TryPeekOrDequeue(string? targetPeerID, bool dequeue, out RpcCall result) {
            lock (syncLock) {
                var queue = GetQueue(targetPeerID);
                if (queue.Count > 0) {
                    result = queue.First!.Value;
                    if (dequeue)
                        queue.RemoveFirst();
                    return true;
                } else {
                    result = default!;
                    return false;
                }
            }
        }

        public void Enqueue(RpcCall call) {
            lock (syncLock) {
                var queue = GetQueue(call.TargetPeerID);
                // Apply strategy
                var strategy = call.RetryStrategy;
                if (strategy == null || strategy == RpcRetryStrategy.None) {
                    // No retry strategy chosen. This method should not have been called at all. Do nothing.
                    return;
                } else if (strategy == RpcRetryStrategy.Retry) {
                    // No preparation needed; just enqueue this call
                } else if (strategy == RpcRetryStrategy.RetryLatest) {
                    // Remove all preceding method calls with this name, if still in enqueued state
                    queue.RemoveAll(it => it.Method.Name == call.Method.Name && it.State == RpcCallState.Enqueued);
                }
                queue.AddLast(call);
            }
        }

        public void Clear() {
            lock (syncLock) {
                queues.Clear();
            }
        }

        private LinkedList<RpcCall> GetQueue(string? targetPeerID) {
            string key = targetPeerID ?? "";
            lock (syncLock) {
                if (queues.TryGetValue(key, out var ret))
                    return ret;
                var queue = new LinkedList<RpcCall>();
                queues.Add(key, queue);
                return queue;
            }
        }

        // A queue for each target peer. The key is the client ID or "" for the server.
        private Dictionary<string, LinkedList<RpcCall>> queues = new Dictionary<string, LinkedList<RpcCall>>();

        private readonly object syncLock = new object();

    }

}
