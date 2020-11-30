using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// Awaitable <see cref="RpcCall"/> in execution. Returns, when
    /// the result is there (whether successful, failed or timeout).
    /// Set the result, as soon as it is there, using <see cref="Finish"/>.
    /// </summary>
    public class RpcCallExecution {

        public RpcCallExecution(RpcCall call) {
            this.call = call;
        }

        /// <summary>
        /// Awaits the result for this execution, but only at maximum the given time in ms.
        /// In case of failure, no exception is thrown, but a result with failure information
        /// is returned.
        /// </summary>
        public async Task<RpcResult> AwaitResult(int timeoutMs) {
            try {
                // Return result, as soon as it is there

                // GOON return await completionHelper.Task.TimeoutAfter(timeoutMs);
                // >> replacement by active waiting
                long startTime = CoreUtils.TimeNowMs();
                while (startTime + timeoutMs > CoreUtils.TimeNowMs()) {
                    if (result != null)
                        return result;
                    await Task.Delay(50);
                }
                return new RpcResult {
                    MethodID = call.Method.ID,
                    Failure = new RpcFailure { Type = RpcFailureType.Timeout }
                };
                // << replacement by active waiting

            } catch (TimeoutException) {
                // Timeout
                return new RpcResult {
                    MethodID = call.Method.ID,
                    Failure = new RpcFailure { Type = RpcFailureType.Timeout }
                };
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
            this.result = result; // GOON completionHelper.TrySetResult(result);

        private RpcCall call;
        // GOON private TaskCompletionSource<RpcResult> completionHelper =
        //    new TaskCompletionSource<RpcResult>();
        private RpcResult? result = null;

    }

}
