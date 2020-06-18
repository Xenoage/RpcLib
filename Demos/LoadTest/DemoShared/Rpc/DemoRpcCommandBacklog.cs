using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DemoShared.Rpc {

    /// <summary>
    /// Very simple implementation of a <see cref="IRpcCommandBacklog"/>,
    /// using JSON files for persisting the queue.
    /// 
    /// This is very inefficient and is just used for demo purposes.
    /// In your real world project, use a database instead.
    /// 
    /// File naming:
    /// ./RpcBacklog/{clientID|"Server"}/{Unix millisecond}{counter}-{command name}
    /// </summary>
    public class DemoRpcCommandBacklog : IRpcCommandBacklog {

        public RpcCommand? GetCommand(string? clientID) {
            lock (syncLock) {
                if (GetLatestFile(clientID) is FileInfo file)
                    return JsonLib.FromJson<RpcCommand>(File.ReadAllText(file.FullName));
                else
                    return null;
            }
        }

        public void FinishCommand(string? clientID) {
            lock (syncLock) {
                if (GetLatestFile(clientID) is FileInfo file)
                    file.Delete();
            }
        }

        public void Enqueue(string? clientID, RpcCommand command, RpcRetryStrategy retryStrategy) {
            lock (syncLock) {
                var dir = GetOrCreateDirectory(clientID);
                // Apply strategy
                if (retryStrategy == RpcRetryStrategy.None) {
                    // No retry strategy chosen. This method should not have been called at all. Do nothing.
                    return;
                }
                else if (retryStrategy == RpcRetryStrategy.RetryWhenOnline) {
                    // No preparation needed; just enqueue this command
                }
                else if (retryStrategy == RpcRetryStrategy.RetryNewestWhenOnline) {
                    // Remove all preceding commands of this type
                    foreach (var file in GetFilesByCommandName(clientID, command.MethodName))
                        file.Delete();
                }
                var filename = "" + CoreUtils.TimeNow() + counter + "-" + command.MethodName;
                File.WriteAllText(Path.Combine(dir.FullName, filename), JsonLib.ToJson(command));
                // Increment counter
                counter++;
                if (counter > 9999)
                    counter = 1000;
            }
        }

        private FileInfo? GetLatestFile(string? clientID) =>
            GetDirectory(clientID).GetFiles().OrderBy(
                file => long.Parse(file.Name.Split('-')[0])).FirstOrDefault();

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
