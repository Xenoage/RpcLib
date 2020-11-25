using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;

namespace Xenoage.RpcLib.Channels {

    /// <summary>
    /// RPC communication channel for testing purposes, simulating a remote peer that
    /// accepts and sends number division commands.
    /// Contionously, new calc tasks are available on the receive queue.
    /// Both the sent and received calculation tasks and their results are collected in
    /// <see cref="SentDivs"/> and <see cref="ReceivedDivs"/>, so that a unit test
    /// can assert that everything went right.
    /// </summary>
    public class DivMockRpcChannel : IRpcChannel {

        public DivMockRpcChannel() {
            // While running, create calculation tasks which the peer can receive
            Task.Run(async () => {
                ulong nextID = 0;
                while (isRunning && isReceivingMoreDivs) {
                    var div = Div.CreateNew(nextID++);
                    receiving.Enqueue(RpcMessage.Encode(div.ToMethod()));
                    await Task.Delay(50);
                }
            });
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            while (isRunning) {
                if (receiving.TryDequeue(out var msg)) {
                    if (msg.IsRpcMethod())
                        ReceivedDivs.Add(Div.FromMethod(msg.DecodeRpcMethod()));
                    return msg;
                }
                await Task.Delay(25);
            }
            return null;
        }

        public async Task Send(RpcMessage message, CancellationToken cancellationToken) {
            if (message.IsRpcMethod()) {
                // Received div task. Wait a short moment and add the result
                // to the receiving queue and log the div.
                var div = Div.FromMethod(message.DecodeRpcMethod());
                div.result = div.ComputeExpectedResult();
                receiving.Enqueue(RpcMessage.Encode(div.result));
                SentDivs.Add(div);
                Log.Trace("DivMock sent Div " + div.methodID);
            }
            else if (message.IsRpcResult()) {
                // Peer sent the result from a div call. Log it.
                var result = message.DecodeRpcResult();
                var div = ReceivedDivs.Find(it => it.methodID == result.MethodID);
                if (div == null)
                    Debug.WriteLine("Unexpected method ID");
                else
                    div.result = result;
            }
        }

        public Task Close() {
            isRunning = false;
            return Task.CompletedTask;
        }

        public void StopReceivingDivs() {
            isReceivingMoreDivs = false;
        }

        public List<Div> SentDivs { get; } = new List<Div>(); // "sent" from the view of the peer (i.e. in Send)
        public List<Div> ReceivedDivs { get; } = new List<Div>(); // "received" from the view of the peer (i.e. in Receive)

        private ConcurrentQueue<RpcMessage> receiving = new ConcurrentQueue<RpcMessage>();
        private bool isRunning = true;
        private bool isReceivingMoreDivs = true;

    }

}
