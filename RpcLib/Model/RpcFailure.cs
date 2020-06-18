using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RpcLib.Model {

    /// <summary>
    /// Simple exception data class, which is serializeable to JSON.
    /// </summary>
    public class RpcFailure {

        public RpcFailure(RpcFailureType type, string message) {
            Type = type;
            Message = message;
        }

        public RpcFailureType Type { get; }

        public string Message { get; }

        /// <summary>
        /// Returns true, iff this failure happened within the RPC engine, probably
        /// caused by a networking problem.
        /// In this case, the command should be repeated later.
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        public bool IsRpcProblem =>
            Type == RpcFailureType.Timeout || Type == RpcFailureType.QueueOverflow;

    }

    /// <summary>
    /// Type of RPC failure.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RpcFailureType {
        /// <summary>
        /// An exception happened on the remote side, when executing the call.
        /// Typical examples are an I/O error or a division-by-0 exception.
        /// Since this is no problem with the RPC engine, the command should only be
        /// repeated if there is a reasonable chance that it will work the next time.
        /// </summary>
        RemoteException,
        /// <summary>
        /// The local side did not receive a response before the timeout happened.
        /// In this case, the command should be repeated when the other peer is online again.
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        Timeout,
        /// <summary>
        /// The local side could not enqueue this call, because the queue is already full.
        /// This exception is thrown immediately after trying to enqueue the call.
        /// In this case, the command should be repeated when the other peer is reachable again.
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        QueueOverflow,
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
