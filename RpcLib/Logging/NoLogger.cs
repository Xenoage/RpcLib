namespace Xenoage.RpcLib.Logging {

    /// <summary>
    /// No logging at all.
    /// </summary>
    public class NoLogger : ILogger {

        public void Log(string message, LogLevel level) {
            // Do nothing
        }

    }

}
