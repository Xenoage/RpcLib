using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Runs <see cref="RpcMethod"/>s on the callee side,
    /// i.e. calling the "real" C# method implementations.
    /// </summary>
    public interface IRpcMethodExecutor {

        /// <summary>
        /// Calls the corresponding method and returns the return value, or null for a void method.
        /// When no implementation can be found, a <see cref="RpcException"/> with
        /// <see cref="RpcFailureType.MethodNotFound"/> is thrown.
        /// </summary>
        public abstract Task<byte[]?> Execute(RpcMethod method);

        /// <summary>
        /// Gets the default options when executing methods.
        /// </summary>
        public RpcOptions DefaultOptions { get; }

    }

}
