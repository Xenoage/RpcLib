using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

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
        public async Task Retry_CorrectOrder() {
            int callsCount = 500;
            int clientsCount = 10;
            var random = new Random();
            var backlog = CreateInstance();
            var allTasks = new List<Task>();
            // Enqueue
            allTasks.Add(Task.Run(async () => {
                await Task.Delay(500); // Wait a moment, so that the test starts with an empty backlog
                for (int iCall = 0; iCall < callsCount; iCall++) {
                    string targetPeerID = "client" + random.Next(clientsCount);
                    backlog.Enqueue(CreateCall("TestMethod", targetPeerID));
                    await Task.Delay(random.Next(10));
                }
            }));
            // Peek and dequeue, with checking the order
            // Test each client individually
            int receivedCallsCount = 0;
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string clientID = "client" + iClient;
                allTasks.Add(Task.Run(async () => {
                    ulong lastID = 0;
                    while (receivedCallsCount < callsCount) {
                        if (backlog.TryPeek(clientID, out var peekedCall)) {
                            // Check client ID and that the ID is higher than the last one
                            Assert.AreEqual(clientID, peekedCall.TargetPeerID);
                            Assert.IsTrue(peekedCall.Method.ID > lastID);
                            // Dequeue
                            Assert.IsTrue(backlog.TryDequeue(clientID, out var dequeuedCall));
                            // Dequeued call must be the same call (same ID) as the previously peeked call
                            Assert.AreEqual(dequeuedCall.Method.ID, peekedCall.Method.ID);
                            receivedCallsCount++;
                        }
                    }
                    await Task.Delay(random.Next(10));
                }));
            }
            await Task.WhenAll(allTasks);
        }

        private RpcCall CreateCall(string methodName, string targetPeerID) => new RpcCall {
            Method = RpcMethod.Create(methodName),
            RetryStrategy = RpcRetryStrategy.Retry,
            TargetPeerID = targetPeerID,
            State = RpcCallState.Enqueued
        };

    }

}
