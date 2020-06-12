using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RpcLib.Model {

    /// <summary>
    /// Simple exception data class, which is serializeable to JSON.
    /// </summary>
    public class RpcFailure {

        public RpcFailureType Type { get; }
        public string Message { get; }

        public RpcFailure(RpcFailureType type, string message) {
            Type = type;
            Message = message;
        }

    }

    /// <summary>
    /// Type of RPC failure.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcFailureType {
        /// <summary>
        /// An exception happened on the remote side, when executing the call.
        /// </summary>
        RemoteException,
        /// <summary>
        /// The local side did not receive a response before the timeout happened.
        /// </summary>
        LocalTimeout,
        /// <summary>
        /// The local side could not enqueue this call, because the queue is already full.
        /// This exception is thrown immediately after trying to enqueue the call.
        /// </summary>
        LocalQueueOverflow,
        /// <summary>
        /// The command was already executed earlier, and the cached result is not available
        /// any more so that we could it send again. Since we must not execute the command twice,
        /// we use this failure to notify the remote peer about the problem.
        /// </summary>
        ObsoleteCommandID,
        /// <summary>
        /// Unexpected exception.
        /// </summary>
        Other
    }

}
