using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Tests for <see cref="RpcMessage"/>.
    /// </summary>
    [TestClass]
    public class RpcMessageTest {

        [TestMethod]
        public void Encode_RpcMethod() {
            // With parameters
            byte[] actual = RpcMessage.Encode(testMethod).Data;
            CollectionAssert.AreEqual(testMethodBytes, actual);
            // Without parameters
            actual = RpcMessage.Encode(testMethod_NoParams).Data;
            CollectionAssert.AreEqual(testMethodBytes_NoParams, actual);
        }

        [TestMethod]
        public void Encode_RpcResult_Success() {
            byte[] actual = RpcMessage.Encode(testResultSuccess).Data;
            CollectionAssert.AreEqual(testResultSuccessBytes, actual);
        }

        [TestMethod]
        public void Encode_RpcMethod_Failure() {
            // With message
            byte[] actual = RpcMessage.Encode(testResultFailure).Data;
            CollectionAssert.AreEqual(testResultFailureBytes, actual);
            // Without message
            actual = RpcMessage.Encode(testResultFailure_NoMessage).Data;
            CollectionAssert.AreEqual(testResultFailureBytes_NoMessage, actual);
        }

        [TestMethod]
        public void IsRpcMethod_Test() {
            Assert.IsTrue(RpcMessage.FromData(testMethodBytes).IsRpcMethod());
            Assert.IsTrue(RpcMessage.FromData(testMethodBytes_NoParams).IsRpcMethod());
            Assert.IsFalse(RpcMessage.FromData(testResultSuccessBytes).IsRpcMethod());
            Assert.IsFalse(RpcMessage.FromData(testResultFailureBytes).IsRpcMethod());
            Assert.IsFalse(RpcMessage.FromData(testResultFailureBytes_NoMessage).IsRpcMethod());
        }

        [TestMethod]
        public void IsRpcResult_Test() {
            Assert.IsFalse(RpcMessage.FromData(testMethodBytes).IsRpcResult());
            Assert.IsFalse(RpcMessage.FromData(testMethodBytes_NoParams).IsRpcResult());
            Assert.IsTrue(RpcMessage.FromData(testResultSuccessBytes).IsRpcResult());
            Assert.IsTrue(RpcMessage.FromData(testResultFailureBytes).IsRpcResult());
            Assert.IsTrue(RpcMessage.FromData(testResultFailureBytes_NoMessage).IsRpcResult());
        }

        [TestMethod]
        public void Decode_RpcMethod() {
            // With parameters
            var actual = RpcMessage.FromData(testMethodBytes).DecodeRpcMethod();
            Assert.AreEqual(testMethod, actual);
            // Without parameters
            actual = RpcMessage.FromData(testMethodBytes_NoParams).DecodeRpcMethod();
            Assert.AreEqual(testMethod_NoParams, actual);
        }

        [TestMethod]
        public void Decode_RpcResult_Success() {
            var actual = RpcMessage.FromData(testResultSuccessBytes).DecodeRpcResult();
            Assert.AreEqual(testResultSuccess, actual);
        }

        [TestMethod]
        public void Decode_RpcMethod_Failure() {
            // With message
            var actual = RpcMessage.FromData(testResultFailureBytes).DecodeRpcResult();
            Assert.AreEqual(testResultFailure, actual);
            // Without message
            actual = RpcMessage.FromData(testResultFailureBytes_NoMessage).DecodeRpcResult();
            Assert.AreEqual(testResultFailure_NoMessage, actual);
        }


        public RpcMessageTest() {

            // Create RpcMethod test data with parameters
            testMethod = new RpcMethod {
                ID = 13, Name = "Hi", Parameters =
                    new List<byte[]> { new byte[] { 100, 101 }, new byte[] { 255 } }
            };
            testMethodBytes = new byte[] {
                (byte)'1', (byte)'M', 13, 0, 0, 0, 0, 0, 0, 0, // Header, ID
                (byte)'H', (byte)'i', (byte)';', 2, // Method name "Hi", 2 Parameters
                2, 0, 0, 0, 100, 101, // 1st parameter: 2 bytes: 100 and 101
                1, 0, 0, 0, 255 }; // 2st parameter: 1 byte

            // Create RpcMethod test data without parameters
            testMethod_NoParams = new RpcMethod {
                ID = 1, Name = "Hey"
            };
            testMethodBytes_NoParams = new byte[] {
                (byte)'1', (byte)'M', 1, 0, 0, 0, 0, 0, 0, 0, // Header, ID
                (byte)'H', (byte)'e', (byte)'y', (byte)';', 0 // Method name "Hey", 0 Parameters
            };

            // Create RpcResult test data (successful state)
            testResultSuccess = new RpcResult {
                MethodID = 300, ReturnValue = new byte[] { 200, 201, 202 }
            };
            testResultSuccessBytes = new byte[] {
                (byte)'1', (byte)'R', 0x2C, 0x01, 0, 0, 0, 0, 0, 0, (byte)'S', // Header, ID, Success
                3, 0, 0, 0, 200, 201, 202 }; // Return value: 3 bytes

            // Create RpcResult test data (failure state with message)
            testResultFailure = new RpcResult {
                MethodID = 65536, Failure = new RpcFailure { Type = RpcFailureType.Timeout, Message = "Oh" }
            };
            testResultFailureBytes = new byte[] {
                (byte)'1', (byte)'R', 0x00, 0x00, 0x01, 0, 0, 0, 0, 0, (byte)'F', // Header, ID, Failure
                (byte)'T', (byte)'i', (byte)'m', (byte)'e', (byte)'o', (byte)'u', (byte)'t', (byte)';', // Timeout
                2, 0, 0, 0, (byte)'O', (byte)'h' // Message
            }; // Return value: 3 bytes

            // Create RpcResult test data (failure state without message)
            testResultFailure_NoMessage = new RpcResult {
                MethodID = 65536, Failure = new RpcFailure { Type = RpcFailureType.Other }
            };
            testResultFailureBytes_NoMessage = new byte[] {
                (byte)'1', (byte)'R', 0x00, 0x00, 0x01, 0, 0, 0, 0, 0, (byte)'F', // Header, ID, Failure
                (byte)'O', (byte)'t', (byte)'h', (byte)'e', (byte)'r', (byte)';', // Timeout
                0, 0, 0, 0 // No message
            }; // Return value: 3 bytes

        }

        // Test data
        private RpcMethod testMethod;
        private byte[] testMethodBytes;
        private RpcMethod testMethod_NoParams;
        private byte[] testMethodBytes_NoParams;
        private RpcResult testResultSuccess;
        private byte[] testResultSuccessBytes;
        private RpcResult testResultFailure;
        private byte[] testResultFailureBytes;
        private RpcResult testResultFailure_NoMessage;
        private byte[] testResultFailureBytes_NoMessage;

    }

}
