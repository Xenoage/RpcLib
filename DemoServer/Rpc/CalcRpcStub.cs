using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="ICalcRpc"/> functions,
    /// one instance for each client.
    /// This file could be auto-generated later from the <see cref="ICalcRpc"/> interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class CalcRpcStub : ICalcRpc {

        public string ClientID { get; }

        /// <summary>
        /// Creates a new stub for the client with the given ID.
        /// </summary>
        public CalcRpcStub(string clientID) {
            ClientID = clientID;
        }

        public async Task<int> AddNumbers(int number1, int number2) =>
            await RpcServerEngine.ExecuteOnClient<int>(ClientID, new RpcCommand("AddNumbers", number1, number2));

        public async Task<int> DivideNumbers(int dividend, int divisor) =>
            await RpcServerEngine.ExecuteOnClient<int>(ClientID, new RpcCommand("DivideNumbers", dividend, divisor));
    }

}
