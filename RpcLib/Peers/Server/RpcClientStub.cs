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
        /// See <see cref="RpcCommand.CreateForClient"/> for the parameters.
        /// </summary>
        protected Task<T> ExecuteOnClient<T>(string methodName, params object[] methodParameters) =>
            RpcServerEngine.Instance.ExecuteOnClient<T>(
                RpcCommand.CreateForClient(ClientID, methodName, methodParameters));

        /// <summary>
        /// Like <see cref="ExecuteOnClient{T}"/> but without return value.
        /// </summary>
        protected Task ExecuteOnClient(string methodName, params object[] methodParameters) =>
            ExecuteOnClient<object>(methodName, methodParameters);

    }

}
