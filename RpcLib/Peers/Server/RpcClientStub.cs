using RpcLib.Model;
using System.Threading.Tasks;

namespace RpcLib.Peers.Server {

    /// <summary>
    /// Base class for a server-side stub of the client's <see cref="IRpcFunctions"/>.
    /// Like <see cref="RpcClientStub"/>, but with the ID of the client
    /// on which to run the commands.
    /// </summary>
    public abstract class RpcClientStub : RpcStub {

        /// <summary>
        /// The ID of the client on which to run the commands.
        /// </summary>
        public string ClientID { get; }

        /// <summary>
        /// Creates a new client-side stub for the client with the given ID.
        /// </summary>
        public RpcClientStub(string clientID) {
            ClientID = clientID;
        }

        /// <summary>
        /// Runs the given RPC command on the client as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        protected Task<T> ExecuteOnClient<T>(RpcCommand command) =>
            RpcServerEngine.Instance.ExecuteOnClient<T>(ClientID, command, TimeoutMs, RetryStrategy);

        /// <summary>
        /// Like <see cref="ExecuteOnServer{T}(RpcCommand)"/> but without return value.
        /// </summary>
        protected Task ExecuteOnClient(RpcCommand command) =>
            ExecuteOnClient<object>(command);

    }

}
