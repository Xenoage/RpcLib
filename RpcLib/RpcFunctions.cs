using RpcLib.Model;
using System.Threading.Tasks;

namespace RpcLib {

    /// <summary>
    /// Base class for real <see cref="IRpcFunctions"/> implementations (not stubs).
    /// New instances are created for each RPC call, to support the <see cref="Context"/> property.
    /// </summary>
    public abstract class RpcFunctions : IRpcFunctions {

        /// <summary>
        /// The context while calling an RPC method.
        /// Contains the ID of the calling client for example.
        /// </summary>
        public RpcContext Context { get; set; } = new RpcContext("", null);

        /// <summary>
        /// Implement this method to map the encoded RPC commands to
        /// the real methods in this class. Thrown exceptions are catched
        /// and the calling peer gets notified about a
        /// <see cref="RpcFailureType.RemoteException"> failure.
        /// If successfull, returns a task with the JSON-encoded result or null
        /// if the method has no return type (void).
        /// If the given command name is undefined in this implementation, returns null
        /// (not a Task with value null, but null itself!).
        /// </summary>
        public abstract Task<string?>? Execute(RpcCommand command);

    }

}
