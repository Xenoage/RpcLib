using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Channels {

    /// <summary>
    /// RPC communication channel for testing purposes.
    /// The list of messages "to receive" are given beforehand,
    /// including the time in ms they require to "execute",
    /// and sent messages are collected.
    /// </summary>
    public class ReceivingMockRpcChannel : IRpcChannel {

        public ReceivingMockRpcChannel(Queue<(RpcMessage, int)> receiving) {
            this.receiving = receiving;
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            while (isRunning) {
                if (receiving.Count > 0) {
                    var (msg, timeMs) = receiving.Dequeue();
                    await Task.Delay(timeMs);
                    return msg;
                } else {
                    await Task.Delay(25);
                }
            }
            return null;
        }

        public Task Send(RpcMessage message, CancellationToken cancellationToken) {
            sent.Add(message);
            return Task.CompletedTask;
        }

        public Task Close() {
            isRunning = false;
            return Task.CompletedTask;
        }

        public List<RpcMessage> SentMessages => sent;

        private Queue<(RpcMessage, int)> receiving;
        private List<RpcMessage> sent = new List<RpcMessage>();
        private bool isRunning = true;

    }

}
