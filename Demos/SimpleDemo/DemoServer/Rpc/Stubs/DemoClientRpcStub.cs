using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace DemoServer.Rpc.Stubs {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="IDemoClientRpc"/> functions,
    /// one instance for each client.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class DemoClientRpcStub : RpcClientStub, IDemoClientRpc {

        public DemoClientRpcStub(string clientID) : base(clientID) {
        }

        public Task SayHelloToClient(Greeting greeting) =>
            ExecuteOnClient(new RpcCommand("SayHello", greeting));

        public Task<SampleData> ProcessDataOnClient(SampleData baseData) =>
            ExecuteOnClient<SampleData>(new RpcCommand("ProcessData", baseData));

    }

}
