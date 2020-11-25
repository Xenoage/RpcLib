using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Returns byte 42 for all method calls.
    /// </summary>
    public class MockRpcMethodExecutor : IRpcMethodExecutor {

        public RpcOptions DefaultOptions { get; } = new RpcOptions();

        public Task<byte[]?> Execute(RpcMethod method) =>
            Task.FromResult((byte[]?) new byte[] { 42 });

    }

}
