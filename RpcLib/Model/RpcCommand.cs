using System.Collections.Generic;
using System.Linq;
using static Xenoage.RpcLib.Serialization.Serializer;
using static Xenoage.RpcLib.Utils.CoreUtils;

namespace Xenoage.RpcLib.Model {

    /// <summary>
    /// A single RPC command instance, i.e. one instance exists for each RPC function call.
    /// Together with the target peer ID, the method name and serialized parameters,
    /// it stores a packet number which can be used to ensure that a call is evaluated
    /// only a single time, also if it arrives multiple times for any reason.
    /// This is the data which is transferred to the other peer over the network
    /// when the command is called. Additional information, which remains on the local
    /// side, is stored in <see cref="RpcCall"/>.
    /// </summary>
    public class RpcCommand {

        #region Properties

        /// <summary>
        /// Unique ID of this command.
        /// The ID looks like this: ({unix time in ms}{0000-9999}),
        /// i.e. ascending over time.
        /// </summary>
        public ulong ID { get; set; }

        /// <summary>
        /// The name of the metho. Equals the actual C# method name.
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// The list of the serialized parameter values.
        /// See <see cref="ISerializer"/> how the parameter values are encoded.
        /// </summary>
        public List<byte[]> MethodParameters { get; set; }

        #endregion

        #region Creation

        /// <summary>
        /// Creates a new RPC command, using the given method name and parameters.
        /// The parameters must be serializable objects or primitive values.
        /// </summary>
        public static RpcCommand Create(string methodName, params object[] methodParameters) =>
            new RpcCommand(methodName, methodParameters);

        /// <summary>
        /// Default constructor. For JSON deserialization only.
        /// In the program code, use the other constructor.
        /// </summary>
#pragma warning disable CS8618
        public RpcCommand() {
#pragma warning restore CS8618
        }

        /// <summary>
        /// Creates a new encoded RPC command, using the given method name and parameters.
        /// The parameters must be JSON-encodable objects.
        /// </summary>
        private RpcCommand(string methodName, params object[] methodParameters) {
            lock (lockSync) {
                // This works for up to 10.000 commands per millisecond. Should really be enough!
                loop++;
                if (loop > 9999)
                    loop = 0;
                ID = ((ulong)TimeNowMs()) * 10000 + loop;
            }
            MethodName = methodName;
            MethodParameters = methodParameters.Select(Serialize).ToList();
        }
        private object lockSync = new object();
        private static ulong loop = 0;

        #endregion

    }

}
