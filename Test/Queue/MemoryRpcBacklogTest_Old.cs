using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Xenoage.RpcLib.Queue {

    /// <summary>
    /// Tests for <see cref="MemoryRpcBacklog_Old"/>, using the <see cref="IRpcBacklogTest_Old"/>.
    /// </summary>
    [TestClass]
    public class MemoryRpcBacklogTest_Old : IRpcBacklogTest_Old {

        protected override IRpcBacklog_Old CreateInstance() =>
            new MemoryRpcBacklog_Old();
    }

}
