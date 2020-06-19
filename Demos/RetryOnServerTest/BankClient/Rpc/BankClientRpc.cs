using BankShared;
using BankShared.Rpc;
using DemoServer.Services;
using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Client-side implementation of the <see cref="IBankClientRpc"/> demo showcase.
    /// </summary>
    public class BankClientRpc : RpcFunctions, IBankClientRpc {

        private BankService bank = new BankService();

        public async Task<int> GetBalance(int accountNumber) {
            return bank.GetOrCreateAccount(accountNumber).Cents;
        }

        public async Task<int> AddMoney(int accountNumber, int cents) {
            var account = bank.GetOrCreateAccount(accountNumber);
            account.Cents += cents;
            Log.WriteToFile($"BankServer-{Context.ClientID}.banklog", $"Add | {accountNumber} | {cents} | {account.Cents}");
            return account.Cents;
        }

        public async Task ChangeOwnerName(int accountNumber, string ownerName) {
            bank.GetOrCreateAccount(accountNumber).OwnerName = ownerName;
            Log.WriteToFile($"BankServer-{Context.ClientID}.banklog", $"Name | {accountNumber} | {ownerName}");
        }

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public override Task<string?>? Execute(RpcCommand command) => command.MethodName switch {
            "GetBalance" => GetBalance(command.GetParam<int>(0)).ToJson(),
            "AddMoney" => AddMoney(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
            "ChangeOwnerName" => ChangeOwnerName(command.GetParam<int>(0), command.GetParam<string>(1)).ToJson(),
            _ => null
        };
        
    }

    

}
