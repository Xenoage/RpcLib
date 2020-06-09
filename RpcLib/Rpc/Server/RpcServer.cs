using Newtonsoft.Json;
using Shared.Model;
using Shared.Rpc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RpcServer.Rpc {

    /// <summary>
    /// Server-side (real) implementation of the <see cref="IRpcServer"/> functions.
    /// </summary>
    public class RpcServer : IRpcServer {

        public async Task<SampleData> ProcessData(SampleData baseData) {
            return new SampleData {
                Text = baseData.Text + "-ServerWasHere",
                Number = baseData.Number * 2,
                List = baseData.List.Select(it => it + "-ServerWasHere").ToList()
            };
        }

        public async Task SayHello(Greeting greeting) {
            Console.WriteLine("Hello, " + greeting.Name + "!" +
                greeting.MoreData == null ? "" :
                    " Here is a message for you: " + JsonConvert.SerializeObject(greeting.MoreData));
        }
    }

}
