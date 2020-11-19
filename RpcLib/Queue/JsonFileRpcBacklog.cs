using System.IO;
using System.Linq;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Serialization;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Very simple implementation of a <see cref="IRpcBacklog"/>,
    /// using JSON files for persisting the queue.
    /// 
    /// This is very inefficient and is just used for demo purposes.
    /// In your real world project, use for example a local database instead!
    /// 
    /// File naming:
    /// ./RpcBacklog/{clientID|"Server"}/{ID}-{method name}
    /// </summary>
    public class JsonFileRpcBacklog : IRpcBacklog {

        public bool TryPeek(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: false, out result);

        public bool TryDequeue(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: true, out result);

        private bool TryPeekOrDequeue(string? targetPeerID, bool dequeue, out RpcCall result) {
            lock (syncLock) {
                if (GetLatestFile(targetPeerID) is FileInfo file) {
                    result = Serializer.Deserialize<RpcCall>(File.ReadAllBytes(file.FullName));
                    if (dequeue)
                        file.Delete();
                    return true;
                } else {
                    result = default!;
                    return false;
                }
            }
        }

        public void Enqueue(string? targetPeerID, RpcCall call) {
            lock (syncLock) {
                var dir = GetDirectory(targetPeerID);
                // Apply strategy
                var strategy = call.RetryStrategy;
                if (strategy == null || strategy == RpcRetryStrategy.None) {
                    // No retry strategy chosen. This method should not have been called at all. Do nothing.
                    return;
                } else if (strategy == RpcRetryStrategy.Retry) {
                    // No preparation needed; just enqueue this call
                } else if (strategy == RpcRetryStrategy.RetryLatest) {
                    // Remove all preceding method calls with this name
                    foreach (var file in GetFilesByMethodName(targetPeerID, call.Method.Name))
                        file.Delete();
                }
                var filename = call.Method.ID + "-" + call.Method.Name;
                File.WriteAllBytes(Path.Combine(dir.FullName, filename), Serializer.Serialize(call));
            }
        }

        private FileInfo? GetLatestFile(string? targetPeerID) =>
            GetDirectory(targetPeerID).GetFiles().OrderBy( // Very inefficient; just for demo purposes
                file => ulong.Parse(file.Name.Split('-')[0])).FirstOrDefault();

        private FileInfo[] GetFilesByMethodName(string? targetPeerID, string methodName) =>
            GetDirectory(targetPeerID).GetFiles("*-" + methodName); // Very inefficient; just for demo purposes

        private DirectoryInfo GetDirectory(string? targetPeerID) {
            var dir = new DirectoryInfo("RpcBacklog/" + (targetPeerID ?? "Server"));
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }

        private readonly object syncLock = new object();

    }

}
