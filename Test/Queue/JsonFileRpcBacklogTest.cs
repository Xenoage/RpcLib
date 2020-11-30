using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for <see cref="JsonFileRpcBacklog"/>, using the <see cref="IRpcBacklogTest"/>.
    /// </summary>
    [TestClass]
    public class JsonFileRpcBacklogTest : IRpcBacklogTest {

        private const string backlogDir = "RpcBacklog";

        protected override IRpcBacklog CreateInstance() =>
            new JsonFileRpcBacklog(new DirectoryInfo(backlogDir));

        [TestInitialize]
        public void InitDir() {
            try {
                Directory.Delete(backlogDir, recursive: true);
            } catch {
            }
        }

    }

}
