using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;
using Xenoage.RpcLib.Peers;

namespace Chat {

    public class ChatServerRpcStub : RpcMethodsStub, IChatServerRpc {

        public ChatServerRpcStub(RpcClient localClient) : base(localClient) {
        }

        public Task<bool> SendPrivateMessage(string message, string username) =>
            ExecuteOnRemotePeer<bool>("SendPrivateMessage", message, username);

        public Task SendPublicMessage(string message) =>
            ExecuteOnRemotePeer("SendPublicMessage", message);

        // TODO: Auto generate
        public event Action<ChatMessage> MessageReceived {
            add {
                messageReceived += value;
                RegisterEvents();
            }
            remove {
                messageReceived -= value;
                RegisterEvents();
            }
        }
        private event Action<ChatMessage> messageReceived = delegate { };

        // TODO: Auto generate
        private void RegisterEvents() {
            // Collect events which are used at least once
            var eventNames = new List<string>();
            if (messageReceived.GetInvocationList().Length > 0)
                eventNames.Add("!.MessageReceived");
            RegisterEventsOnRemotePeer(eventNames.ToArray());
        }

        // TODO: Auto generate
        protected override void ExecuteEvent(RpcMethod evt) {
            switch (evt.Name) {
                case "!.MessageReceived": messageReceived(evt.GetParam<ChatMessage>(0)); break;
            }
        }

    }

}
