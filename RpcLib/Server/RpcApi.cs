using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpcLib.Model;

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
        /// Call this method by the client continuously to report the last result (optional)
        /// and query the next command.
        /// See <see cref="RpcServerEngine.OnClientPull"/> for more details.
        /// </summary>
        [HttpPost("pull")]
        public async Task<IActionResult> Pull([FromBody] RpcCommandResult lastCommandResult) {
            // Identify calling client. If now allowed, return RPC failure.
            string? clientID = auth.GetClientID(Request);
            if (clientID == null)
                return Unauthorized();
            // Report result and query next method
            return Ok(await RpcServerEngine.OnClientPull(clientID, lastCommandResult));
        }

    }

}
