using RpcLib.Model;
using RpcLib.Peers.Client;
using RpcLib.Peers.Server;

namespace RpcLib.Peers {

    /// <summary>
    /// Base class for both <see cref="RpcClientStub"/> and <see cref="RpcServerStub"/>.
    /// </summary>
    public abstract class RpcStub : IRpcFunctions {

        /// <summary>
        /// The retry strategy for the commands called in this class.
        /// By default none.
        /// </summary>
        public RpcRetryStrategy RetryStrategy { get; set; } = RpcRetryStrategy.None;

        /// <summary>
        /// A custom timeout in milliseconds for executing commands in this stub.
        /// If null, <see cref="RpcCommand.timeoutMs"/> is used.
        /// </summary>
        public int? TimeoutMs { get; set; } = null;

    }

}
