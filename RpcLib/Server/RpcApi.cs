using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpcLib.Model;
using RpcLib.Rpc.Utils;

namespace RpcLib.Server {

    /// <summary>
    /// Web API endpoints for the <see cref="RpcServerEngine"/>.
    /// The concrete <see cref="IRpcAuth"/> implementation is injected, so it must be
    /// registered during the ASP.NET Core startup as a service.
    /// </summary>
    [ApiController]
    [Route("rpc")]
    public class RpcApi : ControllerBase {

        private IRpcAuth auth;

        public RpcApi(IRpcAuth auth) {
            this.auth = auth;
        }

        /// <summary>
        /// Call this method by the client to execute the given RPC command
        /// (serialized as JSON in the body) on the server. The result is returned.
        /// See <see cref="RpcServerEngine.OnClientPush"/> for more details.
        /// </summary>
        [HttpPost("push")]
        public async Task<IActionResult> Push() {
            // Identify calling client. If now allowed, return RPC failure.
            string? clientID = auth.GetClientID(Request);
            if (clientID == null)
                return Unauthorized();
            // Read request body (if any). We do not use a [FromBody] parameter, because
            // we want to explicitly use our JsonLib for deserializing (and not overwrite the
            // user's selected default ASP.NET Core JSON serializer)
            using (var reader = new StreamReader(Request.Body)) {
                var body = await reader.ReadToEndAsync();
                if (body.Length > 0) {
                    // Run command and return the result
                    var command = JsonLib.FromJson<RpcCommand>(body);
                    return Ok(await RpcServerEngine.OnClientPush(clientID, command));
                }
            }
            // Command missing
            return BadRequest();
        }

        /// <summary>
        /// Call this method by the client continuously to report the last result (optional,
        /// serialized as JSON in the body) and query the next command.
        /// See <see cref="RpcServerEngine.OnClientPull"/> for more details.
        /// </summary>
        [HttpPost("pull")]
        public async Task<IActionResult> Pull() {
            // Identify calling client. If now allowed, return RPC failure.
            string? clientID = auth.GetClientID(Request);
            if (clientID == null)
                return Unauthorized();
            // Read request body (if any). We do not use a [FromBody] parameter, because
            // we want to explicitly use our JsonLib for deserializing (and not overwrite the
            // user's selected default ASP.NET Core JSON serializer)
            RpcCommandResult? lastResult = null;
            using (var reader = new StreamReader(Request.Body)) {
                var body = await reader.ReadToEndAsync();
                if (body.Length > 0)
                    lastResult = JsonLib.FromJson<RpcCommandResult>(body);
            }
            // Report result and query next method
            return Ok(await RpcServerEngine.OnClientPull(clientID, lastResult));
        }

    }

}
