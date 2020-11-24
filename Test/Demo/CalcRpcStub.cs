using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;

namespace Xenoage.RpcLib.Demo {

    /// <summary>
    /// Demo stub implementation of the <see cref="ICalcRpc"/> methods.
    /// This is the class used on the calling peer, i.e. it contains code to call the remote side.
    /// This is just boilerplate code; we could auto-generate this class later in .NET 5 with source generators.
    /// </summary>
    public class CalcRpcStub : RpcMethodsStub, ICalcRpc {

        public CalcRpcStub(string? targetPeerID) : base(targetPeerID) {
        }

        public Task<int> AddNumbers(int number1, int number2) =>
            ExecuteOnRemotePeer<int>("AddNumbers", number1, number2);

        public Task<int> DivideNumbers(int dividend, int divisor) =>
            ExecuteOnRemotePeer<int>("DivideNumbers", dividend, divisor);
    }
}
