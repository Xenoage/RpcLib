using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Utils {

    /// <summary>
    /// General useful functions.
    /// </summary>
    public static class CoreUtils {

        /// <summary>
        /// Current Unix timestamp in milliseconds.
        /// </summary>
        public static long TimeNowMs() =>
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Removes all elements of this linked list where the given condition is true.
        /// </summary>
        public static void RemoveAll<T>(this LinkedList<T> linkedList, Func<T, bool> predicate) {
            for (var node = linkedList.First; node != null;) {
                var next = node.Next;
                if (predicate(node.Value))
                    linkedList.Remove(node);
                node = next;
            }
        }

        /// <summary>
        /// Awaits all these tasks, but no longer than the given timeout in milliseconds (up to 100 ms tolerance).
        /// Returns true, when all tasks could be finished, otherwise false (timeout).
        /// </summary>
        public static async Task<bool> AwaitAll(this IEnumerable<Task> tasks, int timeoutMs) {
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

    }

}
