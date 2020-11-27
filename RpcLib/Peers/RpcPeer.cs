using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Serialization;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Base class for both the <see cref="RpcServer"/> and the <see cref="RpcClient"/>.
    /// </summary>
    public abstract class RpcPeer : IRpcPeer {

        /// <summary>
        /// Gets the channel for communication with the given remote peer.
        /// </summary>
        protected abstract RpcChannel GetChannel(string? remotePeerID);

        public async Task<T> ExecuteOnRemotePeer<T>(string? remotePeerID,
                string methodName, params object[] methodParameters) {
            // Only calls to the server are supported
            if (remotePeerID != null)
                throw new RpcException(RpcFailure.Other("Clients can only call the server"));
            // Enqueue in the corresponding channel and await call
            try {
                var method = new RpcMethod(methodName, methodParameters);
                var call = PrepareCall(remotePeerID, method);
                var channel = GetChannel(remotePeerID);
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
        public void ApplyRpcOptionsFromCallStack(RpcCall call, StackTrace? stackTrace = null) {
            // Use the given stack trace or the current one
            stackTrace = stackTrace ?? new StackTrace();
            // Find attributes (e.g. custom timeout, retry strategy) for this method definition.
            // In the call stack, find a caller (e.g. "MyRpcStub") compatible with the "IRpcMethods" interface.
            // Find a method with the command's name and have a look at its RpcOptions attribute.
            foreach (var stackFrame in stackTrace.GetFrames()) {
                var frameType = stackFrame?.GetMethod()?.DeclaringType;
                if (frameType != null && typeof(IRpcMethods).IsAssignableFrom(frameType)) {
                    var method = frameType.GetMethod(call.Method.Name);
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

    }

}
