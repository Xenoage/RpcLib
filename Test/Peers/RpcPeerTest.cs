using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {
    
    /// <summary>
    /// Tests for <see cref="RpcPeer"/>.
    /// </summary>
    [TestClass]
    public class RpcPeerTest {

        [TestMethod]
        public async Task ApplyRpcOptionsFromCallStack_Test() {
            await new TestRpcMethodsStub().NotAnnotated();
            await new TestRpcMethodsStub().AsyncAnnotatedWithTimeout();
            await new TestRpcMethodsStub().AnnotatedWithTimeoutAndRetry();
        }

        private static void TestAttributes(string methodName, int? expectedTimeoutMs, RpcRetryStrategy? expectedRetryStrategy) {
            var call = new RpcCall { Method = new RpcMethod(methodName) };
            RpcPeer.ApplyRpcOptionsFromCallStack(call);
            Assert.AreEqual(expectedTimeoutMs, call.TimeoutMs);
            Assert.AreEqual(expectedRetryStrategy, call.RetryStrategy);
        }

        private class TestRpcMethodsStub : RpcMethodsStub, ITestRpcMethods {

            public TestRpcMethodsStub() : base(null!) {
            }

            public Task NotAnnotated() => Task.CompletedTask;

            public Task<int> AsyncAnnotatedWithTimeout() {
                TestAttributes("AsyncAnnotatedWithTimeout", 1234, null);
                return Task.FromResult(42);
            }

            public Task AnnotatedWithTimeoutAndRetry() {
                TestAttributes("AnnotatedWithTimeoutAndRetry", 1234, RpcRetryStrategy.Retry);
                return Task.CompletedTask;
            }
        }

        private interface ITestRpcMethods : IRpcMethods {

            public Task NotAnnotated() => Task.CompletedTask;

            [RpcOptions(TimeoutMs = 1234)]
            public Task<int> AsyncAnnotatedWithTimeout();

            [RpcOptions(TimeoutMs = 1234, RetryStrategy = RpcRetryStrategy.Retry)]
            public Task AnnotatedWithTimeoutAndRetry();
        }

    }

}
