using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    [TestClass]
    public class BlockingQueueTest {

        [TestMethod]
        public async Task TestDequeue() {
            var maxCount = 4;
            var queue = new BlockingQueue<string>(maxCount);
            // Dequeue when empty: await timeout
            int timeoutMs = 500;
            long time = await MeasureTime(queue.Dequeue(timeoutMs));
            Assert.IsTrue(time >= timeoutMs, $"Dequeue timeout too early: {time}");
            Assert.IsTrue(time <= timeoutMs + 50, $"Dequeue timeout too late: {time}");
            // Dequeue, wait a moment, then add an item, and it must be dequeued immediately
            int expectedTime = 300;
            string expectedReturnValue = "item";
            string returnValue = "";
            time = await MeasureTime(Task.Run(async () => {
                var task = queue.Dequeue(timeoutMs);
                await Task.Delay(expectedTime);
                queue.Enqueue(expectedReturnValue);
                returnValue = await task;
            }));
            Assert.AreEqual(expectedReturnValue, returnValue, $"Dequeue returned wrong value: {returnValue}");
            Assert.IsTrue(time >= expectedTime, $"Dequeue return too early: {time}");
            Assert.IsTrue(time <= expectedTime + 50, $"Dequeue return too late: {time}");
        }

        private async Task<long> MeasureTime(Task task) {
            long startTime = CoreUtils.TimeNow();
            await task;
            long endTime = CoreUtils.TimeNow();
            return endTime - startTime;
        }

    }

}
