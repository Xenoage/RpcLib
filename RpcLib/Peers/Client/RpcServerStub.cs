using RpcLib.Model;
using RpcLib.Server.Client;
using System.Threading.Tasks;

namespace RpcLib.Peers.Client {

    /// <summary>
    /// Base class for a client-side stub of the server's <see cref="IRpcFunctions"/>.
    /// Instances of stub classes can be reused, as long as the retry strategy
    /// stays the same. Otherwise, it is recommended to create a new stub instance
    /// for each RPC function call.
    /// </summary>
    public abstract class RpcServerStub : IRpcFunctions {

        /// <summary>
        /// The retry strategy for the commands called in this class.
        /// By default none.
        /// </summary>
        public RpcRetryStrategy RetryStrategy { get; }  = RpcRetryStrategy.None;

        /// <summary>
        /// Creates a new server-side stub with no retry strategy.
        /// </summary>
        public RpcServerStub() {
        }

        /// <summary>
        /// Creates a new server-side stub with the given retry strategy.
        /// </summary>
        public RpcServerStub(RpcRetryStrategy retryStrategy) {
            RetryStrategy = retryStrategy;
        }

        /// <summary>
        /// Runs the given RPC command on the server as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        protected Task<T> ExecuteOnServer<T>(RpcCommand command) =>
            RpcClientEngine.ExecuteOnServer<T>(command, RetryStrategy);

        /// <summary>
        /// Like <see cref="ExecuteOnServer{T}(RpcCommand)"/> but without return value.
        /// </summary>
        protected Task ExecuteOnServer(RpcCommand command) =>
            ExecuteOnServer<object>(command);

    }

}
