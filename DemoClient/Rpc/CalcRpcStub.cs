using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using RpcLib.Server.Client;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="ICalcRpc"/> functions.
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
