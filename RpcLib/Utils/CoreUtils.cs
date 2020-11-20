using System;
using System.Collections.Generic;

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

    }

}
