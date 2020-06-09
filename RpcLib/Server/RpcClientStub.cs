using Newtonsoft.Json;
using Shared;
using Shared.Model;
using Shared.Rpc;
using System.Threading.Tasks;

namespace RpcServer.Rpc {

    /// <summary>
    /// Server-side (stub) implementation of the <see cref="IRpcClient"/> functions,
    /// one instance for each client.
    /// 
    /// The returned tasks are completed when the response/acknowledgement of the client was received.
    /// When there was any problem (client-side exception, network problem, ...) an exception is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="IRpcClient"/> interface,
    /// since it simply forwards the method calls to the RPC handler.
    /// </summary>
    public class RpcClientStub : IRpcClient {

        public string ClientID { get; }

        /// <summary>
        /// Creates a new stub for the client with the given ID.
        /// </summary>
        public RpcClientStub(string clientID) {
            ClientID = clientID;
        }

        public async Task SayHello(Greeting greeting) {
            await Run(new RpcCommand("SayHello", greeting));
        }

        public async Task<SampleData> ProcessData(SampleData baseData) {
            return await Run<SampleData>(new RpcCommand("ProcessData", baseData));
        }

        // TODO: Move to RPC Handler
        private async Task Run(RpcCommand command) {
            // TODO: Enqueue, wait for result, then return
        }

        // TODO: Move to RPC Handler
        private async Task<T> Run<T>(RpcCommand command) where T : class {
            // TODO, wait for result, then return
            return null;
        }

    }

}
