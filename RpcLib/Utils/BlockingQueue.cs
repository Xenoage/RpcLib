using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RpcLib.Utils {

    /// <summary>
    /// A blocking queue implementation based on <see cref="BlockingCollection"/>.
    /// It contains methods for enqueuing and blocking dequeuing.
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
            try {
                return await queue.ReceiveAsync(TimeSpan.FromMilliseconds(timeoutMs));
            }
            catch {
                return null;
            }
        }

        private BufferBlock<T> queue = new BufferBlock<T>();

    }

}
