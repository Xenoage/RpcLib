using System;

namespace RpcLib.Utils {

    /// <summary>
    /// Useful functions.
    /// </summary>
    public class CoreUtils {

        /// <summary>
        /// Current Unix timestamp in milliseconds.
        /// </summary>
        public static long TimeNow() =>
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    }
}
