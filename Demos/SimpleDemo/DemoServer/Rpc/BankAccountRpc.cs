using DemoShared.Rpc;
using RpcLib;
using RpcLib.Model;
using RpcLib.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DemoServer.Rpc {

    /// <summary>
    /// Server-side implementation of the <see cref="IBankAccountRpc"/> demo showcase.
    /// </summary>
    public class BankAccountRpc : RpcFunctions, IBankAccountRpc {

        public async Task<int> GetBalance(int accountNumber) {
            return GetOrCreateAccount(accountNumber).cents;
        }

        public async Task<int> AddMoney(int accountNumber, int cents) {
            var account = GetOrCreateAccount(accountNumber);
            account.cents += cents;
            return account.cents;
        }

        public async Task ChangeOwnerName(int accountNumber, string ownerName) {
            GetOrCreateAccount(accountNumber).ownerName = ownerName;
        }

        // Mapping of RpcCommand to real method calls (boilerplate code; we could auto-generate this method later)
        public override Task<string?>? Execute(RpcCommand command) => command.MethodName switch {
            "GetBalance" => GetBalance(command.GetParam<int>(0)).ToJson(),
            "AddMoney" => AddMoney(command.GetParam<int>(0), command.GetParam<int>(1)).ToJson(),
            "ChangeOwnerName" => ChangeOwnerName(command.GetParam<int>(0), command.GetParam<string>(1)).ToJson(),
            _ => null
        };


        private IDictionary<int, BankAccount> accounts = new Dictionary<int, BankAccount>();

        private BankAccount GetOrCreateAccount(int accountNumber) {
            if (accounts.TryGetValue(accountNumber, out var ret))
                return ret;
            ret = new BankAccount { number = accountNumber };
            accounts[accountNumber] = ret;
            return ret;
        }
        
    }

    /// <summary>
    /// Bank account demo class.
    /// </summary>
    public class BankAccount {
        public int number;
        public string ownerName = "?";
        public int cents = 0;
    }

}
