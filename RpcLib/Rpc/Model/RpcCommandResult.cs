namespace RpcLib.Model {

    /// <summary>
    /// Result of an <see cref="RpcCommand"/>. There a three mutually exclusive scenarios:
    /// 1) Success. Return value is null (for void return values) or the JSON-encoded return data.
    /// 2) Remote failure. An exception happened on the remote side.
    /// 3) Local failure. Normally a networking problem, e.g. the request could not be sent
    ///    or no response was received before the timeout.
    /// </summary>
    public class RpcCommandResult {

        public static RpcCommandResult FromSuccess(ulong callNumber, string? resultJson) =>
            new RpcCommandResult(callNumber) { ResultJson = resultJson };

        public static RpcCommandResult FromFailure(ulong callNumber, RpcFailure failure) =>
            new RpcCommandResult(callNumber) { Failure = failure };

        public RpcCommandResult(ulong callNumber) {
            ID = callNumber;
        }

        /// <summary>
        /// The unique <see cref="RpcCommand.ID"/> this response belongs to.
        /// </summary>
        public ulong ID { get; }

        /// <summary>
        /// Only set when there is no <see cref="Exception"/>.
        /// Contains the JSON-encoded response data of the RPC call.
        /// </summary>
        public string? ResultJson { get; private set; } = null;

        /// <summary>
        /// An exception happened on either the local or the remote side.
        /// </summary>
        public RpcFailure? Failure { get; private set; } = null;

        /// <summary>
        /// Returns, whether is object stores a successful or failed result.
        /// </summary>
        public RpcCallState State => Failure != null ? RpcCallState.Failed : RpcCallState.Successful;

    }

}
