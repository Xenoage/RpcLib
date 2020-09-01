namespace RpcLib.Model {

    /// <summary>
    /// Result of an <see cref="RpcCommand"/>. There a three mutually exclusive scenarios:
    /// 1) Success. Return value is null (for void return values) or the JSON-encoded return data.
    /// 2) Remote failure. An exception happened on the remote side. Do not run the same command again.
    /// 3) Local failure. Normally a networking problem, e.g. the request could not be sent
    ///    or no response was received before the timeout. The command should be retried within the timeout period.
    /// </summary>
    public class RpcCommandResult {

        /// <summary>
        /// Creates a new result for the given command ID after successful execution, using
        /// the given return value encoded in JSON (or null for void return type)
        /// and the given compression strategy.
        /// </summary>
        public static RpcCommandResult FromSuccess(ulong commandID, string? resultJson,
                RpcCompressionStrategy? compression) =>
            new RpcCommandResult(commandID) { ResultJson = resultJson, Compression = compression };

        /// <summary>
        /// Creates a new result for the given command ID after failed execution, using
        /// the given failure reason and the given compression strategy
        /// </summary>
        public static RpcCommandResult FromFailure(ulong commandID, RpcFailure failure,
                RpcCompressionStrategy? compression) =>
            new RpcCommandResult(commandID) { Failure = failure, Compression = compression };

        public RpcCommandResult(ulong commandID) {
            CommandID = commandID;
        }

        // For deserialization only
        public RpcCommandResult() {
        }

        /// <summary>
        /// The unique <see cref="RpcCommand.ID"/> this response belongs to.
        /// </summary>
        public ulong CommandID { get; set; }

        /// <summary>
        /// Only set when there is no <see cref="Failure"/>.
        /// Contains the JSON-encoded response data of the RPC call.
        /// </summary>
        public string? ResultJson { get; set; } = null;

        /// <summary>
        /// An exception happened on either the local or the remote side.
        /// </summary>
        public RpcFailure? Failure { get; set; } = null;

        /// <summary>
        /// Message compression strategy (the same as the strategy of the
        /// command this result belongs to).
        /// </summary>
        public RpcCompressionStrategy? Compression { get; set; } = null;

        /// <summary>
        /// Returns, whether is object stores a successful or failed result.
        /// </summary>
        public RpcCommandState State =>
            Failure != null ? RpcCommandState.Failed : RpcCommandState.Successful;

    }

}
