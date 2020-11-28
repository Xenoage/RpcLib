using System.Net;

namespace Xenoage.RpcLib.Utils {

    /// <summary>
    /// Extension methods to <see cref="HttpListenerContext"/>.
    /// </summary>
    public static class HttpListenerContextEx {

        /// <summary>
        /// Gets the IP of the remote endpoint of this HTTP context.
        /// </summary>
        public static string GetIP(this HttpListenerContext context) =>
            context.Request.RemoteEndPoint.Address.ToString();

        /// <summary>
        /// Closes this HTTP connection with the given status code.
        /// </summary>
        public static void Close(this HttpListenerContext context, HttpStatusCode statusCode) {
            context.Response.StatusCode = (int)statusCode;
            context.Response.Close();
        }

    }

}
