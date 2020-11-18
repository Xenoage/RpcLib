namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// Concise logging calls, using the currently set <see cref="ILogger"/>.
    /// </summary>
    public class Log {

        /// <summary>
        /// By default, the included <see cref="ConsoleLogger"/> is used.
        /// </summary>
        public static ILogger Instance { get; set; } = new ConsoleLogger(LogLevel.Info);

        public static void Error(string message) => Instance.Log(message, LogLevel.Error);
        public static void Warn(string message) => Instance.Log(message, LogLevel.Warn);
        public static void Info(string message) => Instance.Log(message, LogLevel.Info);
        public static void Debug(string message) => Instance.Log(message, LogLevel.Debug);
        public static void Trace(string message) => Instance.Log(message, LogLevel.Trace);

    }

}
