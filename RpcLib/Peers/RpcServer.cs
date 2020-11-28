using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Xenoage.RpcLib.Peers {

    public class RpcServer : RpcPeer {

        public RpcServer() : base(null, null) {

        }

        public override Task Start() {
            throw new NotImplementedException();
        }

        public override Task Stop() {
            throw new NotImplementedException();
        }

        // GOON
        protected override RpcChannel GetChannel(string? remotePeerID) {
            throw new System.NotImplementedException();
        }
    }

}
