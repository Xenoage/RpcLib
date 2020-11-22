using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for <see cref="JsonFileRpcBacklog_Old"/>, using the <see cref="IRpcBacklogTest_Old"/>.
    /// </summary>
    [TestClass]
    public class JsonFileRpcBacklogTest_Old : IRpcBacklogTest_Old {

        private const string backlogDir = "RpcBacklog";

        protected override IRpcBacklog_Old CreateInstance() =>
            new JsonFileRpcBacklog_Old(new DirectoryInfo(backlogDir));

        [TestInitialize]
        public void Init() {
            try {
                Directory.Delete(backlogDir, recursive: true);
            } catch {
            }
        }

        /*
        [TestMethod]
        public async Task ThisWorks() {
            await Task.Run(async () => {
                await Task.Delay(1000);
                Assert.Fail("Works!");
            });
        }

        [TestMethod]
        public async Task ThisDoesNOTWork() {
            _ = Task.Run(async () => {
                await Task.Delay(1000);
                Assert.Fail("Has no effect :-( ");
            });
            await Task.Delay(2000);
        }

        [TestMethod]
        public async Task WorkingWorkaround() {
            string? myError = null;
            _ = Task.Run(async () => {
                await Task.Delay(1000);
                // Assert.Fail("Has no effect :-( ");
                myError = "Yes, it failed";
            });
            await Task.Delay(2000);
            if (myError != null)
                Assert.Fail(myError);
        }

        [TestMethod]
        public async Task Test_WhenAll() {
            var myTask = Task.Run(async () => {
                await Task.Delay(1000);
                Assert.Fail("I want this failure to be reported");
            });
            await Task.WhenAll(myTask, Task.Delay(2000));
        }*/

    }

    /*public void Clear() {
        lock (syncLock) {
            Directory.Delete(baseDir, recursive: true);
        }
    }*/

}
