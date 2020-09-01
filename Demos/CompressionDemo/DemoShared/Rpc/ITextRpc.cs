using RpcLib;
using RpcLib.Model;
using System.Threading.Tasks;

namespace DemoShared.Rpc {

    /// <summary>
    /// Some functions to send text messages from one peer to another.
    /// The different compression strategies are demonstrated:
    /// Default, auto, forced enabled and forced disabled and starting at a given size.
    /// </summary>
    public interface ITextRpc : IRpcFunctions {

        /// <summary>
        /// Returns the given text in capital letters.
        /// Messages starting at a given size (see <see cref="RpcSettings"/>) will be compressed.
        /// </summary>
        [RpcOptions(Compression = RpcCompressionStrategy.Auto)]
        Task<string> CapitalizeText(string text);

        /// <summary>
        /// Like <see cref="CapitalizeText(string)"/>, but compression always on.
        /// </summary>
        [RpcOptions(Compression = RpcCompressionStrategy.Enabled)]
        Task<string> CapitalizeText_Compressed(string text);

        /// <summary>
        /// Like <see cref="CapitalizeText(string)"/>, but compression always off.
        /// </summary>
        [RpcOptions(Compression = RpcCompressionStrategy.Disabled)]
        Task<string> CapitalizeText_Uncompressed(string text);

    }

}
