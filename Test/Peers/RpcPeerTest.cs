using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Channels;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Tests for <see cref="RpcPeer"/>, using a <see cref="ReceivingMockRpcChannel"/>.
    /// </summary>
    [TestClass]
    public class RpcPeerTest {

        [TestMethod, Timeout(1000)]
        public async Task Start_And_Stop_Test() {
            var rpcPeer = await RpcPeer.Create(new PeerInfo(null, "localhost"),
                new ReceivingMockRpcChannel(new Queue<RpcMessage>()),
                new MockExecutor_Result42(), backlog: null);
            var rpcPeerTask = rpcPeer.Start();
            await Task.Delay(500);
            Assert.IsFalse(rpcPeerTask.IsCompleted);
            rpcPeer.Stop();
            await Task.Delay(200);
            Assert.IsTrue(rpcPeerTask.IsCompleted);
        }

        /// <summary>
        /// Tests receiving a call and sending a response.
        /// </summary>
        [TestMethod, Timeout(1000)]
        public async Task ReceiveMethod_And_SendResult_Test() {
            // Method call to receive
            ulong id = 25;
            var receiving = new Queue<RpcMessage>();
            receiving.Enqueue(RpcMessage.Encode(new RpcMethod {
                ID = id,
                Name = "MyMethod"
            }));
            var channel = new ReceivingMockRpcChannel(receiving);
            // Start peer
            var rpcPeer = await RpcPeer.Create(new PeerInfo(null, "localhost"),
                channel, new MockExecutor_Result42(), backlog: null);
            _ = rpcPeer.Start();
            await Task.Delay(200); // Give a moment to execute
            // Check sent message
            Assert.AreEqual(1, channel.SentMessages.Count);
            Assert.AreEqual(new RpcResult {
                MethodID = id,
                ReturnValue = new byte[] { 42 }
            }, channel.SentMessages[0].DecodeRpcResult());
            rpcPeer.Stop();
        }

        private class MockExecutor_Result42 : IRpcMethodExecutor {
            public RpcOptions DefaultOptions { get; } = new RpcOptions();
            public Task<byte[]?> Execute(RpcMethod method) {
                byte[]? ret = new byte[] { 42 };
                return Task.FromResult(ret);
            }
        }

        /// <summary>
        /// Tests sending a call and receiving a response.
        /// </summary>
        [TestMethod, Timeout(200000)]
        public async Task Run_And_ReceiveResult_Test() {
            // Result to receive
            ulong id = 25;
            var responding = new Queue<RpcMessage>();
            responding.Enqueue(RpcMessage.Encode(new RpcResult {
                MethodID = id,
                ReturnValue = new byte[] { 42 }
            }));
            int responseTimeMs = 500;
            var channel = new RespondingMockChannel(responding, responseTimeMs);
            // Start peer
            var rpcPeer = await RpcPeer.Create(new PeerInfo(null, "localhost"),
                channel, new MockExecutor_Result42(), backlog: null);
            _ = rpcPeer.Start();
            var callTask = rpcPeer.Run(new RpcCall {
                Method = new RpcMethod {
                    ID = id,
                    Name = "MyMethod"
                }
            });
            // Wait shorter than the response time, nothing should be received yet
            await Task.Delay(responseTimeMs / 2);
            Assert.IsFalse(callTask.IsCompleted);
            // Give remaining time to execute
            await Task.Delay(responseTimeMs / 2 + 200);
            // Check received message
            Assert.IsTrue(callTask.IsCompleted);
            Assert.AreEqual(new RpcResult {
                MethodID = id,
                ReturnValue = new byte[] { 42 }
            }, callTask.Result);
            rpcPeer.Stop();
        }

    }

}
