using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.IO;
using System.Linq;

namespace BankShared.Rpc {

    /// <summary>
    /// Very simple implementation of a <see cref="IRpcCommandBacklog"/>,
    /// using JSON files for persisting the queue.
    /// 
    /// This is very inefficient and is just used for demo purposes.
    /// In your real world project, use a database instead.
    /// 
    /// File naming:
    /// ./RpcBacklog/{clientID|"Server"}/{unix time in ms}{counter}-{command name}
    /// </summary>
    public class DemoRpcCommandBacklog : IRpcCommandBacklog {

        public RpcCommand? DequeueCommand(string? clientID) {
            lock (syncLock) {
                if (GetLatestFile(clientID) is FileInfo file) {
                    var ret = JsonLib.FromJson<RpcCommand>(File.ReadAllText(file.FullName));
                    file.Delete();
                    return ret;
                }
                else {
                    return null;
                }
            }
        }

        public void EnqueueCommand(string? clientID, RpcCommand command) {
            lock (syncLock) {
                var dir = GetOrCreateDirectory(clientID);
                // Apply strategy
                var strategy = command.RetryStrategy;
                if (strategy == null || strategy == RpcRetryStrategy.None) {
                    // No retry strategy chosen. This method should not have been called at all. Do nothing.
                    return;
                }
                else if (strategy == RpcRetryStrategy.RetryWhenOnline) {
                    // No preparation needed; just enqueue this command
                }
                else if (strategy == RpcRetryStrategy.RetryNewestWhenOnline) {
                    // Remove all preceding commands of this type
                    foreach (var file in GetFilesByCommandName(clientID, command.MethodName))
                        file.Delete();
                }
                var filename = CoreUtils.TimeNow() + counter + "-" + command.MethodName;
                File.WriteAllText(Path.Combine(dir.FullName, filename), JsonLib.ToJson(command));
                // Increment counter
                counter++;
                if (counter > 9999)
                    counter = 1000;
            }
        }

        private FileInfo? GetLatestFile(string? clientID) =>
            GetDirectory(clientID).GetFiles().OrderBy(
                file => ulong.Parse(file.Name.Split('-')[0])).FirstOrDefault();

        private FileInfo[] GetFilesByCommandName(string? clientID, string commandName) =>
            GetDirectory(clientID).GetFiles("*-" + commandName);

        private DirectoryInfo GetOrCreateDirectory(string? clientID) {
            var dir = GetDirectory(clientID);
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }

        private DirectoryInfo GetDirectory(string? clientID) =>
            new DirectoryInfo("RpcBacklog/" + (clientID ?? "Server"));

        private readonly object syncLock = new object();
        private long counter = 1000; // Between 1000 and 9999 to create unique IDs within the same millisecond

    }

}
