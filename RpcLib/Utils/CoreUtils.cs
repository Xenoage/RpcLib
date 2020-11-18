using System;

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

    }

}
