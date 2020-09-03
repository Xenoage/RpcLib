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
    /// ./RpcBacklog/{clientID|"Server"}/{command ID}-{command name}
    /// </summary>
    public class DemoRpcCommandBacklog : IRpcCommandBacklog {

        public RpcCommand? PeekCommand(string clientID) {
            lock (syncLock) {
                if (GetLatestFile(clientID) is FileInfo file)
                    return RpcMain.JsonLib.FromJson<RpcCommand>(File.ReadAllText(file.FullName));
                else
                    return null;
            }
        }

        public void DequeueCommand(string clientID, ulong commandID) {
            lock (syncLock) {
                if (GetLatestFile(clientID) is FileInfo file) {
                    var command = RpcMain.JsonLib.FromJson<RpcCommand>(File.ReadAllText(file.FullName));
                    if (command.ID == commandID)
                        file.Delete();
                }
            }
        }

        public void EnqueueCommand(string clientID, RpcCommand command) {
            lock (syncLock) {
                var dir = GetDirectory(clientID);
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
                var filename = command.ID + "-" + command.MethodName;
                File.WriteAllText(Path.Combine(dir.FullName, filename), RpcMain.JsonLib.ToJson(command));
            }
        }

        private FileInfo? GetLatestFile(string clientID) =>
            GetDirectory(clientID).GetFiles().OrderBy(
                file => ulong.Parse(file.Name.Split('-')[0])).FirstOrDefault();

        private FileInfo[] GetFilesByCommandName(string clientID, string commandName) =>
            GetDirectory(clientID).GetFiles("*-" + commandName);

        private DirectoryInfo GetDirectory(string clientID) {
            var dir = new DirectoryInfo("RpcBacklog/" + (clientID.Length > 0 ? clientID : "Server"));
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }

        private readonly object syncLock = new object();

    }

}
