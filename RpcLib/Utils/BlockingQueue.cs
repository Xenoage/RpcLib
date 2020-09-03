using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace RpcLib.Utils {

    /// <summary>
    /// A blocking queue implementation.
    /// It contains methods for enqueuing and blocking dequeuing.
    /// TODO: improve this class by using only a single collection (we need some blocking queue we can also peek).
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
            blockingQueue.Count;

        /// <summary>
        /// Adds an object to the end of the queue.
        /// Throws an exception if the queue would become longer than <see cref="Size"/> items.
        /// </summary>
        public void Enqueue(T item) {
            lock (this) {
                if (blockingQueue.Count >= Size)
                    throw new Exception("Queue is full");
                blockingQueue.Post(item);
                peekQueue.Enqueue(item);
            }
        }

        /// <summary>
        /// Tries to remove and return the object at the beginning of the queue
        /// in the specified time period in milliseconds. Returns the value, as soon as there
        /// is an item, or null after the timeout.
        /// </summary>
        public async Task<T?> Dequeue(int timeoutMs) {
            try {
                var ret = await blockingQueue.ReceiveAsync(TimeSpan.FromMilliseconds(timeoutMs));
                peekQueue.Dequeue();
                return ret;
            }
            catch {
                return null;
            }
        }

        /// <summary>
        /// Immediately peeks the first element of this queue.
        /// When there is none, null is returned.
        /// </summary>
        public T? Peek() {
            lock (this) {
                if (peekQueue.Count == 0)
                    return null;
                return peekQueue.Peek();
            }
        }

        private BufferBlock<T> blockingQueue = new BufferBlock<T>();
        private Queue<T> peekQueue = new Queue<T>(); // Just needed to support Peek() in this class...

    }

}
