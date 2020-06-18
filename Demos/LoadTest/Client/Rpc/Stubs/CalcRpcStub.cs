using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
using System.Threading.Tasks;

namespace Client.Rpc.Stubs {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="ICalcRpc"/> functions.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class CalcRpcStub : RpcServerStub, ICalcRpc {

        public Task<int> AddNumbers(int number1, int number2) =>
            ExecuteOnServer<int>(new RpcCommand("AddNumbers", number1, number2));

        public Task<int> DivideNumbers(int dividend, int divisor) =>
            ExecuteOnServer<int>(new RpcCommand("DivideNumbers", dividend, divisor));
    }

}
