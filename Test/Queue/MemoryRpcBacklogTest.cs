using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for <see cref="MemoryRpcBacklog"/>, using the <see cref="IRpcBacklogTest"/>.
    /// </summary>
    [TestClass]
    public class MemoryRpcBacklogTest : IRpcBacklogTest {

        protected override IRpcBacklog_Old CreateInstance() =>
            new MemoryRpcBacklog();
    }

}
