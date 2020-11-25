using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Peers;

namespace Xenoage.RpcLib.Channels {

    /// <summary>
    /// RPC communication channel for testing purposes.
    /// The list of messages "to receive" are given beforehand,
    /// and sent messages are collected.
    /// </summary>
    public class ReceivingMockRpcChannel : IRpcChannel {

        public ReceivingMockRpcChannel(Queue<RpcMessage> receiving) {
            this.receiving = receiving;
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            while (isRunning) {
                if (receiving.Count > 0)
                    return receiving.Dequeue();
                else
                    await Task.Delay(25);
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

        private Queue<RpcMessage> receiving;
        private List<RpcMessage> sent = new List<RpcMessage>();
        private bool isRunning = true;

    }

}
