using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace DemoServer.Rpc.Stubs {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="ICalcRpc"/> functions,
    /// one instance for each client.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class CalcRpcStub : RpcClientStub, ICalcRpc {

        public CalcRpcStub(string clientID) : base(clientID) {
        }

        public Task<int> AddNumbers(int number1, int number2) =>
            ExecuteOnClient<int>(new RpcCommand("AddNumbers", number1, number2));

        public Task<int> DivideNumbers(int dividend, int divisor) =>
            ExecuteOnClient<int>(new RpcCommand("DivideNumbers", dividend, divisor));
    }

}
