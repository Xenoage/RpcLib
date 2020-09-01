using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// gzip compression utilities.
    /// </summary>
    public static class Gzip {

        /// <summary>
        /// Compresses the given byte array into a gzip-compressed byte array .
        /// </summary>
        public static async Task<byte[]> ZipToBytes(byte[] data) {
            using (var zipStream = await ZipToStream(data))
            return zipStream.ToArray();
        }

        /// <summary>
        /// Compresses the given byte array into a gzip-compressed stream.
        /// </summary>
        public static async Task<MemoryStream> ZipToStream(byte[] data) {
            using (var streamIn = new MemoryStream(data))
            using (var streamOut = new MemoryStream()) {
                using (var zipStream = new GZipStream(streamOut, CompressionMode.Compress))
                    streamIn.CopyTo(zipStream);
                return streamOut;
            }
        }

        /// <summary>
        /// Uncompresses the given gzip-compressed UTF-8 string.
        /// </summary>
        public static async Task<string> Unzip(byte[] bytes) {
            using (var stream = new MemoryStream(bytes))
            return await Unzip(stream);
        }

        /// <summary>
        /// Uncompresses the given gzip-compressed UTF-8 string stream.
        /// </summary>
        public static async Task<string> Unzip(Stream stream) {
            using (var streamZip = new GZipStream(stream, CompressionMode.Decompress))
                return await new StreamReader(streamZip).ReadToEndAsync();
        }

    }

}
