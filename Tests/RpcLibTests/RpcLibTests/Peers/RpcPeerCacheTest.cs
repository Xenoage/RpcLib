using Microsoft.VisualStudio.TestTools.UnitTesting;
using RpcLib.Model;
using RpcLib.Server;
using RpcLib.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RpcLib.Peers {

    [TestClass]
    public class RpcPeerCacheTest {

        [TestMethod]
        public async Task Test_NoBacklog_Serial() {
            var cache = new RpcPeerCache("test", null);
            var commands = new List<RpcCommand>();
            int count = 100;
            // Enqueue all commands
            for (int i = 0; i < count; i++) {
                var command = CreateRpcCommand();
                commands.Add(command);
                cache.EnqueueCommand(command);
            }
            // Dequeue all commands. Must be in right order
            for (int i = 0; i < count; i++) {
                var expectedCommand = i == 0 ? null : commands[i - 1];
                Assert.AreEqual(expectedCommand, cache.CurrentCommand, $"Unexpected command #{i}");
                await cache.DequeueCommand(1000);
                expectedCommand = commands[i];
                Assert.AreEqual(expectedCommand, cache.CurrentCommand, $"Unexpected command #{i}");
            }
        }

        [TestMethod]
        public async Task Test_NoBacklog_Parallel() {
            var cache = new RpcPeerCache("test", null);
            var commands = new List<RpcCommand>();
            int count = 100;

            var random = new Random();
            var taskEnqueue = Task.Run(async () => {
                // Enqueue commands
                for (int i = 0; i < count; i++) {
                    var command = CreateRpcCommand();
                    commands.Add(command);
                    cache.EnqueueCommand(command);
                    await Task.Delay(random.Next(1, 100 + i * 5)); // Increase pause over time
                }
            });
            var taskDequeue = Task.Run(async () => {
                // Dequeue commands. Must be in right order
                for (int i = 0; i < count; i++) {
                    var command = await cache.DequeueCommand(100);
                    // Null is possible, the queue is empty in this case (producer too slow)
                    if (command == null) {
                        i = i - 1; // Try again
                    } else {
                        Assert.AreEqual(commands[i], cache.CurrentCommand, $"Unexpected command #{i}");
                    }
                }
            });
            try {
                await Task.WhenAll(taskEnqueue, taskDequeue);
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }

        private RpcCommand CreateRpcCommand() =>
            RpcCommand.CreateForClient("test", "MyMethod", CoreUtils.TimeNow());

    }

}
