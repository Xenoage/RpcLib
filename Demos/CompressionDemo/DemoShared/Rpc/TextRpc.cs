using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Implementation of the <see cref="ITextRpc"/> interface,
    /// both used on the server and the client side.
    /// The implementation is independent of the compression method.
    /// </summary>
    public class TextRpc : RpcFunctions, ITextRpc {

        public Task<string> CapitalizeText(string text) =>
            Task.FromResult(text.ToUpper()); 

        public Task<string> CapitalizeText_Compressed(string text) =>
            CapitalizeText(text);

        public Task<string> CapitalizeText_Uncompressed(string text) =>
            CapitalizeText(text);

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public override Task<string?>? Execute(RpcCommand command) => command.MethodName switch {
            "CapitalizeText" => CapitalizeText(command.GetParam<string>(0)).ToJson(),
            "CapitalizeText_Compressed" => CapitalizeText_Compressed(command.GetParam<string>(0)).ToJson(),
            "CapitalizeText_Uncompressed" => CapitalizeText_Compressed(command.GetParam<string>(0)).ToJson(),
            _ => null
        };

    }
}
