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
                        await channel.Send(RpcMessage.Encode(result), cancellationToken.Token);
                        didSomething = true;
                    }
                    // Next call already queued? Then see if we can send it already.
                    if (await callsQueue.Peek() is RpcCall call) {
                        if (false == openCalls.ContainsKey(call.Method.ID)) {
                            // The call has already been disappeared, probably because the timeout
                            // was hit before it could even be sent. So ignore it.
                            Log.Trace($"Do not send method {call.Method.ID} {call.Method.Name}, because already in timeout");
                            await callsQueue.Dequeue();
                            didSomething = true;
                        }
                        else if (TimeNowMs() > nextSendTimeMs) {
                            // Send it. Do not dequeue it yet, only after is has been finished.
                            nextSendTimeMs = TimeNowMs() + GetTimeout(call).Milliseconds; // Is reset to 0 when we receive a response
                            await channel.Send(RpcMessage.Encode(call.Method), cancellationToken.Token);
                            didSomething = true;
                        }
                    }
                    // Close nicely, when locally requested
                    if (cancellationToken.IsCancellationRequested) {
                        await channel.Close();
                        didSomething = true;
                    }
                    // When we had something to do, immediately continue. Otherwise, wait a short moment.
                    if (false == didSomething)
                        await Task.Delay(100);
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
                        HandleReceivedMessage(message);
                    else // When we had something to do, immediately continue. Otherwise, wait a short moment.
                        await Task.Delay(100);
                }
            } catch (Exception ex) {
                Log.Debug($"Unexpectedly closed connection to {RemoteInfo}: {ex.Message}");
            }
        }

        private void HandleReceivedMessage(RpcMessage message) {
            try {
                string logFrom = $"received from {RemoteInfo}, {message.Data.Length} bytes";
                if (message.IsRpcMethod()) {
                    // Received a method call. Execute it immediately and enqueue the result.
                    var method = message.DecodeRpcMethod();
                    Log.Trace($"Method call {method.ID} {method.Name} {logFrom}");
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
                    });
                } else if (message.IsRpcResult()) {
                    // Received a return value
                    nextSendTimeMs = 0; // Allow the next call to start immediately
                    var result = message.DecodeRpcResult();
                    Log.Trace($"Return value for call {result.MethodID} {logFrom}");
                    // Set the result
                    if (openCalls.TryGetValue(result.MethodID, out var callExecution))
                        callExecution.Finish(result);
                    else
                        throw new Exception("Out of order: Received return value for non-open call");
                } else {
                    // Unsupported message
                    Log.Trace($"Unsupported message {logFrom}");
                }
            } catch (Exception ex) {
                Log.Debug($"Error when handling message from {RemoteInfo}: {ex.Message}");
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
            // Enquees the call and awaits the result
            var execution = new RpcCallExecution(call);
            openCalls.TryAdd(call.Method.ID, execution);
            await callsQueue.Enqueue(call);
            var result = await execution.AwaitResult(GetTimeout(call));
            openCalls.TryRemove(call.Method.ID, out var _);
            nextSendTimeMs = 0; // Allow the next call immediately
            return result;
        }

        private TimeSpan GetTimeout(RpcCall call) =>
            TimeSpan.FromMilliseconds(call.TimeoutMs ?? executor.DefaultOptions.TimeoutMs);

        // The communication channel
        private IRpcChannel channel;
        // Run the actual C# implementations of the RPC methods
        private IRpcMethodExecutor executor;
        // Queue of responses to send to the other side
        private ConcurrentQueue<RpcResult> resultsQueue = new ConcurrentQueue<RpcResult>();
        // Queue of calls to send to the other side
        private RpcQueue callsQueue = null!; // Async set in Create
        // Unix milliseconds timestamp when to send the next call.
        // By setting to 0, we may immediately send the next call. Using this field, we switch
        // between the modes "able to send next call" and "wait for receiving response of last call",
        // while still be tolerant for failures (when we never receive a response for a sent call).
        private long nextSendTimeMs = 0;
        // The execution state of the current and the enqueued calls
        private ConcurrentDictionary<ulong, RpcCallExecution> openCalls =
            new ConcurrentDictionary<ulong, RpcCallExecution>();
        // Cancellation token for stopping the loop
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();


    }

}
