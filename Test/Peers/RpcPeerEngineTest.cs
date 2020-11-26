using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xenoage.RpcLib.Connections;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Model;
using static Xenoage.RpcLib.Utils.CoreUtils;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Tests for <see cref="RpcPeerEngine"/>, using a <see cref="ReceivingMockRpcConnection"/>.
    /// </summary>
    [TestClass]
    public class RpcPeerEngineTest {

        [TestInitialize]
        public void Init() {
            Log.Instance = new DebugLogger(LogLevel.Trace);
        }

        [TestMethod, Timeout(1000)]
        public async Task Start_And_Stop_Test() {
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                new ReceivingMockRpcConnection(new Queue<(RpcMessage, int)>()),
                new MockRpcMethodExecutor(), backlog: null);
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
            var receiving = new Queue<(RpcMessage, int)>();
            receiving.Enqueue((RpcMessage.Encode(new RpcMethod {
                ID = id,
                Name = "MyMethod"
            }), 0));
            var connection = new ReceivingMockRpcConnection(receiving);
            // Start peer
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                connection, new MockRpcMethodExecutor(), backlog: null);
            _ = rpcPeer.Start();
            await Task.Delay(200); // Give a moment to execute
            // Check sent message
            Assert.AreEqual(1, connection.SentMessages.Count);
            Assert.AreEqual(new RpcResult {
                MethodID = id,
                ReturnValue = new byte[] { 42 }
            }, connection.SentMessages[0].DecodeRpcResult());
            rpcPeer.Stop();
        }

        /// <summary>
        /// Tests sending a call and receiving a response.
        /// </summary>
        [TestMethod, Timeout(2000)]
        public async Task Run_And_ReceiveResult_Test() {
            // Result to receive
            ulong id = 25;
            var responding = new Queue<RpcMessage>();
            responding.Enqueue(RpcMessage.Encode(new RpcResult {
                MethodID = id,
                ReturnValue = new byte[] { 42 }
            }));
            int responseTimeMs = 500;
            var connection = new RespondingMockRpcConnection(responding, responseTimeMs);
            // Start peer
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                connection, new MockRpcMethodExecutor(), backlog: null);
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

        /// <summary>
        /// Tests sending a call, whose timeout is hit even before the call was sent.
        /// </summary>
        [TestMethod, Timeout(1000)]
        public async Task Run_LocalTimeout_Test() {
            await Timeout_Test(timeoutMs: 1, waitBeforeAssertTimeoutMs: 200);
        }

        /// <summary>
        /// Tests sending a call, whose timeout is hit while processed
        /// on the remote side.
        /// </summary>
        [TestMethod, Timeout(1000)]
        public async Task Run_RemoteTimeout_Test() {
            await Timeout_Test(timeoutMs: 200, waitBeforeAssertTimeoutMs: 500);
        }

        private async Task Timeout_Test(int timeoutMs, int waitBeforeAssertTimeoutMs) {
            ulong id = 25;
            var responding = new Queue<RpcMessage>();
            int responseTimeMs = 500;
            var connection = new RespondingMockRpcConnection(responding, responseTimeMs);
            // Start peer
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                connection, new MockRpcMethodExecutor(), backlog: null);
            _ = rpcPeer.Start();
            var callTask = rpcPeer.Run(new RpcCall {
                Method = new RpcMethod {
                    ID = id,
                    Name = "MyMethod"
                },
                TimeoutMs = timeoutMs
            });
            // When we give more than 200 ms time to fail, check already after
            // 100 ms, then the task should still be open
            if (waitBeforeAssertTimeoutMs > 200) {
                await Task.Delay(100);
                waitBeforeAssertTimeoutMs -= 100;
                Assert.IsFalse(callTask.IsCompleted);
            }
            // Wait a short moment, the method call must have failed
            await Task.Delay(waitBeforeAssertTimeoutMs);
            Assert.IsTrue(callTask.IsCompleted);
            Assert.AreEqual(RpcFailureType.Timeout, callTask.Result.Failure?.Type);
            rpcPeer.Stop();
        }

        /// <summary>
        /// Tests retrying with a retryable method, that fails a few times because of
        /// timeout, and is then successful.
        /// </summary>
        [TestMethod, Timeout(5000)]
        public async Task Run_RetryUntilWorking() {
            var connection = new DivMockRpcConnection(enableReceivingCalls: false);
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                connection, new DivMockRpcMethodExecutor(), backlog: null);
            _ = rpcPeer.Start();
            // Set remote execution time very high, so that the response can not be received
            // during the following asserts. Each call should fail because of timeout.
            connection.SetExecutionTimeMs(100_000);
            long startTime = TimeNowMs();
            var callTask = rpcPeer.Run(new RpcCall {
                Method = Div.CreateNew(1000).ToMethod(),
                RetryStrategy = RpcRetryStrategy.Retry,
                TimeoutMs = 100
            });
            await callTask;
            long duration = TimeNowMs() - startTime;
            Assert.IsTrue(100 <= duration && duration < 200);
            Assert.AreEqual(RpcFailureType.Timeout, callTask.Result.Failure?.Type);
            // It should be repeated, but now we can not receive the result any more.
            // But we should observe the increasing number of sent attempts.
            int attempts = connection.SentDivs.Count;
            int newAttempts;
            for (int i = 0; i < 3; i++) {
                await Task.Delay(300);
                newAttempts = connection.SentDivs.Count;
                Assert.IsTrue(newAttempts > attempts && Math.Abs(newAttempts - attempts) <= 3,
                    $"Failed in step {i}, attempts = {attempts}, newAttempts = {newAttempts}");
                attempts = newAttempts;
            }
            // When we set the remote execution time down to 100, the call should be finished.
            attempts = connection.SentDivs.Count;
            connection.SetExecutionTimeMs(50);
            await Task.Delay(300);
            newAttempts = connection.SentDivs.Count;
            Assert.IsTrue(newAttempts == attempts + 1 || newAttempts == attempts + 2);
            rpcPeer.Stop();
        }

        /// <summary>
        /// <see cref="LoadTest"/> without timeouts.
        /// </summary>
        [TestMethod]
        public async Task LoadTest_NoTimeouts() {
            await LoadTest(useTimeouts: false);
        }

        /// <summary>
        /// <see cref="LoadTest"/> with timeouts.
        /// </summary>
        [TestMethod]
        public async Task LoadTest_WithTimeouts() {
            await LoadTest(useTimeouts: true);
        }

        /// <summary>
        /// Sends, receives and processes lot of calls with a number division calculation task,
        /// using the <see cref="DivMockRpcConnection"/>.
        /// At the end, checks if each of the calls, in both directions, was correctly processed.
        /// </summary>
        /// <param name="useTimeouts">Iff true, all <see cref="Div"/>s are sent as retryable
        ///   calls and a timeout of 50 ms is set (the RPC peer is not fast enough
        ///   to process a calls without timeouts)</param>
        private async Task LoadTest(bool useTimeouts) {
            int callsCount = 0;
            var connection = new DivMockRpcConnection(enableReceivingCalls: true);
            var testDurationMs = 3000;
            // Start peer
            var rpcPeer = await RpcPeerEngine.Create(new RpcPeerInfo(null, "localhost"),
                connection, new DivMockRpcMethodExecutor(), backlog: null);
            _ = rpcPeer.Start();
            // For the time of the test, send calculations
            long testStartTime = TimeNowMs();
            _ = Task.Run(async () => {
                while (TimeNowMs() - testStartTime < testDurationMs) {
                    var div = Div.CreateNew((ulong)callsCount);
                    _ = rpcPeer.Run(new RpcCall {
                        Method = div.ToMethod(),
                        RetryStrategy = useTimeouts ? RpcRetryStrategy.Retry : (RpcRetryStrategy?)null,
                        TimeoutMs = 50
                    });
                    callsCount++;
                    if (callsCount % 5 == 0)
                        await Task.Delay(20);
                }
            });
            // When we test timeouts, let the first second respond very slowly
            if (useTimeouts) {
                connection.SetExecutionTimeMs(100);
                await Task.Delay(1000);
                testDurationMs -= 1000;
                connection.SetExecutionTimeMs(0);
            }
            // When the test time is over, tell the mock connection to stop receiving new div tasks
            await Task.Delay(testDurationMs);
            connection.StopReceivingDivs();
            // Wait a short moment to let ongoing computations complete
            await Task.Delay(Debugger.IsAttached ? 2000 : 500); // More time when debugging (much slower taks and/or logging)
            // Check results of sent and received calls
            var sentCallsWithTimeouts = connection.SentDivs.ToList();
            var sentCalls = sentCallsWithTimeouts.Where((call, index) =>
                index == sentCallsWithTimeouts.FindLastIndex(it => it.methodID == call.methodID)).ToList(); // Filter out retried calls
            if (useTimeouts)
                Assert.IsTrue(sentCallsWithTimeouts.Count > sentCalls.Count + 10);
            else
                Assert.AreEqual(sentCallsWithTimeouts.Count, sentCalls.Count);
            Assert.AreEqual(callsCount, sentCalls.Count);
            for (int i = 0; i < sentCalls.Count; i++)
                Assert.AreEqual(sentCalls[i].ComputeExpectedResult(), sentCalls[i].result);
            var receivedCalls = connection.ReceivedDivs.ToList();
            Assert.IsTrue(receivedCalls.Count > testDurationMs / 100);
            for (int i = 0; i < receivedCalls.Count; i++)
                Assert.AreEqual(receivedCalls[i].ComputeExpectedResult(), receivedCalls[i].result);
        }

        private Random random = new Random();

    }

}
