using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RpcLib.Utils {

    /// <summary>
    /// A blocking queue implementation based on <see cref="BlockingCollection"/>.
    /// It contains methods for enqueuing, blocking dequeuing and blocking peeking.
    /// </summary>
    public class BlockingQueue<T> where T : class {

        public BlockingQueue(int size) {
            Size = size;
        }
        
        /// <summary>
        /// The maximum number of items in the queue.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The current number of items in the queue.
        /// </summary>
        public int Count =>
            queue.Count;

        /// <summary>
        /// Adds an object to the end of the queue.
        /// Throws an exception if the queue would become longer than <see cref="Size"/> items.
        /// </summary>
        public void Enqueue(T item) {
            if (queue.Count >= Size)
                throw new Exception("Queue is full");
            queue.Post(item);
        }

        /// <summary>
        /// Tries to remove and return the object at the beginning of the queue
        /// in the specified time period in milliseconds. Returns the value, as soon as there
        /// is an item, or null after the timeout.
        /// </summary>
        public async Task<T?> Dequeue(int timeoutMs) {
            // When the next element is set, return and remove it immediately
            lock (queue) {
                if (next != null) {
                    var ret = next;
                    next = null;
                    return ret;
                }
            }
            // Otherwise dequeue first element in queue (blocking)
            try {
                return await queue.ReceiveAsync(TimeSpan.FromMilliseconds(timeoutMs));
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// Tries to return an object from the beginning of the queue without removing it
        /// in the specified time period in milliseconds. Returns the value, as soon as there
        /// is an item, or null after the timeout.
        /// </summary>
        public async Task<T?> Peek(int timeoutMs) {
            // When the next element is set, return it immediately
            lock (queue) {
                if (next != null)
                    return next;
            }
            // Otherwise read first element in queue (blocking) and remember it
            // as the next element
            try {
                next = await queue.ReceiveAsync(TimeSpan.FromMilliseconds(timeoutMs));
                return next;
            }
            catch {
                return null;
            }
        }

        private BufferBlock<T> queue = new BufferBlock<T>();
        private T? next = null;

    }

}
