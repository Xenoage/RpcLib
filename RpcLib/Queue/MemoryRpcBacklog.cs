using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Implementation of a <see cref="IRpcBacklog_Old"/>,
    /// using in-memory <see cref="LinkedList"/>s.
    /// This means, that all enqueued retryable calls will be lost as soon
    /// the program is closed. When they should be retained over program restarts,
    /// use a permanent storage solution as demonstrated in <see cref="JsonFileRpcBacklog_Old"/>.
    /// </summary>
    public class MemoryRpcBacklog : IRpcBacklog_Old {

        public bool IsPersistent => false;

        public async Task<RpcCall?> Peek(string? targetPeerID) =>
            await PeekOrDequeue(targetPeerID, dequeue: false);

        public async Task<RpcCall?> Dequeue(string? targetPeerID) =>
            await PeekOrDequeue(targetPeerID, dequeue: true);

        private async Task<RpcCall?> PeekOrDequeue(string? targetPeerID, bool dequeue) {
            await semaphore.WaitAsync();
            RpcCall? result = null;
            var queue = GetQueue(targetPeerID);
            if (queue.Count > 0) {
                result = queue.First!.Value;
                if (dequeue)
                    queue.RemoveFirst();
            }
            semaphore.Release();
            return result;
        }

        public async Task Enqueue(RpcCall call) {
            await semaphore.WaitAsync();
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
                queue.RemoveAll(it => it.Method.Name == call.Method.Name /* && it.State == RpcCallState.Enqueued */);
            }
            queue.AddLast(call);
            semaphore.Release();
        }

        public async Task<int> GetCount(string? targetPeerID) {
            await semaphore.WaitAsync();
            int count = GetQueue(targetPeerID).Count;
            semaphore.Release();
            return count;
        }

        private LinkedList<RpcCall> GetQueue(string? targetPeerID) {
            string key = targetPeerID ?? "";
            if (queues.TryGetValue(key, out var ret))
                return ret;
            var queue = new LinkedList<RpcCall>();
            queues.Add(key, queue);
            return queue;
        }

        // A queue for each target peer. The key is the client ID or "" for the server.
        private Dictionary<string, LinkedList<RpcCall>> queues = new Dictionary<string, LinkedList<RpcCall>>();

        private static SemaphoreSlim semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    }

}
