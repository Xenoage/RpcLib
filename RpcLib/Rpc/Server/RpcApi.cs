using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RpcLib.Model;
using Shared.Rpc;

namespace RpcServer.Rpc {

    /// <summary>
    /// Web API endpoints for the <see cref="RpcServerEngine"/>
    /// </summary>
    [ApiController]
    [Route("rpc")]
    public class RpcApi : ControllerBase {

        private const int longPollingSeconds = 90;
        private const int queryMilliseconds = 500;

        RpcServerEngine engine; // TODO

        /// <summary>
        /// This is the communication channel "from the server to the client".
        /// 
        /// Returns the current <see cref="RpcCommand"/> in the queue of the calling client,
        /// by "long polling". Because the server can not call the client directly (firewall...),
        /// instead the client continuously calls this method and waits for new data.
        /// Because the server only responds when there is data available or with null when
        /// a long timeout is hit (e.g. 90 seconds), the traffic in the network is highly limited. 
        /// 
        /// To ensure that a command is received by the client, it contains a unique <see cref="RpcCommand.ID"/>.
        /// The client has to acknowledge that it has received and executed it by
        /// sending this ID and the result (or exception) back as a <see cref="RpcCommandResult"/>
        /// in the body of this API endpoint. Otherwise, the same command would be sent again by the server.
        /// The client can also use this ID to ensure that the command is only evaluated once,
        /// even when it was received two times for any reason.
        /// </summary>
        [HttpPost("pull")]
        public async Task<RpcCommand?> Pull([FromBody] RpcCommandResult lastCommandResult) {
            string clientID = Request
            // When a result is received, process it
            if (lastCommandResult != null)
                engine.ReportClientResult(clientID, lastCommandResult);
            // Wait for next command
            long endTime = Utils.TimeNow() + longPollingSeconds * 1000;
            while (Utils.TimeNow() < endTime) {
                RpcCommand? next = engine.GetClientCommand(clientID);
                if (next != null) {
                    next.State = RpcCommandState.Sent;
                    return next;
                }
                await Task.Delay(queryMilliseconds);
            }
            // No item during long polling time. Return null.
            return null;
        }

    }

}
