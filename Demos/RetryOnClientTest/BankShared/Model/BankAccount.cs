namespace BankShared.Model {

    /// <summary>
    /// Simple bank account demo class.
    /// </summary>
    public class BankAccount {

        /// <summary>
        /// The unique ID of the account, e.g. 1234.
        /// </summary>
        public int AccountNumber { get; set; }

        /// <summary>
        /// Name of the owner of the account, e.g. "John Doe".
        /// </summary>
        public string OwnerName { get; set; } = "?";

        /// <summary>
        /// Account balance in cents.
        /// </summary>
        public int Cents { get; set; } = 0;

    }

}
