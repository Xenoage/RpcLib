using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        /// <summary>
        /// Creates a new JSON-file-based backlog, using the given directory
        /// for storing the backlog items.
        /// </summary>
        public JsonFileRpcBacklog(DirectoryInfo backlogDir) {
            this.backlogDir = backlogDir;
        }

        public async Task<Queue<RpcCall>> ReadAll(string? targetPeerID) {
            await semaphore.WaitAsync();
            var queue = new Queue<RpcCall>(GetFilesOrdered(targetPeerID).Select(ReadFromFile));
            semaphore.Release();
            return queue;
        }

        public async Task Add(RpcCall call) {
            await semaphore.WaitAsync();
            WriteToFile(call);
            semaphore.Release();
        }

        public async Task RemoveByMethodID(string? targetPeerID, ulong methodID) {
            await semaphore.WaitAsync();
            if (GetFileByMethodID(targetPeerID, methodID) is FileInfo file)
                file.Delete();
            semaphore.Release();
        }

        public async Task RemoveByMethodName(string? targetPeerID, string methodName) {
            await semaphore.WaitAsync();
            foreach (var file in GetFilesByMethodName(targetPeerID, methodName))
                file.Delete();
            semaphore.Release();
        }


        private RpcCall ReadFromFile(FileInfo file) =>
            Serializer.Deserialize<RpcCall>(File.ReadAllBytes(file.FullName));

        private void WriteToFile(RpcCall call) {
            var dir = GetDirectory(call.RemotePeerID);
            string filename = call.Method.ID + "-" + call.Method.Name;
            File.WriteAllBytes(Path.Combine(dir.FullName, filename), Serializer.Serialize(call));
        }

        private IEnumerable<FileInfo> GetFilesOrdered(string? targetPeerID) =>
            GetDirectory(targetPeerID).GetFiles().OrderBy(file => ulong.Parse(file.Name.Split('-')[0]));

        private FileInfo[] GetFilesByMethodName(string? targetPeerID, string methodName) =>
            GetDirectory(targetPeerID).GetFiles("*-" + methodName); // Very inefficient; just for demo purposes

        private FileInfo? GetFileByMethodID(string? targetPeerID, ulong methodID) {
            var files = GetDirectory(targetPeerID).GetFiles(methodID + "-*"); // Very inefficient; just for demo purposes
            return files.Length > 0 ? files[0] : null;
        }

        private DirectoryInfo GetDirectory(string? targetPeerID) {
            var dir = new DirectoryInfo(backlogDir.FullName + "/" + (targetPeerID ?? "Server"));
            if (false == dir.Exists)
                Directory.CreateDirectory(dir.FullName);
            return dir;
        }


        // Directory where the JSON files are stored.
        private DirectoryInfo backlogDir;
        // Semaphore which allows only one thread to enter it at the same time. The others will wait.
        private SemaphoreSlim semaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    }

}
