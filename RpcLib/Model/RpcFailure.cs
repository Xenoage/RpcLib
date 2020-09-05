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
        /// Returns true, iff this failure happened because the remote peer could not
        /// be reached, probably caused by a networking problem.
        /// In this case, the command could be repeated later.
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        public bool IsNetworkProblem =>
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
        /// The client was not able to authenticate at the server (HTTP status code 401).
        /// Check the credentials of the client.
        /// </summary>
        AuthError,
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
        /// The peer received a response from the remote peer, but it was not an expected RPC response
        /// (in case of a remote exception, a well-formated response with an <see cref="RemoteException"/>
        /// would be expected). We should not repeat this command, since it would probably result
        /// in the same error again.
        /// </summary>
        RpcError,
        /// <summary>
        /// The command was already executed earlier, and the cached result is not available
        /// any more so that we could it send again. Since we must not execute the command twice,
        /// we use this failure to notify the remote peer about the problem.
        /// This is only true for non-retryable commands. Retryable commands can be sent
        /// "out of order" and will not produce this error.
        /// </summary>
        ObsoleteCommandID,
        /// <summary>
        /// Unexpected exception.
        /// </summary>
        Other
    }

}
