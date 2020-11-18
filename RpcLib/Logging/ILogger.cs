namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// Logging interface for the RpcLib library.
    /// It is intentially kept very simple, so that any logging framework can easily
    /// be adapted to receive log messages from this library.
    /// </summary>
    public interface ILogger {

        /// <summary>
        /// Logs the given message with the given severity.
        /// </summary>
        public void Log(string message, LogLevel level);    

    }

}
