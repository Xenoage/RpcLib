using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for <see cref="RpcQueue"/>.
    /// </summary>
    [TestClass]
    public class RpcQueueTest {

        private const string backlogDir = "RpcBacklog";

        [TestInitialize]
        public void InitDir() {
            try {
                Directory.Delete(backlogDir, recursive: true);
            } catch {
            }
        }

        /// <summary>
        /// Creates a queue with some items, using a JSON file backlog.
        /// Create a new queue with the same backlog. Items must be restored.
        /// </summary>
        [TestMethod]
        public async Task Create_RestoreFromBacklog() {
            var backlog = new JsonFileRpcBacklog(new DirectoryInfo(backlogDir));
            string targetPeerID = "TestClient";
            int callsCount = 10;
            // Create queue and enqueue
            var queue = await RpcQueue.Create(targetPeerID, backlog);
            for (int i = 0; i < callsCount; i++)
                await queue.Enqueue(CreateCall("TestMethod", targetPeerID));
            // Recreate queue and check contents
            queue = await RpcQueue.Create(targetPeerID, backlog);
            Assert.AreEqual(callsCount, queue.Count);
        }

        /// <summary>
        /// Over time, enqueues some calls, peeks and dequeues them.
        /// The order must be retained.
        /// </summary>
        [TestMethod]
        public async Task Enqueue_And_Peek_And_Dequeue_CorrectOrder() {
            int callsCount = 100;
            string targetPeerID = "TestClient";
            var backlog = new JsonFileRpcBacklog(new DirectoryInfo(backlogDir));
            var queue = await RpcQueue.Create(targetPeerID, backlog);
            var allTasks = new List<Task>();
            // Enqueue
            allTasks.Add(Task.Run(async () => {
                await Task.Delay(1000); // Wait a moment, so that the test starts with an empty backlog
                for (int iCall = 0; iCall < callsCount; iCall++) {
                    await queue.Enqueue(CreateCall("TestMethod", targetPeerID));
                    await Task.Delay(50);
                }
            }));
            // Peek and dequeue, with checking the order
            allTasks.Add(Task.Run(async () => {
                ulong lastID = 0;
                int receivedCallsCount = 0;
                while (receivedCallsCount < callsCount) {
                    if (await queue.Peek() is RpcCall peekedCall) {
                        // Check that the ID is higher than the last one
                        Assert.IsTrue(peekedCall.Method.ID > lastID);
                        // Dequeue
                        var dequeuedCall = await queue.Dequeue();
                        // Dequeued call must be the same call (same ID) as the previously peeked call
                        Assert.AreEqual(dequeuedCall!.Method.ID, peekedCall.Method.ID);
                        lastID = dequeuedCall.Method.ID;
                        receivedCallsCount++;
                    }
                    await Task.Delay(50);
                }
            }));
            // Give time to finish, but only a certain amount of time
            if (false == await AwaitAll(allTasks, timeoutMs: 20_000))
                Assert.Fail($"Timeout");
            // Backlog must also be empty now
            Assert.AreEqual(0, (await backlog.ReadAll(targetPeerID)).Count);
        }


        /// <summary>
        /// Awaits the given tasks, but no longer than the given timeout in milliseconds (up to 100 ms tolerance).
        /// Returns true, when all tasks could be finished, otherwise false (timeout).
        /// </summary>
        private async Task<bool> AwaitAll(IEnumerable<Task> tasks, int timeoutMs) {
            for (int t = 0; t < timeoutMs; t += 100) {
                foreach (var task in tasks)
                    if (task.IsCompleted)
                        await task; // It's already completed, but in this way we get
                                    // notified if there was an Assert failure/Exception
                if (tasks.All(it => it.IsCompleted))
                    return true; // All tasks successfully finished
                await Task.Delay(100);
            }
            return false;
        }

        private RpcCall CreateCall(string methodName, string targetPeerID) => new RpcCall {
            Method = RpcMethod.Create(methodName),
            RetryStrategy = RpcRetryStrategy.Retry,
            TargetPeerID = targetPeerID
        };

        private Random random = new Random();

    }

}
