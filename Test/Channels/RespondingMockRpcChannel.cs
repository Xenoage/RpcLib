using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Peers;

namespace Xenoage.RpcLib.Channels {

    /// <summary>
    /// RPC communication channel for testing purposes.
    /// Whenever a message is sent, a moment later the next of the
    /// messages "to respond", which are given beforehand, are received.
    /// </summary>
    public class RespondingMockChannel : IRpcChannel {

        public RespondingMockChannel(Queue<RpcMessage> responding, int responseTimeMs) {
            this.responding = responding;
            this.responseTimeMs = responseTimeMs;
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            RpcMessage? ret = null;
            await semaphore.WaitAsync();
            if (isRunning && allowedResponses > 0 && responding.Count > 0) {
                ret = responding.Dequeue();
                allowedResponses--;
            }
            semaphore.Release();
            return ret;
        }

        public Task Send(RpcMessage message, CancellationToken cancellationToken) {
            _ = Task.Run(async () => {
                await Task.Delay(responseTimeMs);
                await semaphore.WaitAsync();
                allowedResponses++;
                semaphore.Release();
            });
            return Task.CompletedTask;
        }

        public Task Close() {
            isRunning = false;
            return Task.CompletedTask;
        }

        private Queue<RpcMessage> responding;
        private int responseTimeMs;
        private int allowedResponses = 0;
        private bool isRunning = true;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    }

}
