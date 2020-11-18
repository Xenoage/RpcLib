namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// Concise logging calls, using the currently set logger.
    /// </summary>
    public class Log {

        public static ILogger Logger { get; set; } = new ConsoleLogger(LogLevel.Info);

        public static void Error(string message) => Logger.Log(message, LogLevel.Error);
        public static void Warn(string message) => Logger.Log(message, LogLevel.Warn);
        public static void Info(string message) => Logger.Log(message, LogLevel.Info);
        public static void Debug(string message) => Logger.Log(message, LogLevel.Debug);
        public static void Trace(string message) => Logger.Log(message, LogLevel.Trace);

    }

}
