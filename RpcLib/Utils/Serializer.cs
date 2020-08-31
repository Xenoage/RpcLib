using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// (De)serializes content from/into plaintext JSON or gzip-compressed JSON.
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
            return JsonLib.FromJson<T>(json);
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
            return JsonLib.FromJson<T>(json);
        }

        /// <summary>
        /// Creates an HTTP response with the given content.
        /// The content may be stored as plaintext JSON or in gzip-compressed JSON.
        /// </summary>
        public static async Task<IActionResult> Serialize<T>(T content, bool compress, ControllerBase ctrl) {
            if (compress && content != null)
                return ctrl.File(await Gzip.ZipToBytes(JsonLib.ToJson(content)), "application/gzip");
            else
                return ctrl.Ok(content);
        }

        /// <summary>
        /// Creates an HTTP content with the given content.
        /// The content may be stored as plaintext JSON or in gzip-compressed JSON.
        /// </summary>
        public static async Task<HttpContent?> Serialize<T>(T content, bool compress) {
            var json = content != null ? JsonLib.ToJson(content) : null;
            if (compress && json != null) {
                var ret = new ByteArrayContent(await Gzip.ZipToBytes(json));
                ret.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/gzip");
                return ret;
            }
            else if (json != null) {
                return new StringContent(json, Encoding.UTF8, "application/json");
            }
            else {
                return null;
            }
        }

    }

}
