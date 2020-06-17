using Microsoft.Extensions.DependencyInjection;
using RpcLib.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpcLib.Peers {

    /// <summary>
    /// Use this class to run commands, finding the implementation in one of the registered <see cref="RpcFunctions"/>s.
    /// </summary>
    public class RpcCommandRunner {

        private IEnumerable<RpcFunctions> rpcFunctions;
        private IServiceScopeFactory? serviceScopeFactory;

        public RpcCommandRunner(IEnumerable<RpcFunctions> rpcFunctions, IServiceScopeFactory? serviceScopeFactory) {
            this.rpcFunctions = rpcFunctions;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Runs the given command on the client ID.
        /// </summary>
        public async Task<RpcCommandResult> Execute(string clientID, RpcCommand command) {
            foreach (var rpc in rpcFunctions) {
                rpc.Context = new RpcContext(clientID, serviceScopeFactory);
                if (rpc.Execute(command) is Task<string?> task) {
                    // Method found in this class
                    return RpcCommandResult.FromSuccess(command.ID, await task);
                }
            }
            // Called method is not implemented in any registered class
            throw new Exception("Unknown method name: " + command.MethodName);
        }

    }

}
