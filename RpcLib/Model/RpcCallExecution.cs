using System;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Awaitable <see cref="RpcCall"/> in execution. Returns, when
    /// the result is there (whether successful or failed).
    /// Set the result, as soon as it is there, using <see cref="Finish"/>.
    /// </summary>
    public class RpcCallExecution {

        public RpcCallExecution(RpcCall call) {
            this.call = call;
        }

        /// <summary>
        /// Awaits the result for this execution.
        /// In case of failure, no exception is thrown, but a result with failure information is returned.
        /// </summary>
        public async Task<RpcResult> AwaitResult() {
            try {
                // Return result, as soon as it is there
                return await completionHelper.Task;
            } catch (Exception ex) {
                // Other error
                return new RpcResult {
                    MethodID = call.Method.ID,
                    Failure = new RpcFailure {
                        Type = RpcFailureType.Other,
                        Message = ex.Message
                    }
                };
            }
        }

        /// <summary>
        /// Call this method as soon as a result was received for this call.
        /// </summary>
        public void Finish(RpcResult result) =>
            completionHelper.TrySetResult(result);

        private RpcCall call;
        private TaskCompletionSource<RpcResult> completionHelper =
            new TaskCompletionSource<RpcResult>();

    }

}
