using DemoShared.Model;
using DemoShared.Rpc;
using RpcLib.Client;
using RpcLib.Model;
using System.Threading.Tasks;

namespace DemoClient.Rpc {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="IDemoRpcServer"/> functions.
    /// 
    /// The returned tasks are completed when the response of the server was received.
    /// When there was any problem (server-side exception, network problem, ...) an <see cref="RpcException"/> is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="IDemoRpcServer"/> interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class DemoRpcServerStub : IDemoRpcServer {

        public async Task SayHelloToServer(Greeting greeting) =>
            await RpcClientEngine.ExecuteOnServer(new RpcCommand("SayHelloToServer", greeting));

        public async Task<SampleData> ProcessDataOnServer(SampleData baseData) =>
            await RpcClientEngine.ExecuteOnServer<SampleData>(new RpcCommand("ProcessDataOnServer", baseData));

        public async Task<int> AddNumbers(int number1, int number2) =>
            await RpcClientEngine.ExecuteOnServer<int>(new RpcCommand("AddNumbers", number1, number2));
    }

}
