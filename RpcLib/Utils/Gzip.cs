using Microsoft.AspNetCore.Mvc.Formatters;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// gzip compression utilities.
    /// </summary>
    public static class Gzip {

        /// <summary>
        /// Compresses the given string into a gzip-compressed byte array .
        /// </summary>
        public static async Task<byte[]> ZipToBytes(string str) {
            using (var zipStream = await ZipToStream(str))
            return zipStream.ToArray();
        }

        /// <summary>
        /// Compresses the given string into a gzip-compressed stream.
        /// </summary>
        public static async Task<MemoryStream> ZipToStream(string str) {
            using (var streamIn = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            using (var streamOut = new MemoryStream()) {
                using (var zipStream = new GZipStream(streamOut, CompressionMode.Compress))
                    streamIn.CopyTo(zipStream);
                return streamOut;
            }
        }

        /// <summary>
        /// Uncompressed the given gzip-compressed string.
        /// </summary>
        public static async Task<string> Unzip(byte[] bytes) {
            using (var stream = new MemoryStream(bytes))
            return await Unzip(stream);
        }

        /// <summary>
        /// Uncompresses the given gzip-compressed string stream.
        /// </summary>
        public static async Task<string> Unzip(Stream stream) {
            using (var streamZip = new GZipStream(stream, CompressionMode.Decompress))
                return await new StreamReader(streamZip).ReadToEndAsync();
        }

    }

}
