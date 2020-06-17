using DemoServer.Rpc;
using DemoShared;
using Microsoft.AspNetCore.Mvc;
using RpcLib.Model;
using System.Threading.Tasks;

namespace DemoServer.Api {

    /// <summary>
    /// Demo API endpoints, just for testing.
    /// </summary>
    [ApiController]
    [Route("api/demo")]
    public class DemoApi : ControllerBase {

        /// <summary>
        /// Sends a greeting with the given name to the client with the given ID.
        /// </summary>
        [HttpPost("greet")]
        public async Task Greet([FromQuery] string name, [FromQuery] string clientID) {
            var client = new DemoClientRpcStub(clientID);
            Log.Write($"Sending greeting to name {name} to client {clientID}");
            try {
                await client.SayHelloToClient(new DemoShared.Model.Greeting { Name = name });
                Log.Write("Greeting sent.");
            }
            catch (RpcException ex) {
                Log.Write("Greeting failed: " + ex.Type + ": " + ex.Message);
            }
        }


    }

}
