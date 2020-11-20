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

        public bool IsPersistent => true;

        public bool TryPeek(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: false, out result);

        public bool TryDequeue(string? targetPeerID, out RpcCall result) =>
            TryPeekOrDequeue(targetPeerID, dequeue: true, out result);

        private bool TryPeekOrDequeue(string? targetPeerID, bool dequeue, out RpcCall result) {
            lock (syncLock) {
                if (GetLatestFile(targetPeerID) is FileInfo file) {
                    result = LoadFromFile(file);
                    if (dequeue)
                        file.Delete();
                    return true;
                } else {
                    result = default!;
                    return false;
                }
            }
        }

        public void Enqueue(RpcCall call) {
            lock (syncLock) {
                var dir = GetDirectory(call.TargetPeerID);
                // Apply strategy
                var strategy = call.RetryStrategy;
                if (strategy == null || strategy == RpcRetryStrategy.None) {
                    // No retry strategy chosen. This method should not have been called at all. Do nothing.
                    return;
                } else if (strategy == RpcRetryStrategy.Retry) {
                    // No preparation needed; just enqueue this call
                } else if (strategy == RpcRetryStrategy.RetryLatest) {
                    // Remove all preceding method calls with this name, if still in enqueued state
                    foreach (var file in GetFilesByMethodName(call.TargetPeerID, call.Method.Name)) {
                        if (LoadFromFile(file).State == RpcCallState.Enqueued) // Very inefficient; just for demo purposes
                            file.Delete();
                    }
                }
                var filename = call.Method.ID + "-" + call.Method.Name;
                File.WriteAllBytes(Path.Combine(dir.FullName, filename), Serializer.Serialize(call));
            }
        }

        public void Clear() {
            lock (syncLock) {
                Directory.Delete(baseDir, recursive: true);
            }
        }

        private RpcCall LoadFromFile(FileInfo file) =>
            Serializer.Deserialize<RpcCall>(File.ReadAllBytes(file.FullName));

        private FileInfo? GetLatestFile(string? targetPeerID) =>
            GetDirectory(targetPeerID).GetFiles().OrderBy( // Very inefficient; just for demo purposes
                file => ulong.Parse(file.Name.Split('-')[0])).FirstOrDefault();

        private FileInfo[] GetFilesByMethodName(string? targetPeerID, string methodName) =>
            GetDirectory(targetPeerID).GetFiles("*-" + methodName); // Very inefficient; just for demo purposes

        private DirectoryInfo GetDirectory(string? targetPeerID) {
            var dir = new DirectoryInfo(baseDir + "/" + (targetPeerID ?? "Server"));
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }

        private const string baseDir = "RpcBacklog";

        private readonly object syncLock = new object();

    }

}
