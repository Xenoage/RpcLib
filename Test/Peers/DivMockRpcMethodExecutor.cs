using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Executes <see cref="Div"/> tasks.
    /// </summary>
    public class DivMockRpcMethodExecutor : IRpcMethodExecutor {

        public RpcOptions DefaultOptions { get; } = new RpcOptions();

        public async Task<byte[]?> Execute(RpcMethod method, RpcPeerInfo callingPeer) {
            var div = Div.FromMethod(method);
            byte ret = (byte) (div.dividend / div.divisor); // Yes, may throw div/0 exception
            return await Task.FromResult(new byte[] { ret });
        }

    }

}
