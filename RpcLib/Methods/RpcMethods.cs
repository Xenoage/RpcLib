using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Methods {

    /// <summary>
    /// Base class for the <see cref="IRpcMethods"/> implementations on the callee side,
    /// i.e. containing the real implementations of the methods.
    /// New instances are created for each RPC call, to support the <see cref="Context"/> property.
    /// </summary>
    public abstract class RpcMethods : IRpcMethods {

        /// <summary>
        /// The context while calling an RPC method.
        /// Contains the ID of the calling peer for example.
        /// </summary>
        public RpcContext Context { get; set; } = null!; // Will be set when a method is called

        /// <summary>
        /// Implement this method to map the encoded RPC methods to
        /// the real methods in this class. Thrown exceptions are catched
        /// and the calling peer gets notified about a
        /// <see cref="RpcFailureType.RemoteException"/> failure.
        /// If successfull, returns a task with the encoded result or null
        /// if the method has no return type (void).
        /// If the given command name is undefined in this implementation, returns null
        /// (not a Task with value null, but null itself!).
        /// </summary>
        public abstract Task<byte[]?>? Execute(RpcMethod method);

    }

}
