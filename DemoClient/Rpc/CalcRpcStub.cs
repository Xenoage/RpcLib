using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Server.Client;
using System.Threading.Tasks;

namespace DemoClient.Rpc {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="ICalcRpc"/> functions.
    /// 
    /// The returned tasks are completed when the response of the server was received.
    /// When there was any problem (server-side exception, network problem, ...) an <see cref="RpcException"/> is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="ICalcRpc"/> interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class CalcRpcStub : ICalcRpc {

        public async Task<int> AddNumbers(int number1, int number2) =>
            await RpcClientEngine.ExecuteOnServer<int>(new RpcCommand("AddNumbers", number1, number2));

        public async Task<int> DivideNumbers(int dividend, int divisor) =>
            await RpcClientEngine.ExecuteOnServer<int>(new RpcCommand("DivideNumbers", dividend, divisor));
    }

}
