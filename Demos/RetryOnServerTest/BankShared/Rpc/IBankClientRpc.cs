using RpcLib;
using RpcLib.Model;
using System.Threading.Tasks;

namespace BankShared.Rpc {

    /// <summary>
    /// Demo interface for showcasing the automatic retry capabilities of this library,
    /// using a simple client-side bank account manager.
    /// </summary>
    public interface IBankClientRpc : IRpcFunctions {

        /// <summary>
        /// Gets the current account balance in cents of the account with the given number.
        /// Retrying this command is not required, since it does not modify state on the bank client.
        /// </summary>
        Task<int> GetBalance(int accountNumber);

        /// <summary>
        /// Adds the given amount of cents to the account with the given number
        /// and returns the new account balance in cents.
        /// This command should be retried until the bank client is finally reached,
        /// i.e. <see cref="RpcRetryStrategy.RetryWhenOnline"/> must be used.
        /// </summary>
        [RpcOptions(RetryStrategy = RpcRetryStrategy.RetryWhenOnline)]
        Task<int> AddMoney(int accountNumber, int cents);

        /// <summary>
        /// Changes the name of the owner of the account with the given number.
        /// This command should be retried until the bank client is finally reached,
        /// but only the latest change is relevant,
        /// i.e. <see cref="RpcRetryStrategy.RetryNewestWhenOnline"/> must be used.
        /// </summary>
        [RpcOptions(RetryStrategy = RpcRetryStrategy.RetryNewestWhenOnline)]
        Task ChangeOwnerName(int accountNumber, string ownerName);

    }

}
