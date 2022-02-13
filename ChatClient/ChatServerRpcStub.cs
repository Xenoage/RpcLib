using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Logging;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class ChatServerRpcStub : RpcMethodsStub, IChatServerRpc {

        public ChatServerRpcStub(RpcClient localClient) : base(localClient) {
            localClient.Reconnected += () => {
                Log.Debug("Reconnected. Register events.");
                RegisterEventsOnRemotePeer();
            };
        }

        public Task<bool> SendPrivateMessage(string message, string username) =>
            ExecuteOnRemotePeer<bool>("SendPrivateMessage", message, username);

        public Task SendPublicMessage(string message) =>
            ExecuteOnRemotePeer("SendPublicMessage", message);

        // TODO: Auto generate
        public event Action<ChatMessage> MessageReceived {
            add {
                messageReceived += value;
                RegisterEventsOnRemotePeer();
            }
            remove {
                messageReceived -= value;
                RegisterEventsOnRemotePeer();
            }
        }
        private event Action<ChatMessage> messageReceived = delegate { };

        // TODO: Auto generate
        protected override IEnumerable<string> GetRegisteredEventNames() {
            // Collect events which are used at least once
            var eventNames = new List<string>();
            if (messageReceived.GetInvocationList().Length > 0)
                eventNames.Add(nameof(MessageReceived));
            return eventNames.ToArray();
        }

        // TODO: Auto generate
        public override void ExecuteEventOnLocalPeer(RpcMethod evt) {
            switch (evt.Name) {
                case "!.MessageReceived": messageReceived(evt.GetParam<ChatMessage>(0)); break;
            }
        }

    }

}
