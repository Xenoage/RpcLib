using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    /// {backlogDir}/{clientID|"Server"}/{ID}-{method name}
    /// </summary>
    public class JsonFileRpcBacklog : IRpcBacklog {

        public bool IsPersistent => true;

        /// <summary>
        /// Creates a new JSON-file-based backlog, using the given directory
        /// for storing the backlog items.
        /// </summary>
        public JsonFileRpcBacklog(DirectoryInfo backlogDir) {
            this.backlogDir = backlogDir;
        }

        public async Task<RpcCall?> Peek(string? targetPeerID) =>
            await PeekOrDequeue(targetPeerID, dequeue: false);

        public async Task<RpcCall?> Dequeue(string? targetPeerID) =>
            await PeekOrDequeue(targetPeerID, dequeue: true);

        private async Task<RpcCall?> PeekOrDequeue(string? targetPeerID, bool dequeue) {
            lock (syncLock) {
                if (GetLatestFile(targetPeerID) is FileInfo file) {
                    var result = LoadFromFile(file);
                    if (dequeue)
                        file.Delete();
                    return result;
                } else {
                    return null;
                }
            }
        }

        public async Task Enqueue(RpcCall call) {
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

        public async Task<int> GetCount(string? targetPeerID) =>
            GetDirectory(targetPeerID).GetFiles().Length;


        private RpcCall LoadFromFile(FileInfo file) =>
            Serializer.Deserialize<RpcCall>(File.ReadAllBytes(file.FullName));

        private FileInfo? GetLatestFile(string? targetPeerID) =>
            GetDirectory(targetPeerID).GetFiles().OrderBy( // Very inefficient; just for demo purposes
                file => ulong.Parse(file.Name.Split('-')[0])).FirstOrDefault();

        private FileInfo[] GetFilesByMethodName(string? targetPeerID, string methodName) =>
            GetDirectory(targetPeerID).GetFiles("*-" + methodName); // Very inefficient; just for demo purposes

        private DirectoryInfo GetDirectory(string? targetPeerID) {
            var dir = new DirectoryInfo(backlogDir.FullName + "/" + (targetPeerID ?? "Server"));
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }

        private DirectoryInfo backlogDir;

        private readonly object syncLock = new object();

    }

}
