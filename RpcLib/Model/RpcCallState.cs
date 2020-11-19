namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// The current state of an <see cref="RpcCall"/>.
    /// </summary>
    public enum RpcCallState {

        /// <summary>
        /// The start state. It was just created.
        /// </summary>
        Created,

        /// <summary>
        /// The call is enqueued and will be sent as soon as possible.
        /// </summary>
        Enqueued,

        /// <summary>
        /// The call was sent and, as soon as it reached the remote side, is executing there.
        /// </summary>
        Sent,

        /// <summary>
        /// The response of the call was received. It was executed successfully on the remote side.
        /// The response value (if not void) can be retrieved from <see cref="RpcCall.Result"/>.
        /// </summary>
        Successful,

        /// <summary>
        /// The call failed, either on the local side (because of a timeout for example) or on
        /// the remote side (exception during execution).
        /// </summary>
        Failed

    }

}