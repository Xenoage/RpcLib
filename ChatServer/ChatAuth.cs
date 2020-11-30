using Xenoage.RpcLib.Auth;

namespace Chat {

    /// <summary>
    /// In this chat server demo, the authentication is based on HTTP Basic Auth headers
    /// and the correct password is the username in uppercase letters.
    /// </summary>
    public class ChatAuth : RpcServerBasicAuth {

        public override bool AreCredentialsCorrect(string username, string password) =>
            username.ToUpper() == password;

    }

}
