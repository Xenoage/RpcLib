using System;

namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// A simple logger to the console.
    /// </summary>
    public class ConsoleLogger : ILogger {

        /// <summary>
        /// The minimum level for log messages to be included in the log.
        /// Lower-level messages will be ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; }

        public ConsoleLogger(LogLevel minimumLevel) {
            MinimumLevel = minimumLevel;
        }

        public void Log(string message, LogLevel level) {
            if (level >= MinimumLevel) {
                var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss .fff").PadRight(27) +
                    $"{level}".PadRight(8) + message;
                Console.WriteLine(line);
            }
        }
    }

}
