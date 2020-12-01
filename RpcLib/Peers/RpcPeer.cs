using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Queue;
using Xenoage.RpcLib.Serialization;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Base class for both the <see cref="RpcServer"/> and the <see cref="RpcClient"/>.
    /// </summary>
    public abstract class RpcPeer : IRpcPeer, IRpcMethodExecutor {

        /// <summary>
        /// Settings for this local peer.
        /// </summary>
        public RpcPeerSettings Settings { get; set; }

        /// <summary>
        /// Creates a new local peer with can locally execute the given RPC methods,
        /// using the given options by default.
        /// </summary>
        public RpcPeer(IEnumerable<Type> methods, RpcPeerSettings settings) {
            this.methods = methods.ToList();
            Settings = settings;
            Serializer.Instance = Settings.Serializer;
            Logging.Log.Instance = Settings.Logger;
        }

        /// <summary>
        /// Gets the channel for communication with the given remote peer,
        /// or null if no such peer is connected.
        /// </summary>
        protected abstract RpcChannel? GetChannel(string? remotePeerID);

        /// <summary>
        /// Starts the communication. When the connection breaks,
        /// it is automatically reestablished.
        /// The returned task runs until <see cref="Stop"/> is called.
        /// </summary>
        public abstract Task Start();

        /// <summary>
        /// Stops the communication, i.e. the task returned by
        /// <see cref="Start"/> will be completed.
        /// </summary>
        public abstract void Stop();

        public async Task<T> ExecuteOnRemotePeer<T>(string? remotePeerID,
                string methodName, params object[] methodParameters) {
            // Enqueue in the corresponding channel and await call
            try {
                var method = new RpcMethod(methodName, methodParameters);
                var call = PrepareCall(remotePeerID, method);
                var channel = GetChannel(remotePeerID);
                if (channel == null) {
                    string msg = $"No client with ID {remotePeerID} connected";
                    // The given client is not connected. If the command is retryable and if
                    // a backlog is registered, remember it.
                    if (call.IsRetryable() && Settings.Backlog != null) {
                        await RpcQueue.AddToBacklogIfApplicable(call, Settings.Backlog);
                        msg += ", but adding the call to the backlog";
                    }
                    throw new Exception(msg);
                }
                var result = await channel.Run(call);
                if (result.Failure != null)
                    throw new RpcException(result.Failure);
                else if (result.ReturnValue != null)
                    return Serializer.Deserialize<T>(result.ReturnValue);
                else
                    return default!; // void methods are called with T = object, thus null
            }
            catch (RpcException) {
                throw;
            }
            catch (Exception ex) {
                throw new RpcException(RpcFailure.Other(ex.Message));
            }
        }

        /// <summary>
        /// Returns a new call for the given remote peer and method, applying all options like
        /// timeout and retry strategy.
        /// </summary>
        public RpcCall PrepareCall(string? remotePeerID, RpcMethod method) {
            var call = new RpcCall {
                RemotePeerID = remotePeerID,
                Method = method
            };
            ApplyRpcOptionsFromCallStack(call);
            return call;
        }

        /// <summary>
        /// When this method is called with a class implementing <see cref="IRpcMethods"/>
        /// in the calling stack, the <see cref="RpcOptionsAttribute"/> (if any) of the method
        /// with this command name are read and applied.
        /// </summary>
        public static void ApplyRpcOptionsFromCallStack(RpcCall call, StackTrace? stackTrace = null) {
            // Use the given stack trace or the current one
            stackTrace = stackTrace ?? new StackTrace();
            // Find attributes (e.g. custom timeout, retry strategy) for this method definition.
            // In the call stack, find a caller (e.g. "MyRpcStub") compatible with the "IRpcMethods" interface.
            // Find a method with the command's name and have a look at its RpcOptions attribute.
            foreach (var stackFrame in stackTrace.GetFrames()) {
                var frameType = stackFrame?.GetMethod()?.DeclaringType;
                if (frameType?.GetInterfaces().FirstOrDefault(it => it.GetInterfaces().Contains(typeof(IRpcMethods))) is Type intf) {
                    var method = intf.GetMethod(call.Method.Name);
                    if (method != null) {
                        if (method.GetCustomAttribute<RpcOptionsAttribute>() is RpcOptionsAttribute options) {
                            if (options.TimeoutMs != RpcOptionsAttribute.useDefaultTimeout)
                                call.TimeoutMs = options.TimeoutMs;
                            if (options.RetryStrategy is RpcRetryStrategy retryStrategy)
                                call.RetryStrategy = retryStrategy;
                        }
                        break; // Method found, do not traverse call stack any further
                    }
                }
            }
        }

        public async Task<byte[]?> Execute(RpcMethod method, RpcPeerInfo callingPeer) {
            var context = CreateRpcContext(callingPeer);
            // Try to find and execute method (TODO: speed up)
            foreach (var m in methods) {
                var methodInstance = (RpcMethods) Activator.CreateInstance(m)!;
                methodInstance.Context = context;
                if (methodInstance.Execute(method) is Task<byte[]?> task) {
                    // Found. Execute it and return result (null for void).
                    byte[]? returnValue = await task;
                    return returnValue;
                }
            }
            // Method could not be found
            throw new RpcException(new RpcFailure {
                Type = RpcFailureType.MethodNotFound
            });
        }

        protected abstract RpcContext CreateRpcContext(RpcPeerInfo callingPeer);

        public RpcOptions DefaultOptions => Settings.DefaultOptions;

        // The registered RPC methods for local execution
        private List<Type> methods;
    }

}
