using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Connections;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Queue;
using Xenoage.RpcLib.Utils;
using static Xenoage.RpcLib.Utils.CoreUtils;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Bidirectional channel for peer-to-peer communication.
    /// For each connection between a client and the server, one instance of this
    /// class is running both on the client and on the server. Thus, the server has multiple
    /// instances of this class running, while each client has exactly one.
    /// 
    /// It sends calls to the other side (server to client, or vice versa) and
    /// receives their response, and it receives calls from the other side,
    /// executes them and sends the responses.
    /// 
    /// This is done over a given established connection, normally a
    /// <see cref="WebSocketRpcConnection"/>. When this connection is closed, it must be
    /// reestablished, i.e. a new instance of this class has to be launched, using the
    /// new connection.
    /// 
    /// This class is thread-safe, i.e. the method <see cref="Run"/> can be called from
    /// everywhere and anytime.
    /// </summary>
    public class RpcChannel {
        
        /// <summary>
        /// Information on the connected remote peer.
        /// </summary>
        public RpcPeerInfo RemotePeer { get; private set; }


        /// <summary>
        /// Creates a new channel with the given information, already established connection,
        /// and optionally the given backlog.
        /// </summary>
        public static async Task<RpcChannel> Create(RpcPeerInfo remoteInfo, IRpcConnection connection,
                IRpcMethodExecutor executor, IRpcBacklog? backlog = null) {
            var ret = new RpcChannel(remoteInfo, connection, executor);
            ret.callsQueue = await RpcQueue.Create(remoteInfo.PeerID, backlog);
            return ret;
        }

        /// <summary>
        /// Use <see cref="Create"/> for creating new instances.
        /// </summary>
        private RpcChannel(RpcPeerInfo remotePeer, IRpcConnection connection, IRpcMethodExecutor executor) {
            RemotePeer = remotePeer;
            this.connection = connection;
            this.executor = executor;
        }

        /// <summary>
        /// Starts the communication in a loop. The task returned by this method will be completed
        /// when either the connection is closed or after <see cref="Stop"/> is called.
        /// </summary>
        public async Task Start() {
            var sendTask = SendLoop();
            var receiveTask = ReceiveLoop();
            await Task.WhenAll(sendTask, receiveTask);
        }

        /// <summary>
        /// Runs the sending operations in a loop, as long as the websocket is open.
        /// It is sending both the results from remote calls and its own calls.
        /// It also handles closing requests from this side (triggered by calling <see cref="Stop"/>).
        /// </summary>
        private async Task SendLoop() {
            try {
                var messagePart = new StringBuilder();
                while (connection.IsOpen()) {
                    bool didSomething = false;
                    // Result in the queue? Then send it.
                    if (resultsQueue.TryDequeue(out var queuedResult)) {
                        Log.Trace($"Sending result {queuedResult.MethodID} to {RemotePeer}");
                        await connection.Send(RpcMessage.Encode(queuedResult), cancellationToken.Token);
                        didSomething = true;
                    }
                    // Current call ran into a timeout?
                    if (currentCall != null && currentCall.Result == null &&
                            currentCall.StartTime + GetTimeoutMs(currentCall) < TimeNowMs()) {
                        currentCall.Result = RpcResult.Timeout(currentCall.Method.ID);
                    }
                    // Current call finished?
                    if (currentCall != null && currentCall.Result is RpcResult result) {
                        string logMsg = $"Method {currentCall.Method.ID} {currentCall.Method.Name} finished " +
                            (result.Failure != null ? "with failure " + result.Failure?.Type : "successfully");
                        if (result.IsRetryNeeded() && currentCall.IsRetryable()) {
                            Log.Trace(logMsg + ", trying it again");
                            currentCall.ResetStartTimeAndResult();
                        } else {
                            var discarded = await callsQueue.Dequeue();
                            Log.Trace(logMsg + $", dequeuing [{discarded?.Method.ID}]");
                        }
                        if (openCalls.TryGetValue(result.MethodID, out var callExecution))
                            callExecution.Finish(result);
                        currentCall = null; // Take next call from queue
                    }
                    // Next call already queued? Then see if we can send it already.
                    if (currentCall == null && await callsQueue.Peek() is RpcCall call) {
                        // Send it. Do not dequeue it yet, only after is has been finished.
                        Log.Trace($"Sending method {call.Method.ID} {call.Method.Name} to {RemotePeer}");
                        currentCall = call;
                        await connection.Send(RpcMessage.Encode(call.Method), cancellationToken.Token);
                        didSomething = true;
                    }
                    // Close nicely, when locally requested
                    if (cancellationToken.IsCancellationRequested) {
                        await connection.Close();
                        didSomething = true;
                    }
                    // When we had something to do, immediately continue. Otherwise, wait a short moment
                    // or until we get notified that the next item is here
                    if (false == didSomething) {
                        sendingWaiter = CreateAsyncTaskCompletionSource<bool>(); // async continuation is crucial
                        await Task.WhenAny(Task.Delay(100), sendingWaiter.Task);
                    }
                }
            } catch (Exception ex) {
                Log.Debug($"Unexpectedly closed connection to {RemotePeer}: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs the receiving operations in a loop, as long as the websocket is open.
        /// It is receiving both the results of its own calls and the calls sent from the remote side.
        /// It also handles closing requests from the remote side.
        /// </summary>
        private async Task ReceiveLoop() {
            try {
                while (connection.IsOpen()) {
                    // Receive call or response from remote side
                    Log.Trace($"Listening...");
                    var message = await connection.Receive(cancellationToken.Token);
                    if (message != null)
                        HandleReceivedMessage(message);
                }
            } catch (Exception ex) {
                ;
                // Websocket exception is already logged in the sending loop
            }
        }

        private void HandleReceivedMessage(RpcMessage message) {
            try {
                string logFrom = $"from {RemotePeer}, {message.Data.Length} bytes";
                if (message.IsRpcMethod()) {
                    // Received a method call. Execute it immediately and enqueue the result.
                    var method = message.DecodeRpcMethod();
                    Log.Trace($"Receiving method {method.ID} {method.Name} {logFrom}");
                    // Do not await the method execution, since it could require some time and we do not
                    // want to block the receiving loop during this time.
                    _ = Task.Run(async () => {
                        var result = new RpcResult { MethodID = method.ID };
                        try {
                            result.ReturnValue = await executor.Execute(method, RemotePeer);
                        } catch (RpcException ex) {
                            result.Failure = ex.Failure;
                        } catch (Exception ex) {
                            result.Failure = new RpcFailure {
                                Type = RpcFailureType.RemoteException, // Not retryable; the command failed on caller side
                                Message = ex.Message // Some information for identifying the problem
                            };
                        }
                        Log.Trace($"Method executed, result failure type is " + result.Failure?.Type);
                        resultsQueue.Enqueue(result);
                        // Let the sending loop respond immediately
                        sendingWaiter.TrySetResult(true);
                    });
                } else if (message.IsRpcResult()) {
                    // Received a return value
                    var result = message.DecodeRpcResult();
                    Log.Trace($"Receiving result {result.MethodID} {logFrom}" +
                        (result.Failure != null ? $" with failure {result.Failure.Type}" : ""));
                    // Set the result
                    if (result.MethodID == currentCall?.Method.ID)
                        currentCall.Result = result;
                    else
                        throw new Exception("Out of order: Received return value for non-open call");
                    sendingWaiter.TrySetResult(true);

                } else {
                    // Unsupported message
                    Log.Trace($"Unsupported message {logFrom}");
                }
            } catch (Exception ex) {
                Log.Debug($"Problem when handling message from {RemotePeer}: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops the communication.
        /// </summary>
        public void Stop() {
            cancellationToken.Cancel();
        }

        /// <summary>
        /// Runs the given RPC call on the remote side as soon as possible.
        /// The returned task is completed, when the call has been finished either
        /// successfully or when it has failed. While retryable calls will be retried
        /// as soon as possible, the returned task already returns when any case of
        /// failure has happened, i.e. before the first retry attempt. Thus, return
        /// values of retried calls can not be received by the caller.
        /// </summary>
        public async Task<RpcResult> Run(RpcCall call) {
            if (call.RemotePeerID != RemotePeer.PeerID)
                throw new ArgumentException("Call is not for this peer");
            // Enqueues the call and awaits the result
            Log.Trace($"Enqueuing call {call.Method.ID} {call.Method.Name}");
            call.ResetStartTimeAndResult();
            var execution = new RpcCallExecution(call);
            openCalls.TryAdd(call.Method.ID, execution);
            await callsQueue.Enqueue(call);
            sendingWaiter.TrySetResult(true);
            // Wait for the result, whether successful, failed or timeout
            var result = await execution.AwaitResult(GetTimeoutMs(call));
            openCalls.TryRemove(call.Method.ID, out var _);
            return result;
        }

        private int GetTimeoutMs(RpcCall call) =>
            call.TimeoutMs ?? executor.DefaultOptions.TimeoutMs;

        // The established connection to the remote peer
        private IRpcConnection connection;
        // Run the actual C# implementations of the RPC methods
        private IRpcMethodExecutor executor;
        // Queue of responses to send to the other side
        private ConcurrentQueue<RpcResult> resultsQueue = new ConcurrentQueue<RpcResult>();
        // Queue of calls to send to the other side
        private RpcQueue callsQueue = null!; // Async set in Create
        // The sending task waits a short moment in the loop to reduce CPU load.
        // Complete this task to immediately start the next round. Using a TaskCompletionSource
        // seems to be much faster than using a CancellationTokenSource (while debugging, experienced ~2 ms vs ~13 ms)
        private TaskCompletionSource<bool> sendingWaiter =
            CreateAsyncTaskCompletionSource<bool>(); // async continuation is crucial
        // The execution state of the calls in the Run method, where still a caller is waiting for the result.
        private ConcurrentDictionary<ulong, RpcCallExecution> openCalls =
            new ConcurrentDictionary<ulong, RpcCallExecution>();
        // The current call (only one is currently executing on the remote side at a time)
        private RpcCall? currentCall = null;
        // Cancellation token for stopping the loop
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();


    }

}
