namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Type of RPC failure.
    /// </summary>
    public enum RpcFailureType {

        /// <summary>
        /// An exception happened on the remote side, when executing the call.
        /// Typical examples are an I/O error or a division-by-0 exception.
        /// Since this is no problem of the RPC engine itself, the RPC call should only be
        /// repeated by the caller if there is a reasonable chance that it will work
        /// the next time.
        /// </summary>
        RemoteException,

        /// <summary>
        /// The local side did not receive a response before the timeout happened.
        /// This can happen when the remote side responds too slowly, but it's also possible
        /// that the RPC call was not even sent yet, because there were too many tasks
        /// in the local queue before this call.
        /// In this case, the RPC call can be repeated when the other peer is reachable again.
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        Timeout,

        /// <summary>
        /// The peer received a response from the remote peer, but it was not an expected RPC response
        /// (in case of a remote exception, a well-formated response with a <see cref="RemoteException"/>
        /// would be expected). We can repeat this RPC call, since it seems, that the server is not
        /// available right now (for example, it may be a 503 service anavailable error).
        /// See <see cref="RpcRetryStrategy"/> how to automate this.
        /// </summary>
        RpcError,

        /// <summary>
        /// Unexpected exception.
        /// </summary>
        Other
    }

    public static class RpcFailureTypeEx {

        /// <summary>
        /// Returns true, iff this failure type indicates that the failed command can be
        /// tried again, because there is a chance that it will work the next time.
        /// See the documentation of the <see cref="RpcFailureType"/> items.
        /// </summary>
        public static bool IsRetryable(this RpcFailureType type) =>
            type == RpcFailureType.Timeout
            || type == RpcFailureType.RpcError;
    }

}
