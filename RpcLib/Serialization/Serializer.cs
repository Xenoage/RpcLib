using System.Threading.Tasks;

namespace Xenoage.RpcLib.Serialization {

    /// <summary>
    /// Concise (de)serializing calls, using the currently set <see cref="ISerializer"/>.
    /// </summary>
    public static class Serializer {

        /// <summary>
        /// By default, the included <see cref="JsonSerializer"/> is used.
        /// </summary>
        public static ISerializer Instance { get; set; } = new JsonSerializer();

        /// <summary>
        /// Deserializes the given data to an object of the given type.
        /// </summary>
        public static T Deserialize<T>(byte[] data) => Instance.Deserialize<T>(data);

        /// <summary>
        /// Serializes the given object to a byte array.
        /// </summary>
        public static byte[] Serialize(object value) => Instance.Serialize(value);

    }

}
