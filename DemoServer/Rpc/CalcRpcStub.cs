using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="ICalcRpc"/> functions,
    /// one instance for each client.
    /// 
    /// The returned tasks are completed when the response/acknowledgement of the client was received.
    /// When there was any problem (client-side exception, network problem, ...) an exception is thrown.
    /// 
    /// This file could be auto-generated later from the <see cref="ICalcRpc"/> interface,
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
