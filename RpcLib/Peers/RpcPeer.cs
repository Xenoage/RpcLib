using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Channels;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Queue;
using static Xenoage.RpcLib.Utils.CoreUtils;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Endpoint for the RPC traffic. For each connection between a client
    /// and the server, one instance of this class is running both on the client
    /// and on the server. That means, the server has multiple instances of this
    /// class running, while a client has exactly one.
    /// 
    /// It sends calls to the other side (server to client, or vice versa) and
    /// receives their response, and it receives calls from the other side,
    /// executes them and sends the responses.
    /// 
    /// This is done over a given communication channel, normally a
    /// <see cref="WebSocketRpcChannel"/>. When this connection is closed, it must be
    /// reestablished, i.e. a new instance of this class has to be launched, using the
    /// new connection.
    /// 
    /// This class is thread-safe, i.e. the method <see cref="Run"/> can be called from
    /// everywhere and anytime.
    /// </summary>
    public class RpcPeer {
        
        /// <summary>
        /// Information on the connected remote peer.
        /// </summary>
        public PeerInfo RemoteInfo { get; private set; }


        /// <summary>
        /// Creates a new peer with the given information, already connected websocket,
        /// and optionally the given backlog.
        /// </summary>
        public static async Task<RpcPeer> Create(PeerInfo remoteInfo, IRpcChannel channel,
                IRpcMethodExecutor executor, IRpcBacklog? backlog = null) {
            var ret = new RpcPeer(remoteInfo, channel, executor);
            ret.callsQueue = await RpcQueue.Create(remoteInfo.PeerID, backlog);
            return ret;
        }

        /// <summary>
        /// Use <see cref="Create"/> for creating new instances.
        /// </summary>
        private RpcPeer(PeerInfo remoteInfo, IRpcChannel channel, IRpcMethodExecutor executor) {
            RemoteInfo = remoteInfo;
            this.channel = channel;
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
                while (channel.IsOpen()) {
                    bool didSomething = false;
                    // Result in the queue? Then send it.
                    if (resultsQueue.TryDequeue(out var result)) {
                        Log.Trace($"Sending result {result.MethodID}");
                        await channel.Send(RpcMessage.Encode(result), cancellationToken.Token);
                        didSomething = true;
                    }
                    // Current call ran into a timeout?
                    if (currentCall != null && currentCallStartTime + GetTimeoutMs(currentCall) < TimeNowMs()) {
                        string logMsg = $"Method {currentCall.Method.ID} {currentCall.Method.Name} ran into timeout";
                        if (currentCall.IsRetryable()) {
                            Log.Trace(logMsg + ", trying it again");
                        } else {
                            Log.Trace(logMsg + ", discard it");
                            await callsQueue.Dequeue();
                        }
                        if (openCalls.TryRemove(currentCall.Method.ID, out var executingTask))
                            executingTask.Finish(RpcResult.Timeout(currentCall.Method.ID));
                        currentCall = null;
                    }
                    // Next call already queued? Then see if we can send it already.
                    if (currentCall == null && await callsQueue.Peek() is RpcCall call) {
                        // Send it. Do not dequeue it yet, only after is has been finished.
                        Log.Trace($"Sending method {call.Method.ID} {call.Method.Name}");
                        currentCall = call;
                        currentCallStartTime = TimeNowMs();
                        await channel.Send(RpcMessage.Encode(call.Method), cancellationToken.Token);
                        didSomething = true;
                    }
                    // Close nicely, when locally requested
                    if (cancellationToken.IsCancellationRequested) {
                        await channel.Close();
                        didSomething = true;
                    }
                    // When we had something to do, immediately continue. Otherwise, wait a short moment
                    // or until we get notified that the next item is here
                    if (false == didSomething) {
                        sendingWaiter = new TaskCompletionSource<bool>();
                        await Task.WhenAny(Task.Delay(100), sendingWaiter.Task);
                    }
                }
            } catch (Exception ex) {
                Log.Debug($"Unexpectedly closed connection to {RemoteInfo}: {ex.Message}");
            }
        }

        /// <summary>
        /// Runs the receiving operations in a loop, as long as the websocket is open.
        /// It is receiving both the results of its own calls and the calls sent from the remote side.
        /// It also handles closing requests from the remote side.
        /// </summary>
        private async Task ReceiveLoop() {
            try {
                while (channel.IsOpen()) {
                    // Receive call or response from remote side
                    var message = await channel.Receive(cancellationToken.Token);
                    if (message != null)
                        await HandleReceivedMessage(message);
                }
            } catch (Exception ex) {
                Log.Debug($"Unexpectedly closed connection to {RemoteInfo}: {ex.Message}");
            }
        }

        private async Task HandleReceivedMessage(RpcMessage message) {
            try {
                string logFrom = $"from {RemoteInfo}, {message.Data.Length} bytes";
                if (message.IsRpcMethod()) {
                    // Received a method call. Execute it immediately and enqueue the result.
                    var method = message.DecodeRpcMethod();
                    Log.Trace($"Receiving method {method.ID} {method.Name} {logFrom}");
                    // Do not await the method execution, since it could require some time and we do not
                    // want to block the receiving channel during this time.
                    _ = Task.Run(async () => {
                        var result = new RpcResult { MethodID = method.ID };
                        try {
                            result.ReturnValue = await executor.Execute(method);
                        } catch (Exception ex) {
                            result.Failure = new RpcFailure {
                                Type = RpcFailureType.RemoteException, // Not retryable; the command failed on caller side
                                Message = ex.Message // Some information for identifying the problem
                            };
                        }
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
                    if (result.MethodID == currentCall?.Method.ID) {
                        if (openCalls.TryRemove(result.MethodID, out var callExecution))
                            callExecution.Finish(result);
                    } else {
                        throw new Exception("Out of order: Received return value for non-open call");
                    }
                    // When it has failed, but is a retryable command, send it again.
                    // Otherwise, dequeue the call so that the next one can be started.
                    if (currentCall.IsRetryable() && result.IsRetryNeeded()) {
                        // Let it in the queue to send it again
                    } else {
                        // Finished; dequeue it.
                        await callsQueue.Dequeue();
                    }
                    // Allow the next call to start immediately
                    currentCall = null;
                    sendingWaiter.TrySetResult(true);
                } else {
                    // Unsupported message
                    Log.Trace($"Unsupported message {logFrom}");
                }
            } catch (Exception ex) {
                Log.Debug($"Problem when handling message from {RemoteInfo}: {ex.Message}");
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
            if (call.TargetPeerID != RemoteInfo.PeerID)
                throw new ArgumentException("Call is not for this peer");
            // Enqueues the call and awaits the result
            var execution = new RpcCallExecution(call);
            openCalls.TryAdd(call.Method.ID, execution);
            await callsQueue.Enqueue(call);
            sendingWaiter.TrySetResult(true);
            // Wait for the result, whether successful, failed or timeout
            var result = await execution.AwaitResult();
            return result;
        }

        private int GetTimeoutMs(RpcCall call) =>
            call.TimeoutMs ?? executor.DefaultOptions.TimeoutMs;

        // The communication channel
        private IRpcChannel channel;
        // Run the actual C# implementations of the RPC methods
        private IRpcMethodExecutor executor;
        // Queue of responses to send to the other side
        private ConcurrentQueue<RpcResult> resultsQueue = new ConcurrentQueue<RpcResult>();
        // Queue of calls to send to the other side
        private RpcQueue callsQueue = null!; // Async set in Create
        // The sending task waits a short moment in the loop to reduce CPU load.
        // Complete this task to immediately start the next round. Using a TaskCompletionSource
        // seems to be much faster than using a CancellationTokenSource (while debugging, experienced ~2 ms vs ~13 ms)
        private TaskCompletionSource<bool> sendingWaiter = new TaskCompletionSource<bool>(); // Remove unneeded generic in .NET 5
        // The execution state of the current and the enqueued calls, where still a caller is waiting
        // for the result. Once a timeout occurs, the call disappears from this dictionary.
        // Always remove a call from this dictionary when it is also removed from the callsQueue!
        private ConcurrentDictionary<ulong, RpcCallExecution> openCalls =
            new ConcurrentDictionary<ulong, RpcCallExecution>();
        // The current call (only one is currently executing on the remote side at a time)
        private RpcCall? currentCall = null;
        private long currentCallStartTime = 0;
        // Cancellation token for stopping the loop
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();


    }

}
