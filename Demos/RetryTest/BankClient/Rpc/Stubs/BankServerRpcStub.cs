using BankShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
using System.Threading.Tasks;

namespace BankClient.Rpc.Stubs {

    /// <summary>
    /// Client-side stub implementation of the <see cref="IBankServerRpc"/> functions.
    /// </summary>
    public class BankServerRpcStub : RpcServerStub, IBankServerRpc {

        public Task<int> GetBalance(int accountNumber) =>
            ExecuteOnServer<int>(new RpcCommand("GetBalance", accountNumber));

        public Task<int> AddMoney(int accountNumber, int cents) =>
            ExecuteOnServer<int>(new RpcCommand("AddMoney", accountNumber, cents) { RetryStrategy = RpcRetryStrategy.RetryWhenOnline });

        public Task ChangeOwnerName(int accountNumber, string ownerName) =>
            ExecuteOnServer(new RpcCommand("ChangeOwnerName", accountNumber, ownerName) { RetryStrategy = RpcRetryStrategy.RetryNewestWhenOnline });

    }

}
