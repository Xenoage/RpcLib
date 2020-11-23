using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xenoage.RpcLib.Model;

namespace Xenoage.RpcLib.Peers {

    /// <summary>
    /// Low-level RPC communication protocol.
    /// 
    /// Format: UTF-8 text (variables shown here as {}), including binary data (variables shown here as [...])
    /// - <see cref="RpcMethod"/> calls are sent in format (UTF-8)
    ///     1M[ID:ulong]{MethodName};[Count(Parameters):byte][Length(Parameters[i]):int][Parameters[i]]...
    /// - <see cref="RpcResult"/> responses are sent in format (UTF-8)
    ///   - when successful
    ///     1R[ID:ulong]S[Length(ReturnValue):int][ReturnValue]
    ///   - when failed
    ///     1R[ID:ulong]F{FailureType};[Length(Message):int]{Message}
    /// 
    /// As shown, the first bytes are (UTF-8 characters):
    /// - First:
    ///   - 1: Version number of this protocol. Allows later extensions while remaining backward compatibility.
    /// - Second:
    ///   - M: This is a <see cref="RpcMethod"/>
    ///   - R: This is a <see cref="RpcResult"/>
    /// </summary>
    public class RpcMessage {

        /// <summary>
        /// The encoded data.
        /// </summary>
        public byte[] Data { get; set; } = new byte[0];


        /// <summary>
        /// Creates a new message from the given encoded data.
        /// </summary>
        public static RpcMessage FromData(byte[] data) =>
            new RpcMessage { Data = data };

        /// <summary>
        /// Encodes the given method call to a RPC message.
        /// </summary>
        public static RpcMessage Encode(RpcMethod methodCall) {
            byte[] name = Encoding.UTF8.GetBytes(methodCall.Name);
            int parametersCount = methodCall.Parameters?.Count ?? 0;
            int length = 2 /* Header */ + 8 /* ID */ +
                name.Length + 1 + /* Method name and ";" */
                1 + (methodCall.Parameters?.Sum(it => 4 + it.Length) ?? 0); /* Parameters: Count, and for each: length and data */
            byte[] data = new byte[length];
            int pos = 0;
            data[pos++] = (byte)'1'; // Header
            data[pos++] = (byte)'M';
            Array.Copy(BitConverter.GetBytes(methodCall.ID), 0, data, pos, 8); // ID
            pos += 8;
            Array.Copy(name, 0, data, pos, name.Length); // Name
            pos += name.Length;
            data[pos++] = (byte)';';
            data[pos++] = (byte)parametersCount;
            if (parametersCount > 0) {
                foreach (var param in methodCall.Parameters!) {
                    Array.Copy(BitConverter.GetBytes(param.Length), 0, data, pos, 4); // Param length
                    pos += 4;
                    Array.Copy(param, 0, data, pos, param.Length); // Param data
                    pos += param.Length;
                }
            }
            return new RpcMessage { Data = data };
        }

        /// <summary>
        /// Encodes the given RPC result to a RPC message.
        /// </summary>
        public static RpcMessage Encode(RpcResult result) {
            int length = 2 /* Header */ + 8 /* ID */ + 1 /* Success/Failure */;
            byte[]? failureType = null;
            int failureMessageLength = 0;
            byte[]? failureMessage = null;
            int returnValueLength = result.ReturnValue?.Length ?? 0;
            if (result.Failure == null) {
                // Successful
                length += 4 + returnValueLength; /* Length and data */
            }
            else {
                // Failure
                failureType = Encoding.UTF8.GetBytes(""+result.Failure.Type);
                failureMessageLength = result.Failure.Message?.Length ?? 0;
                if (failureMessageLength > 0)
                    failureMessage = Encoding.UTF8.GetBytes(result.Failure.Message!);
                length += failureType.Length + 1 /* ";" */ + 4 + failureMessageLength;
            }
            byte[] data = new byte[length];
            int pos = 0;
            data[pos++] = (byte)'1'; // Header
            data[pos++] = (byte)'R';
            Array.Copy(BitConverter.GetBytes(result.MethodID), 0, data, pos, 8); // ID
            pos += 8;
            // Success or failure
            if (result.Failure == null) {
                // Successful
                data[pos++] = (byte)'S';
                Array.Copy(BitConverter.GetBytes(returnValueLength), 0, data, pos, 4); // Return value length
                pos += 4;
                if (returnValueLength > 0) {
                    Array.Copy(result.ReturnValue!, 0, data, pos, returnValueLength); // Return value data
                    pos += returnValueLength;
                }
            } else {
                // Failure
                data[pos++] = (byte)'F';
                Array.Copy(failureType!, 0, data, pos, failureType!.Length); // Failure type
                pos += failureType!.Length;
                data[pos++] = (byte)';';
                Array.Copy(BitConverter.GetBytes(failureMessageLength), 0, data, pos, 4); // Failure message length
                pos += 4;
                if (failureMessageLength > 0) {
                    Array.Copy(failureMessage!, 0, data, pos, failureMessageLength); // Failure message
                    pos += failureMessageLength;
                }
            }
            return new RpcMessage { Data = data };
        }

