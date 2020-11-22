﻿using System.Collections.Generic;
using System.Linq;
using static Xenoage.RpcLib.Serialization.Serializer;
using static Xenoage.RpcLib.Utils.CoreUtils;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// A single RPC method invocation.
    /// Together with the target peer ID, the method name and serialized parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// This is the data which is transferred to the other peer over the network
    /// when the method is called. Additional information, which remains on the local
    /// side, is stored in <see cref="RpcCall"/>.
    /// </summary>
    public class RpcMethod {

        #region Properties

        /// <summary>
        /// Unique ID of this call.
        /// The ID looks like this: ({unix time in ms}{0000-9999}),
        /// i.e. ascending over time.
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// The name of the method. Equals the actual C# method name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The list of the serialized parameter values.
        /// See <see cref="ISerializer"/> how the parameter values are encoded.
        /// </summary>
        public List<byte[]> Parameters { get; set; }

        #endregion

        #region Creation

        /// <summary>
        /// Creates a new RPC call, using the given method name and parameters.
        /// The parameters must be serializable objects or primitive values.
        /// </summary>
        public static RpcMethod Create(string name, params object[] parameters) =>
            new RpcMethod(name, parameters);

        /// <summary>
        /// Default constructor. For JSON deserialization only.
        /// In the program code, use the other constructor.
        /// </summary>
#pragma warning disable CS8618
        public RpcMethod() {
#pragma warning restore CS8618
        }

        /// <summary>
        /// Creates a new RPC method call, using the given name and parameters.
        /// The parameters must be serializable objects.
        /// </summary>
        private RpcMethod(string name, params object[] parameters) {
            lock (lockSync) {
                // This works for up to 10.000 calls per millisecond. Should really be enough!
                loop++;
                if (loop > 9999)
                    loop = 0;
                ID = ((ulong)TimeNowMs()) * 10000 + loop;
            }
            Name = name;
            Parameters = parameters.Select(Serialize).ToList();
        }
        private object lockSync = new object();
        private static ulong loop = 0;

        #endregion

    }

}