using DemoShared.Rpc;
using RpcLib.Peers.Client;
using System.Threading.Tasks;

namespace DemoClient.Rpc.Stubs {

    /// <summary>
    /// Demo client-side (stub) implementation of the <see cref="ITextRpc"/> functions.
    /// This file could be auto-generated later from its interface,
    /// since it simply forwards the method calls to the RPC engine.
    /// </summary>
    public class TextRpcStub : RpcServerStub, ITextRpc {

        public Task<string> CapitalizeText(string text) =>
            ExecuteOnServer<string>("CapitalizeText", text);

        public Task<string> CapitalizeText_Compressed(string text) =>
            ExecuteOnServer<string>("CapitalizeText_Compressed", text);

        public Task<string> CapitalizeText_Uncompressed(string text) =>
            ExecuteOnServer<string>("CapitalizeText_Uncompressed", text);
    }

}
