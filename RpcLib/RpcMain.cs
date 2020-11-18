using System;
using Xenoage.RpcLib.Logging;

namespace Xenoage.RpcLib {

    public class RpcMain {

        /// <summary>
        /// Logs the given message with the given severity to
        /// the logger defined in <see cref="DefaultSettings"/>.
        /// </summary>
        public static void Log(string message, LogLevel level) =>
            Console.WriteLine(message); // GOON DefaultSettings.Logger.Log(message, level);

    }

}
