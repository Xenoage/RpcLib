using BankShared;
using BankShared.Rpc;
using RpcLib.Model;
using RpcLib.Peers.Client;
using System;
using System.Threading.Tasks;

namespace BankClient.Rpc.Stubs {

    /// <summary>
    /// Client-side stub implementation of the <see cref="IBankServerRpc"/> functions.
    /// </summary>
    public class BankServerRpcStub : RpcServerStub, IBankServerRpc {

        public Task<int> GetBalance(int accountNumber) =>
            ExecuteOnServer<int>("GetBalance", accountNumber);

        public Task<int> AddMoney(int accountNumber, int cents) =>
            ExecuteOnServer<int>("AddMoney", accountNumber, cents);

        public Task ChangeOwnerName(int accountNumber, string ownerName) =>
            ExecuteOnServer("ChangeOwnerName", accountNumber, ownerName);

    }

}
