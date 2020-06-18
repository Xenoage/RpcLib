using System;
using System.IO;

namespace DemoShared {

    /// <summary>
    /// Simple console logging for the demo projects.
    /// </summary>
    public static class Log {

        public static void Write(string message) {
            Console.WriteLine(DateTime.Now.ToString().PadRight(20) + message);
        }

        public static void WriteToFile(string filename, string line) {
            lock (lockObj) {
                File.AppendAllText(filename, line + "\n");
            }
        }

        private static object lockObj = new object();

    }

}
