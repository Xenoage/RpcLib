using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// Extension methods for <see cref="Task"/>.
    /// </summary>
    public static class TaskUtils {

        /// <summary>
        /// Runs the given task without a return value and returns null.
        /// </summary>
        public static async Task<string?> ToJson(this Task task) {
            await task;
            return null;
        }

        /// <summary>
        /// Runs the given task and returns the JSON-encoded return value.
        /// </summary>
        public static async Task<string?> ToJson<T>(this Task<T> task) {
            var ret = await task;
            return ret != null ? RpcMain.JsonLib.ToJson(ret) : null;
        }

    }
}
