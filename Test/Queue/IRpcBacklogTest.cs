using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for all implementations of <see cref="IRpcBacklog"/>.
    /// The program restart is simulated by creating a new instance using <see cref="CreateInstance"/>.
    /// Notice: Thus, this test will not work correctly if the backlog implementation uses static fields.
    /// </summary>
    [TestClass]
    public abstract class IRpcBacklogTest {

        /// <summary>
        /// Requests a new instance of this backlog implementation.
        /// </summary>
        protected abstract IRpcBacklog CreateInstance();
        

        /// <summary>
        /// Enqueues some calls for different target peers.
        /// </summary>
        [TestMethod]
        public async Task Add_CountByReadAll() {
            int callsCount = 500;
            int clientsCount = 5;
            int[] countByClient = new int[clientsCount];
            // Add
            for (int iCall = 0; iCall < callsCount; iCall++) {
                int clientIndex = random.Next(clientsCount);
                string targetPeerID = "client" + clientIndex;
                await backlog.Add(CreateCall("TestMethod", targetPeerID));
                countByClient[clientIndex]++;
                // Read all to find out number of calls
                int callsCountOfThisClient = (await backlog.ReadAll(targetPeerID)).Count;
                Assert.AreEqual(countByClient[clientIndex], callsCountOfThisClient);
            }
        }

        /// <summary>
        /// Enqueues some calls for different target peers.
        /// They must all exist after enqueuing and even after "restart".
        /// </summary>
        [TestMethod]
        public async Task ReadAll_AfterRestart() {
            int callsCount = 500;
            int clientsCount = 5;
            List<List<RpcCall>> callsByClient =
                Enumerable.Range(0, clientsCount).Select(it => new List<RpcCall>()).ToList();
            // Add
            for (int iCall = 0; iCall < callsCount; iCall++) {
                int clientIndex = random.Next(clientsCount);
                string targetPeerID = "client" + clientIndex;
                var call = CreateCall("TestMethod" + random.Next(10), targetPeerID);
                await backlog.Add(call);
                callsByClient[clientIndex].Add(call);
            }
            // Test after restart, if still the same
            SimulateRestart();
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string targetPeerID = "client" + iClient;
                // Read all and compare
                var calls = new List<RpcCall>(await backlog.ReadAll(targetPeerID));
                Assert.AreEqual(callsByClient[iClient].Count, calls.Count);
                for (int iCall = 0; iCall < calls.Count; iCall++)
                    Assert.AreEqual(callsByClient[iClient][iCall], calls[iCall]);
            }
        }

        /// <summary>
        /// Removes some specific calls by method ID.
        /// </summary>
        [TestMethod]
        public async Task RemoveByMethodID_Test() {
            int callsCount = 500;
            int clientsCount = 5;
            List<List<RpcCall>> callsByClient =
                Enumerable.Range(0, clientsCount).Select(it => new List<RpcCall>()).ToList();
            List<RpcCall> allCalls = new List<RpcCall>();
            // Add
            for (int iCall = 0; iCall < callsCount; iCall++) {
                int clientIndex = random.Next(clientsCount);
                string targetPeerID = "client" + clientIndex;
                var call = CreateCall("TestMethod" + random.Next(10), targetPeerID);
                await backlog.Add(call);
                callsByClient[clientIndex].Add(call);
                allCalls.Add(call);
            }
            // Remove some calls
            for (int iRemove = 0; iRemove < allCalls.Count; iRemove += random.Next(50)) {
                var callToRemove = allCalls[iRemove];
                await backlog.RemoveByMethodID(callToRemove.RemotePeerID, callToRemove.Method.ID);
                allCalls.RemoveAt(iRemove);
                callsByClient.ForEach(list => list.RemoveAll(it => it.Method.ID == callToRemove.Method.ID));
            }
            // Test if calls were removed
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string targetPeerID = "client" + iClient;
                // Read all and compare
                var calls = new List<RpcCall>(await backlog.ReadAll(targetPeerID));
                Assert.AreEqual(callsByClient[iClient].Count, calls.Count);
                for (int iCall = 0; iCall < calls.Count; iCall++)
                    Assert.AreEqual(callsByClient[iClient][iCall], calls[iCall]);
            }
        }

        /// <summary>
        /// Removes some calls by method name.
        /// </summary>
        [TestMethod]
        public async Task RemoveByMethodName_Test() {
            int callsCount = 500;
            int clientsCount = 5;
            List<List<RpcCall>> callsByClient =
                Enumerable.Range(0, clientsCount).Select(it => new List<RpcCall>()).ToList();
            // Add
            for (int iCall = 0; iCall < callsCount; iCall++) {
                int clientIndex = random.Next(clientsCount);
                string targetPeerID = "client" + clientIndex;
                var call = CreateCall("TestMethod" + random.Next(10), targetPeerID);
                await backlog.Add(call);
                callsByClient[clientIndex].Add(call);
            }
            // Remove all calls with method ID "TestMethod5"
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string targetPeerID = "client" + iClient;
                await backlog.RemoveByMethodName(targetPeerID, "TestMethod5");
                callsByClient[iClient].RemoveAll(it => it.Method.Name == "TestMethod5");
            }
            // Test if calls were removed
            for (int iClient = 0; iClient < clientsCount; iClient++) {
                string targetPeerID = "client" + iClient;
                // Read all and compare
                var calls = new List<RpcCall>(await backlog.ReadAll(targetPeerID));
                Assert.AreEqual(callsByClient[iClient].Count, calls.Count);
                for (int iCall = 0; iCall < calls.Count; iCall++)
                    Assert.AreEqual(callsByClient[iClient][iCall], calls[iCall]);
            }
        }

        /// <summary>
        /// Tests the target peer ID null (server).
        /// </summary>
        [TestMethod]
        public async Task Add_Server() {
            int callsCount = 10;
            // Add
            for (int iCall = 0; iCall < callsCount; iCall++)
                await backlog.Add(CreateCall("TestMethod", targetPeerID: null));
            // Read all to find out number of calls
            int actualCallsCount = (await backlog.ReadAll(targetPeerID: null)).Count;
            Assert.AreEqual(callsCount, actualCallsCount);
        }


        [TestInitialize]
        public void Init() {
            backlog = CreateInstance();
        }

        /// <summary>
        /// Simulates a restart by creating a new backlog instance.
        /// </summary>
        private void SimulateRestart() {
            backlog = CreateInstance();
        }

        private RpcCall CreateCall(string methodName, string? targetPeerID) => new RpcCall {
            Method = RpcMethod.Create(methodName),
            RetryStrategy = RpcRetryStrategy.Retry,
            RemotePeerID = targetPeerID
        };

        private IRpcBacklog backlog;
        private Random random = new Random();

    }

}
