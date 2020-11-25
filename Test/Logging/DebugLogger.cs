using System;
using System.Diagnostics;

namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// A simple logger to the debug console.
    /// </summary>
    public class DebugLogger : ILogger {

        /// <summary>
        /// The minimum level for log messages to be included in the log.
        /// Lower-level messages will be ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; }

        public DebugLogger(LogLevel minimumLevel) {
            MinimumLevel = minimumLevel;
        }

        public void Log(string message, LogLevel level) {
            if (level >= MinimumLevel) {
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss .fff").PadRight(27) +
                    $"{level}".PadRight(8) + message;
                Debug.WriteLine(line);
            }
        }
    }

}
