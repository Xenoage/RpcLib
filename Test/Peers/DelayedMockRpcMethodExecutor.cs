using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Returns byte 42 for all method calls, but requires some time to response.
    /// The response times have to be given in the constructor.
    /// </summary>
    public class DelayedMockRpcMethodExecutor : IRpcMethodExecutor {

        public DelayedMockRpcMethodExecutor(List<int> delaysMs) {
            this.delaysMs = delaysMs;
        }

        public RpcOptions DefaultOptions { get; } = new RpcOptions();

        public async Task<byte[]?> Execute(RpcMethod method, RpcPeerInfo callingPeer) {
            await Task.Delay(delaysMs[callIndex % delaysMs.Count]);
            callIndex++;
            return new byte[] { 42 };
        }

        private List<int> delaysMs;
        private int callIndex = 0;

    }

}
