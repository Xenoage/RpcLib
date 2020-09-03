namespace RpcLib.Auth {

    /// <summary>
    /// Result of an authentication request.
    /// </summary>
    public class AuthResult {

        /// <summary>
        /// The ID of the requesting client, even when the authentication failed.
        /// Can be null, when it was not possible to find out the client ID from
        /// the request.
        /// </summary>
        public string? ClientID { get; }

        /// <summary>
        /// True, iff the authentication was successful, e.g. if the password
        /// or the authentication token was correct.
        /// </summary>
        public bool Success { get; }


        public AuthResult(string? clientID, bool success) {
            ClientID = clientID;
            Success = success;
        }

    }

}
