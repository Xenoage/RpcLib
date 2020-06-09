using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Server;
using System;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="IDemoRpcClient"/> functions,
    /// one instance for each client.
    /// 
    /// The returned tasks are completed when the response/acknowledgement of the client was received.
    /// When there was any problem (client-side exception, network problem, ...) an exception is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="IDemoRpcClient"/> interface,
    /// since it simply forwards the method calls to the RPC handler.
    /// </summary>
    public class DemoRpcClientStub : IDemoRpcClient {

        public string ClientID { get; }

        /// <summary>
        /// Creates a new stub for the client with the given ID.
        /// </summary>
        public DemoRpcClientStub(string clientID) {
            ClientID = clientID;
        }

        public async Task SayHello(Greeting greeting) =>
            await RpcServerEngine.ExecuteOnClient<object>(ClientID, new RpcCommand("SayHello", greeting)); // <object> for void

        public async Task<SampleData> ProcessData(SampleData baseData) =>
            await RpcServerEngine.ExecuteOnClient<SampleData>(ClientID, new RpcCommand("ProcessData", baseData));

    }

}
