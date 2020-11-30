using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Connections {

    /// <summary>
    /// Simulated RPC connection for testing purposes, acting as a remote peer that
    /// accepts and sends number division commands.
    /// If requested in the constructor, new calc tasks will be continously available on the receive queue,
    /// until the connection is stopped or <see cref="StopReceivingDivs"/> is called.
    /// The execution time (time before a call result is available on the receiving queue) can be
    /// set during runtime by calling <see cref="SetExecutionTimeMs"/>.
    /// Both the sent and received calculation tasks and their results are collected in
    /// <see cref="SentDivs"/> and <see cref="ReceivedDivs"/>, so that a unit test
    /// can assert that everything went right.
    /// </summary>
    public class DivMockRpcConnection : IRpcConnection {

        public DivMockRpcConnection(bool enableReceivingCalls) {
            if (enableReceivingCalls) {
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
        }

        public bool IsOpen() =>
            isRunning;

        public async Task<RpcMessage?> Receive(CancellationToken cancellationToken) {
            while (isRunning) {
                if (receiving.TryDequeue(out var msg)) {
                    if (msg.IsRpcMethod())
                        ReceivedDivs.Enqueue(Div.FromMethod(msg.DecodeRpcMethod()));
                    return msg;
                }
                await Task.Delay(25);
            }
            return null;
        }

        public async Task Send(RpcMessage message, CancellationToken cancellationToken) {
            if (message.IsRpcMethod()) {
                // Peer sent us a div task. "Execute" the method (by waiting executionTimeMs) and
                // add the result to the receiving queue and log the div.
                var div = Div.FromMethod(message.DecodeRpcMethod());
                Log.Trace($"Div {div.methodID} sent to DivMock");
                SentDivs.Enqueue(div);
                Action setResult = () => {
                    div.result = div.ComputeExpectedResult();
                    receiving.Enqueue(RpcMessage.Encode(div.result));
                    Log.Trace($"Div {div.methodID} executed on DivMock");
                };
                if (executionTimeMs > 0)
                    _ = Task.Delay(executionTimeMs).ContinueWith(_ => setResult());
                else
                    setResult(); // Immediate execution on the same thread
            }
            else if (message.IsRpcResult()) {
                // Peer sent the result from a div call. Log it.
                var result = message.DecodeRpcResult();
                var div = ReceivedDivs.AsEnumerable().FirstOrDefault(it => it.methodID == result.MethodID);
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

        public void SetExecutionTimeMs(int executionTimeMs) =>
            this.executionTimeMs = executionTimeMs;

        public ConcurrentQueue<Div> SentDivs { get; } = new ConcurrentQueue<Div>(); // "sent" from the view of the peer (i.e. in Send)
        public ConcurrentQueue<Div> ReceivedDivs { get; } = new ConcurrentQueue<Div>(); // "received" from the view of the peer (i.e. in Receive)

        private ConcurrentQueue<RpcMessage> receiving = new ConcurrentQueue<RpcMessage>();
        private bool isRunning = true;
        private bool isReceivingMoreDivs = true;
        private int executionTimeMs = 0;

    }

}
