using BankShared.Model;
using DemoServer.Rpc;
using System.Collections.Generic;

namespace DemoServer.Services {

    /// <summary>
    /// A bank consists of any number of bank accounts, identified by their account number.
    /// </summary>
    public class BankService {

        private IDictionary<int, BankAccount> accounts = new Dictionary<int, BankAccount>();

        public BankAccount GetOrCreateAccount(int accountNumber) {
            if (accounts.TryGetValue(accountNumber, out var ret))
                return ret;
            ret = new BankAccount { AccountNumber = accountNumber };
            accounts[accountNumber] = ret;
            return ret;
        }

    }

}
