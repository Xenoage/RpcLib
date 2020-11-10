using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpcLib.Auth;
using RpcLib.Logging;
using RpcLib.Model;
using RpcLib.Utils;

namespace RpcLib.Peers.Server {

    /// <summary>
    /// Web API endpoints for the <see cref="RpcServerEngine"/>.
    /// The concrete <see cref="IRpcAuth"/> implementation is injected, so it must be
    /// registered during the ASP.NET Core startup as a service.
    /// </summary>
    [Obsolete("Use SignalR based mechanism instead!")]
    [ApiController]
    [Route("rpc")]
    public class RpcApi : ControllerBase {

        private IRpcAuth auth;
        private RpcCommandRunner runner;

        public RpcApi(IRpcAuth auth, RpcCommandRunner runner) {
            this.auth = auth;
            this.runner = runner;
        }

        /// <summary>
        /// Call this method by the client to execute the given RPC command
        /// (serialized as JSON in the body) on the server. The result is returned.
        /// See <see cref="RpcServerEngine.OnClientPush"/> for more details.
        /// </summary>
        [HttpPost("push")]
        public async Task<IActionResult> Push() {
            // Identify calling client. If now allowed, return RPC failure.
            var authResult = auth.Authenticate(Request);
            if (false == authResult.Success) {
                RpcMain.Log($"Unauthorized push received from client {authResult.ClientID}", LogLevel.Debug);
                return Unauthorized();
            }
            string clientID = authResult.ClientID ?? "?";
            // Read request body (if any). We do not use a [FromBody] parameter, because
            // we support two content types (gzipped and plaintext JSON) and we
            // want to explicitly use our JsonLib for deserializing (and not overwrite the
            // user's selected default ASP.NET Core JSON serializer)
            try {
                var command = await Serializer.Deserialize<RpcCommand>(Request);
                var result = await RpcServerEngine.Instance.OnClientPush(clientID, command, runner);
                return await Serializer.Serialize(result, result.Compression, this);
            }
            catch (Exception ex) {
                // Missing or bad command
                RpcMain.Log($"Push from client {clientID}: Corrupt message: {ex.Message}" , LogLevel.Debug);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Call this method by the client continuously to report the last result (optional,
        /// serialized as JSON in the body) and query the next command.
        /// See <see cref="RpcServerEngine.OnClientPull"/> for more details.
        /// </summary>
        [HttpPost("pull")]
        public async Task<IActionResult> Pull() {
            // Identify calling client. If now allowed, return RPC failure.
            var authResult = auth.Authenticate(Request);
            if (false == authResult.Success) {
                RpcMain.Log($"Unauthorized pull received from client {authResult.ClientID}", LogLevel.Debug);
                return Unauthorized();
            }
            string clientID = authResult.ClientID ?? "?";
            // Read request body (if any). We do not use a [FromBody] parameter, because
            // we support two content types (gzipped and plaintext JSON) and we
            // want to explicitly use our JsonLib for deserializing (and not overwrite the
            // user's selected default ASP.NET Core JSON serializer)
            var lastResult = await Serializer.Deserialize<RpcCommandResult?>(Request);
            // Report result and query next method
            var result = await RpcServerEngine.Instance.OnClientPull(clientID, lastResult);
            return await Serializer.Serialize(result, result?.Compression, this);
        }

    }

}
