using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Utils {

    /// <summary>
    /// Tests for <see cref="CoreUtils"/>.
    /// </summary>
    [TestClass]
    public class CoreUtilsTest {

        [TestMethod]
        public async Task TimeoutAfter_Single_Success() {
            await TimeoutAfter_Test(success: true, tasksCount: 1);
        }

        [TestMethod]
        public async Task TimeoutAfter_Single_Failed() {
            await TimeoutAfter_Test(success: false, tasksCount: 1);
        }

        [TestMethod]
        public async Task TimeoutAfter_List_Success() {
            await TimeoutAfter_Test(success: true, tasksCount: 5);
        }

        [TestMethod]
        public async Task TimeoutAfter_List_Failed() {
            await TimeoutAfter_Test(success: false, tasksCount: 5);
        }

        /// <summary>
        /// Tests for different combinations of execution time and timeout
        /// if <see cref="CoreUtils.TimeoutAfter"/> works for successful or failing tasks.
        /// Tests with the given number of tasks at the same time.
        /// </summary>
        private async Task TimeoutAfter_Test(bool success, int tasksCount) {
            for (int timeoutMs = 50; timeoutMs <= 350; timeoutMs += 100) {
                for (int completionMs = 0; completionMs <= 300; completionMs += 100) {
                    try {
                        if (tasksCount == 1) {
                            // Test the method with a single task
                            var task = Compute42(TimeSpan.FromMilliseconds(completionMs), success);
                            await task.TimeoutAfter(timeoutMs);
                        }
                        else {
                            // Test the method with a list of tasks
                            var tasks = Enumerable.Range(0, tasksCount).Select(_ =>
                                Compute(TimeSpan.FromMilliseconds(completionMs), success)).ToList();
                            await tasks.TimeoutAfter(timeoutMs);
                        }
                        Assert.IsTrue(timeoutMs > completionMs, "Timeout did not fire");
                    } catch (AssertFailedException) {
                        throw;
                    } catch (TimeoutException) {
                        Assert.IsTrue(timeoutMs < completionMs, "Timeout fired too early");
                    } catch {
                        Assert.IsFalse(success, "Task failed, but it should succeed");
                    }
                }
            }
        }

        
        /// <summary>
        /// After the given time, returns 42 or throws an Exception if success is false.
        /// </summary>
        private async Task<int> Compute42(TimeSpan duration, bool success) {
            await Task.Delay(duration);
            if (success)
                return 42;
            else
                throw new Exception();
        }

        /// <summary>
        /// After the given time, returns or throws an Exception with message "MyEx".
        /// </summary>
        private async Task Compute(TimeSpan duration, bool success) {
            await Task.Delay(duration);
            if (false == success)
                throw new Exception();
        }


    }

}
