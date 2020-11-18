using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xenoage.RpcLib.Utils;

namespace Xenoage.RpcLib.Auth {

    /// <summary>
    /// Tests for <see cref="RpcServerBasicAuth"/>.
    /// </summary>
    [TestClass]
    public class RpcServerBasicAuthTest {

        /// <summary>
        /// Test implementation of <see cref="RpcServerBasicAuth"/>.
        /// Password must be username in uppercase letters.
        /// </summary>
        private class TestRpcServerBasicAuth : RpcServerBasicAuth {
            public override bool AreCredentialsCorrect(string username, string password) =>
                username.ToUpper() == password;
        }

        [TestMethod]
        public void MissingAuth_Fails() {
            var request = HttpTestUtils.CreateHttpListenerRequest();
            var auth = new TestRpcServerBasicAuth();
            var result = auth.Authenticate(request);
            Assert.IsNull(result.ClientID);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void WrongAuth_Fails() {
            var request = HttpTestUtils.CreateHttpListenerRequest(client =>
                client.DefaultRequestHeaders.Add("Authorization", "Basic b3R0bzpoYW5z")); // otto:hans
            var auth = new TestRpcServerBasicAuth();
            var result = auth.Authenticate(request);
            Assert.AreEqual("otto", result.ClientID);
            Assert.IsFalse(result.Success);
        }

        [TestMethod]
        public void CorrectAuth_Succeeds() {
            var request = HttpTestUtils.CreateHttpListenerRequest(client =>
                client.DefaultRequestHeaders.Add("Authorization", "Basic b3R0bzpPVFRP")); // otto:OTTO
            var auth = new TestRpcServerBasicAuth();
            var result = auth.Authenticate(request);
            Assert.AreEqual("otto", result.ClientID);
            Assert.IsTrue(result.Success);
        }

    }

    

}
