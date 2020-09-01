using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace DemoServer.Rpc.Stubs {

    /// <summary>
    /// Demo server-side (stub) implementation of the <see cref="ITextRpc"/> functions,
    /// one instance for each client.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class TextRpcStub : RpcClientStub, ITextRpc {

        public TextRpcStub(string clientID) : base(clientID) {
        }

        public Task<string> CapitalizeText(string text) =>
            ExecuteOnClient<string>("CapitalizeText", text);

        public Task<string> CapitalizeText_Compressed(string text) =>
            ExecuteOnClient<string>("CapitalizeText_Compressed", text);

        public Task<string> CapitalizeText_Uncompressed(string text) =>
            ExecuteOnClient<string>("CapitalizeText_Uncompressed", text);

    }

}
