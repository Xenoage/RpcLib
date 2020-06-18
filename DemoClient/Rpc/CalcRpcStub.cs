using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
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
    public class CalcRpcStub : RpcServerStub, ICalcRpc {

        public Task<int> AddNumbers(int number1, int number2) =>
            ExecuteOnServer<int>(new RpcCommand("AddNumbers", number1, number2));

        public Task<int> DivideNumbers(int dividend, int divisor) =>
            ExecuteOnServer<int>(new RpcCommand("DivideNumbers", dividend, divisor));
    }

}
