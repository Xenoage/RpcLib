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
            new TestRpcMethods1().NotAnnotated();
            await new TestRpcMethods1().AsyncAnnotatedWithTimeout();
            new TestRpcMethods2().AnnotatedWithTimeoutAndRetry();
        }

        private static void TestAttributes(string methodName, int? expectedTimeoutMs, RpcRetryStrategy? expectedRetryStrategy) {
            var call = new RpcCall { Method = new RpcMethod(methodName) };
            RpcPeer.ApplyRpcOptionsFromCallStack(call);
            Assert.AreEqual(expectedTimeoutMs, call.TimeoutMs);
            Assert.AreEqual(expectedRetryStrategy, call.RetryStrategy);
        }

        private class TestRpcMethods1 : IRpcMethods {
            public StackTrace NotAnnotated() => new StackTrace();
            [RpcOptions(TimeoutMs = 1234)]
            public Task AsyncAnnotatedWithTimeout() {
                TestAttributes("AsyncAnnotatedWithTimeout", 1234, null);
                return Task.CompletedTask;
            }
        }

        private class TestRpcMethods2 : RpcMethodsStub {
            public TestRpcMethods2() : base(null!) {
            }
            [RpcOptions(TimeoutMs = 1234, RetryStrategy = RpcRetryStrategy.Retry)]
            public void AnnotatedWithTimeoutAndRetry() {
                TestAttributes("AnnotatedWithTimeoutAndRetry", 1234, RpcRetryStrategy.Retry);
            }
        }

    }

}
