using System.Threading.Tasks;

namespace Xenoage.RpcLib.Methods {

    /// <summary>
    /// Base class for the <see cref="IRpcMethods"/> implementations on the caller side,
    /// i.e. containing code to call the remote side.
    /// </summary>
    public abstract class RpcMethodsStub : IRpcMethods {

        /// <summary>
        /// The ID of the client on which to run the commands, or null for the server.
        /// </summary>
        public string? RemotePeerID { get; }

        /// <summary>
        /// Creates a new callee-side stub for the client with the given ID or null for the server.
        /// </summary>
        public RpcMethodsStub(string? remotePeerID) {
            RemotePeerID = remotePeerID;
        }

        /// <summary>
        /// Runs the given RPC method on the remote peer as soon as possible
        /// and returns the result or throws an <see cref="RpcException"/>.
        /// </summary>
        protected Task<T> ExecuteOnRemotePeer<T>(string methodName, params object[] methodParameters) =>
            Task.FromResult<T>(default!); // GOON RpcServerEngine.Instance.ExecuteOnClient<T>(ClientID, methodName, methodParameters);

        /// <summary>
        /// Like <see cref="ExecuteOnRemotePeer{T}"/> but without return value.
        /// </summary>
        protected Task ExecuteOnRemotePeer(string methodName, params object[] methodParameters) =>
            Task.CompletedTask; // GOON ExecuteOnClient<object>(methodName, methodParameters);

    }

}
