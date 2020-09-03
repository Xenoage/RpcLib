using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using RpcLib.Model;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// (De)serializes content from/into plaintext JSON or gzip-compressed JSON.
    /// The compression strategy is dependent on the given <see cref="RpcCompressionStrategy"/>,
    /// or, if not set, the default <see cref="RpcSettings.Compression"/>.
    /// </summary>
    public static class Serializer {
        
        /// <summary>
        /// Parses the given HTTP content.
        /// Compressed content is automatically recognized.
        /// </summary>
        public static async Task<T> Deserialize<T>(HttpContent content) {
            if (content.Headers.ContentLength == 0)
                return default!;
            var mediaType = content.Headers.ContentType.MediaType;
            string json;
            if (mediaType == "application/json")
                json = await content.ReadAsStringAsync();
            else if (mediaType == "application/gzip")
                json = await Gzip.Unzip(await content.ReadAsStreamAsync());
            else
                throw new Exception("Unexpected content type: " + mediaType);
            return RpcMain.JsonLib.FromJson<T>(json);
        }

        /// <summary>
        /// Parses the given HTTP request.
        /// Compressed content is automatically recognized.
        /// </summary>
        public static async Task<T> Deserialize<T>(HttpRequest request) {
            if (request.ContentLength == 0)
                return default!;
            string mediaType = request.ContentType.Split(";")[0];
            string json;
            if (mediaType == "application/json") {
                using (var reader = new StreamReader(request.Body))
                    json = await reader.ReadToEndAsync();
            }
            else if (mediaType == "application/gzip") {
                json = await Gzip.Unzip(request.Body);
            }
            else {
                throw new Exception("Unexpected content type: " + mediaType);
            }
            return RpcMain.JsonLib.FromJson<T>(json);
        }

        /// <summary>
        /// Creates an HTTP response with the given content.
        /// The content may be stored as plaintext JSON or in gzip-compressed JSON.
        /// </summary>
        public static async Task<IActionResult> Serialize<T>(T content,
                RpcCompressionStrategy? compression, ControllerBase ctrl) {
            if (content == null)
                return ctrl.Ok();
            var json = RpcMain.JsonLib.ToJson(content);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            if (IsCompressionRequired(compression, jsonBytes.Length))
                return ctrl.File(await Gzip.ZipToBytes(jsonBytes), "application/gzip");
            else
                return ctrl.File(jsonBytes, "application/json");
        }

        /// <summary>
        /// Creates an HTTP content with the given content.
        /// The content may be stored as plaintext JSON or in gzip-compressed JSON.
        /// </summary>
        public static async Task<HttpContent?> Serialize<T>(T content,
                RpcCompressionStrategy? compression) {
            if (content == null)
                return null;
            var json = RpcMain.JsonLib.ToJson(content);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            if (IsCompressionRequired(compression, jsonBytes.Length)) {
                var ret = new ByteArrayContent(await Gzip.ZipToBytes(jsonBytes));
                ret.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/gzip");
                return ret;
            }
            else {
                var ret = new ByteArrayContent(jsonBytes);
                ret.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                return ret;
            }
        }

        /// <summary>
        /// Returns true, iff message compression is enabled for the given compression
        /// strategy and message size in bytes.
        /// </summary>
        private static bool IsCompressionRequired(RpcCompressionStrategy? compression, int sizeBytes) =>
            (compression ?? RpcMain.DefaultSettings.Compression) switch {
                RpcCompressionStrategy.Auto => sizeBytes >= RpcMain.DefaultSettings.CompressionThresholdBytes,
                RpcCompressionStrategy.Enabled => true,
                _ => false
            };

    }

}
