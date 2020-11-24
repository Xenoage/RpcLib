using System.Threading.Tasks;
using Xenoage.RpcLib.Serialization;

namespace Xenoage.RpcLib.Methods {

    /// <summary>
    /// Extension methods for <see cref="Task"/>.
    /// This class is intentionally in the same namespace as <see cref="RpcMethods"/>, so that
    /// these extension methods are immediately visible in a <see cref="RpcMethods"/> implementation
    /// without needing to include a using-statement for another package.
    /// </summary>
    public static class TaskEx {

        /// <summary>
        /// Extension method for task without a return value.
        /// Runs the given task value and returns null.
        /// </summary>
        public static async Task<byte[]?> Serialize(this Task task) {
            await task;
            return null;
        }

        /// <summary>
        /// Extension method for task with a return value.
        /// Runs the given task and returns the serialized return value.
        /// </summary>
        public static async Task<byte[]?> Serialize<T>(this Task<T> task) {
            var ret = await task;
            return ret != null ? Serializer.Serialize(ret) : null;
        }

    }
}
