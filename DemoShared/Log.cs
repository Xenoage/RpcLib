using System;

namespace DemoShared {

    /// <summary>
    /// Simple console logging for the demo projects.
    /// </summary>
    public static class Log {

        public static void Write(string message) {
            Console.WriteLine(DateTime.Now.ToString().PadRight(20) + message);
        }

    }

}
