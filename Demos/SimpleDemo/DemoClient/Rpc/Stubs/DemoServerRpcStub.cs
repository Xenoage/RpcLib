using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
using System.Threading.Tasks;

namespace DemoClient.Rpc.Stubs {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="IDemoServerRpc"/> functions.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class DemoServerRpcStub : RpcServerStub, IDemoServerRpc {

        public Task SayHelloToServer(Greeting greeting) =>
            ExecuteOnServer("SayHelloToServer", greeting);

        public Task<SampleData> ProcessDataOnServer(SampleData baseData) =>
            ExecuteOnServer<SampleData>("ProcessDataOnServer", baseData);
    }

}
