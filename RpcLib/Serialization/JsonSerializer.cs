using System.Text.Json;

namespace Xenoage.RpcLib.Serialization {

    /// <summary>
    /// This is the default (de)serializer in this library, using JSON as the data format.
    /// Realized using .NET's built in System.Text.Json library.
    /// Caution: When polymorphic deserialization is required, use a different library like
    /// Newtonsoft's Json.NET with the appropriate settings.
    /// </summary>
    public class JsonSerializer : ISerializer {

        /// <summary>
        /// Deserializes the given UTF-8 encoded JSON string to an object of the given type.
        /// </summary>
        public T Deserialize<T>(byte[] json) =>
            System.Text.Json.JsonSerializer.Deserialize<T>(json, options);

        /// <summary>
        /// Serializes the given object to JSON.
        /// </summary>
        public byte[] Serialize(object value) =>
            System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value, options);

        private static readonly JsonSerializerOptions options = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // camelCaseNaming
            // TODO: not found? DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, // More compact by ignoring null value properties
            WriteIndented = false, // Compact representation
        };

    }

}