        /// <summary>
        /// Returns true, iff this message encodes a <see cref="RpcMethod"/>.
        /// This does not ensure yet that this is a valid message, it just checks the first bytes.
        /// Call <see cref="DecodeRpcMethod"/> to test this and get the whole message.
        /// </summary>
        public bool IsRpcMethod() =>
            Data.Length > 2 && Data[0] == (byte)'1' && Data[1] == (byte)'M';

        /// <summary>
        /// Returns true, iff this message encodes a <see cref="RpcResult"/>.
        /// This does not ensure yet that this is a valid message, it just checks the first bytes.
        /// Call <see cref="DecodeRpcResult"/> to test this and get the whole message.
        /// </summary>
        public bool IsRpcResult() =>
            Data.Length > 2 && Data[0] == (byte)'1' && Data[1] == (byte)'R';

        /// <summary>
        /// Gets the <see cref="RpcMethod"/> content of this message or throws an exception, if it
        /// is no valid message of this type.
        /// <see cref="IsRpcMethod"/> can be called before to find out the type of this message.
        /// </summary>
        public RpcMethod DecodeRpcMethod() {
            if (false == IsRpcMethod())
                throw new FormatException("Header wrong");
            try {
                int pos = 2;
                var ret = new RpcMethod();
                // ID
                ret.ID = BitConverter.ToUInt64(Data, pos);
                pos += 8;
                // Method name
                int methodNameEnd = Array.FindIndex(Data, pos, it => it == (byte)';');
                if (methodNameEnd == -1)
                    throw new FormatException("Method end not found");
                ret.Name = Encoding.UTF8.GetString(Data, pos, methodNameEnd - pos);
                pos = methodNameEnd + 1;
                // Parameters
                byte paramsCount = Data[pos++];
                if (paramsCount > 0) {
                    ret.Parameters = new List<byte[]>(capacity: paramsCount);
                    for (byte iParam = 0; iParam < paramsCount; iParam++) {
                        int paramLength = BitConverter.ToInt32(Data, pos);
                        pos += 4;
                        ret.Parameters.Add(
                            new ArraySegment<byte>(Data, pos, paramLength).ToArray());
                        pos += paramLength;
                    }
                }
                return ret;
            }
            catch (Exception ex) {
                throw new FormatException("Content wrong: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="RpcResult"/> content of this message or throws an exception, if it
        /// is no valid message of this type.
        /// <see cref="IsRpcResult"/> can be called before to find out the type of this message.
        /// </summary>
        public RpcResult DecodeRpcResult() {
            if (false == IsRpcResult())
                throw new FormatException("Header wrong");
            try {
                int pos = 2;
                var ret = new RpcResult();
                // ID
                ret.MethodID = BitConverter.ToUInt64(Data, pos);
                pos += 8;
                // Success or failure
                bool isSuccess = Data[pos++] == (byte)'S';
                if (isSuccess) {
                    // Successful
                    int returnValueLength = BitConverter.ToInt32(Data, pos);
                    pos += 4;
                    if (returnValueLength > 0) {
                        ret.ReturnValue = new ArraySegment<byte>(Data, pos, returnValueLength).ToArray();
                        pos += returnValueLength;
                    }
                }
                else {
                    // Failure
                    ret.Failure = new RpcFailure();
                    // Failure type
                    int failureTypeEnd = Array.FindIndex(Data, pos, it => it == (byte)';');
                    if (failureTypeEnd == -1)
                        throw new FormatException("FailureType end not found");
                    ret.Failure.Type = Enum.Parse<RpcFailureType>(
                        Encoding.UTF8.GetString(Data, pos, failureTypeEnd - pos));
                    pos = failureTypeEnd + 1;
                    // Message
                    int messageLength = BitConverter.ToInt32(Data, pos);
                    pos += 4;
                    if (messageLength > 0) {
                        ret.Failure.Message = Encoding.UTF8.GetString(Data, pos, messageLength);
                        pos += messageLength;
                    }
                }
                return ret;
            } catch (Exception ex) {
                throw new FormatException("Content wrong: " + ex.Message, ex);
            }
        }

    }

}
