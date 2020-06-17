using Microsoft.Extensions.DependencyInjection;

namespace RpcLib.Model {

    /// <summary>
    /// The calling context within a <see cref="RpcFunctions"/>.
    /// </summary>
    public class RpcContext {

        /// <summary>
        /// The ID of this client (when running client-side) or of the calling client
        /// (when running server-side).
        /// </summary>
        public string ClientID { get; }

        /// <summary>
        /// The current factory to create services within a scope. Can be used
        /// to load dependency-injected classes during execution of an RPC command.
        /// Only set on the server side.
        /// </summary>
        public IServiceScopeFactory? ServiceScopeFactory { get; }

        public RpcContext(string clientID, IServiceScopeFactory? serviceScopeFactory) {
            ClientID = clientID;
            ServiceScopeFactory = serviceScopeFactory;
        }

    }

}
