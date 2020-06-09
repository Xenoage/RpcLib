using RpcLib.Model;
using System;
using System.Threading.Tasks;

namespace RpcLib.Client {

    /// <summary>
    /// This is the core part of the RPC engine on the client side.
    /// It sends the commands to the server, i.e. it stores the command queue,
    /// sends the commands as soon as possible and processes the results.
    /// It also pulls the commands for this client from the server,
    /// executes them and responds with the results.
    /// </summary>
    public static class RpcClientEngine {

        /// <summary>
        /// Runs the given RPC command on the server as soon as possible.
        /// The returned task finishes when the call was either successfully executed,
        /// or failed (e.g. because of a timeout).
        /// The result is stored in the given command itself. If successful, the JSON-encoded return value
        /// is also returned, otherwise an <see cref="RpcException"/> is thrown.
        /// </summary>
        public static async Task<T> ExecuteOnServer<T>(RpcCommand command) where T : class {
            throw new NotImplementedException(); // TODO
        }

    }

}
