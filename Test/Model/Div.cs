using System;
using System.Collections.Generic;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// A simple calculation task, dividing two numbers.
    /// Method name: "Div", byte-typed parameters (dividend and divisor),
    /// the byte-typed result is dividend/divisor (including a failure because of division by 0).
    /// Dividend is always MethodID % 111, Divisor is always MethodID % 11.
    /// </summary>
    public class Div {

        public const string methodName = "Div";

        public ulong methodID;
        public byte dividend;
        public byte divisor;
        public RpcResult? result = null;

        public static Div CreateNew(ulong id) {
            return new Div {
                methodID = id,
                dividend = (byte)(id % 111),
                divisor = (byte)(id % 11),
            };
        }

        public static Div FromMethod(RpcMethod method) {
            if (method.Name != methodName)
                throw new Exception("Wrong method name");
            return new Div {
                methodID = method.ID,
                dividend = method.Parameters[0][0],
                divisor = method.Parameters[1][0]
            };
        }

        public RpcMethod ToMethod() {
            return new RpcMethod {
                ID = methodID,
                Name = methodName,
                Parameters = new List<byte[]> {
                    new byte[] { dividend },
                    new byte[] { divisor }
                }
            };
        }

        public RpcResult ComputeExpectedResult() => new RpcResult {
            MethodID = methodID,
            ReturnValue = divisor > 0 ? new byte[] { (byte)(dividend / divisor) } : null,
            Failure = divisor > 0 ? null : new RpcFailure {
                Type = RpcFailureType.RemoteException,
                Message = new DivideByZeroException().Message
            }
        };

    }

}
