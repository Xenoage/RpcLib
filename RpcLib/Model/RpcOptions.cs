namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Settings for the RPC communication.
    /// </summary>
    public class RpcOptions {

        /// <summary>
        /// Maximum time in milliseconds a call may take to be executed.
        /// This is the time from the local invocation (including the time where it is still in the queue)
        /// until the response was received from the remote side.
        /// 30 seconds by default.
        /// Can be overwritten on method level by using the <see cref="RpcOptionsAttribute.TimeoutMs"/>.
        /// </summary>
        public int TimeoutMs { get; set; } = 30_000;

    }

}
