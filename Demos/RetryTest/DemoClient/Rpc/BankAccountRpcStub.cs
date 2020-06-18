using DemoShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
using System.Threading.Tasks;

namespace DemoClient.Rpc {

    /// <summary>
    /// Client-side stub implementation of the <see cref="IBankAccountRpc"/> demo showcase.
    /// </summary>
    public class BankAccountRpcStub : RpcServerStub, IBankAccountRpc {

        public Task<int> GetBalance(int accountNumber) =>
            ExecuteOnServer<int>(new RpcCommand("GetBalance", accountNumber));

        public Task<int> AddMoney(int accountNumber, int cents) =>
            ExecuteOnServer<int>(new RpcCommand("AddMoney", accountNumber, cents));

        public Task ChangeOwnerName(int accountNumber, string ownerName) =>
            ExecuteOnServer(new RpcCommand("ChangeOwnerName", accountNumber, ownerName));

    }

}
