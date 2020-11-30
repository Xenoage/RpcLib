using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Connections {

    /// <summary>
    /// Simulated RPC connection for testing purposes.
    /// Whenever a message is sent, a moment later the next of the
    /// messages "to respond", which are given beforehand, are received.
    /// </summary>
    public class RespondingMockRpcConnection : IRpcConnection {

        public RespondingMockRpcConnection(Queue<RpcMessage> responding, int responseTimeMs) {
            this.responding = responding;
            this.responseTimeMs = responseTimeMs;
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            RpcMessage? ret = null;
            while (isRunning && ret == null) {
                await semaphore.WaitAsync();
                if (allowedResponses > 0 && responding.Count > 0) {
                    ret = responding.Dequeue();
                    allowedResponses--;
                }
                semaphore.Release();
                if (ret == null)
                    await Task.Delay(50);
            }
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
