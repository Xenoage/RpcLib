using BankShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Server;
using System.Threading.Tasks;

namespace BankClient.Rpc.Stubs {

    /// <summary>
    /// Server-side stub implementation of the <see cref="IBankClientRpc"/> functions.
    /// </summary>
    public class BankClientRpcStub : RpcClientStub, IBankClientRpc {

        public BankClientRpcStub(string clientID) : base(clientID) {
        }

        public Task<int> GetBalance(int accountNumber) =>
            ExecuteOnClient<int>(new RpcCommand("GetBalance", accountNumber));

        public Task<int> AddMoney(int accountNumber, int cents) =>
            ExecuteOnClient<int>(new RpcCommand("AddMoney", accountNumber, cents) {
                RetryStrategy = RpcRetryStrategy.RetryWhenOnline
            });

        public Task ChangeOwnerName(int accountNumber, string ownerName) =>
            ExecuteOnClient(new RpcCommand("ChangeOwnerName", accountNumber, ownerName) {
                RetryStrategy = RpcRetryStrategy.RetryNewestWhenOnline
            });

    }

}
