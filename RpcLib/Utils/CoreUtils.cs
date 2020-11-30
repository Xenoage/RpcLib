using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        /// Awaits this tasks, but no longer than the given timeout in milliseconds.
        /// Returns with the result of the task when finished, otherwise re-throws the exception of the
        /// task execution or a <see cref="TimeoutException"/> when the timeout is reached.
        /// Original source: https://stackoverflow.com/a/22078975/518491 .
        /// </summary>
        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int timeoutMs) {
            using (var timeoutHelper = new CancellationTokenSource()) {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs, timeoutHelper.Token));
                if (completedTask == task) {
                    timeoutHelper.Cancel();
                    return await task; // It's already completed, but in this way we get
                                       // notified if there was an Exception
                } else {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Awaits these tasks, but no longer than the given timeout in milliseconds.
        /// If there is a problem, re-throws the exception of the first failing task
        /// or throws a <see cref="TimeoutException"/> when the timeout is reached.
        /// </summary>
        public static async Task TimeoutAfter(this List<Task> tasks, int timeoutMs) {
            using (var timeoutHelper = new CancellationTokenSource()) {
                var allTasks = new List<Task>(tasks.Count + 1);
                allTasks.AddRange(tasks);
                var timeoutTask = Task.Delay(timeoutMs, timeoutHelper.Token);
                allTasks.Add(timeoutTask);
                var completedTask = await Task.WhenAny(allTasks);
                if (completedTask != timeoutTask) {
                    timeoutHelper.Cancel();
                    await completedTask; // It's already completed, but in this way we get
                                         // notified if there was an Exception
                } else {
                    throw new TimeoutException();
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource"/>, which runs its
        /// continuations asynchronously. This is crucial to avoid deadlocks in our library.
        /// See https://devblogs.microsoft.com/premier-developer/the-danger-of-taskcompletionsourcet-class/
        /// </summary>
        public static TaskCompletionSource<T> CreateAsyncTaskCompletionSource<T>() =>
            new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

    }

}
