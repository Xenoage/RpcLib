using System;
using System.Collections.Generic;
using System.Text;

namespace Xenoage.RpcLib.Peers {
    public class RpcServer : RpcPeer {
        // GOON
        protected override RpcChannel GetChannel(string? remotePeerID) {
            throw new System.NotImplementedException();
        }
    }
}
