using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for all implementations of <see cref="IRpcBacklog"/>.
    /// Persistent backlogs are tested for retaining the queue after a program restart
    /// by simulating the restart by creating a new instance using <see cref="CreateInstance"/>.
    /// </summary>
    [TestClass]
    public abstract class IRpcBacklogTest {

        /// <summary>
        /// Requests a new instance of this backlog implementation.
        /// </summary>
        protected abstract IRpcBacklog CreateInstance();

        /// <summary>
        /// Over time, enqueues some calls for different target peers, peeks and dequeues them.
        /// The order must be retained.
        /// </summary>
        [TestMethod]
        public async Task Enqueue_All() {
            int callsCount = 500;
            int clientsCount = 5;
            var backlog = CreateInstance();
            // Enqueue - equally distributed on the clients
            for (int iCall = 0; iCall < callsCount; iCall++) {
                string targetPeerID = "client" + (iCall % clientsCount);
                await backlog.Enqueue(CreateCall("TestMethod", targetPeerID));
            }
            // All tasks must have been enqueued
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                int count = await backlog.GetCount("client" + iClient);
                Assert.AreEqual(callsCount / clientsCount, count);
            }
        }

        /// <summary>
        /// Over time, enqueues some calls for different target peers, peeks and dequeues them.
        /// The order must be retained.
        /// </summary>
        [TestMethod]
        public async Task Peek_And_Dequeue_CorrectOrder() {
            int callsCount = 500;
            int clientsCount = 1;
            var random = new Random();
            var backlog = CreateInstance();
            var allTasks = new List<Task>();
            // Enqueue
            allTasks.Add(Task.Run(async () => {
                await Task.Delay(2000); // Wait a moment, so that the test starts with an empty backlog
                for (int iCall = 0; iCall < callsCount; iCall++) {
                    string targetPeerID = "client" + (iCall % clientsCount);
                    await backlog.Enqueue(CreateCall("TestMethod", targetPeerID));
                    await Task.Delay(10);
                }
            }));
            // Peek and dequeue, with checking the order
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string clientID = "client" + iClient;
                allTasks.Add(Task.Run(async () => {
                    ulong lastID = 0;
                    int receivedCallsCount = 0;
                    while (receivedCallsCount < callsCount / clientsCount) {
                        if (await backlog.Peek(clientID) is RpcCall peekedCall) {
                            // Check client ID and that the ID is higher than the last one
                            Assert.AreEqual(clientID, peekedCall.TargetPeerID);
                            Assert.IsTrue(peekedCall.Method.ID > lastID);
                            // Dequeue
                            var dequeuedCall = await backlog.Dequeue(clientID);
                            Assert.IsTrue(clientID == dequeuedCall?.TargetPeerID);
                            // Dequeued call must be the same call (same ID) as the previously peeked call
                            Assert.AreEqual(dequeuedCall!.Method.ID, peekedCall.Method.ID);
                            lastID = dequeuedCall.Method.ID;
                            receivedCallsCount++;
                        }
                        // Dequeue slower than enqueue, so that the queues become longer over time
                        await Task.Delay(10);
                    }
                }));
            }
            // Give time to finish, but only a certain amount of time
            if (false == await AwaitAll(allTasks, timeoutMs: 20_000))
                Assert.Fail($"Timeout");
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

        //protected class IRpcBacklogProxy: 

        private RpcCall CreateCall(string methodName, string targetPeerID) => new RpcCall {
            Method = RpcMethod.Create(methodName),
            RetryStrategy = RpcRetryStrategy.Retry,
            TargetPeerID = targetPeerID,
            State = RpcCallState.Enqueued
        };

    }

}
