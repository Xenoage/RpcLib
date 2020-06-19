using BankShared;
using BankShared.Rpc;
using DemoServer.Services;
using Microsoft.Extensions.DependencyInjection;
using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Server-side implementation of the <see cref="IBankServerRpc"/> demo showcase.
    /// </summary>
    public class BankServerRpc : RpcFunctions, IBankServerRpc {

        public async Task<int> GetBalance(int accountNumber) {
            return Bank.GetOrCreateAccount(accountNumber).Cents;
        }

        public async Task<int> AddMoney(int accountNumber, int cents) {
            var account = Bank.GetOrCreateAccount(accountNumber);
            account.Cents += cents;
            Log.WriteToFile($"BankServer-{Context.ClientID}.banklog", $"Add | {accountNumber} | {cents} | {account.Cents}");
            return account.Cents;
        }

        public async Task ChangeOwnerName(int accountNumber, string ownerName) {
            Bank.GetOrCreateAccount(accountNumber).OwnerName = ownerName;
            Log.WriteToFile($"BankServer-{Context.ClientID}.banklog", $"Name | {accountNumber} | {ownerName}");
        }

        /// <summary>
        /// Gets the dependency-injected <see cref="BankService"/>.
        /// </summary>
        private BankService Bank {
            get {
                using (var scope = Context.ServiceScopeFactory!.CreateScope()) {
                    return scope.ServiceProvider.GetService<BankService>();
                }
            }
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
