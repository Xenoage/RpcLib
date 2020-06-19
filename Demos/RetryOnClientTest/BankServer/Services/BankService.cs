using BankShared.Model;
using DemoServer.Rpc;
using System.Collections.Generic;

namespace DemoServer.Services {

    /// <summary>
    /// A bank consists of any number of bank accounts, identified by their account number.
    /// We could use a simple singleton instance of this class, but to demonstrate the
    /// "ASP.NET Core way" to do things, this service is registered as a singleton service
    /// in <see cref="Startup"/> and is injected into the <see cref="BankServerRpc"/> class.
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
