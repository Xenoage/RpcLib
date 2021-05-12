using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xenoage.RpcLib.Serialization {

    /// <summary>
    /// This is the default (de)serializer in this library, using JSON as the data format.
    /// Realized using .NET's built in System.Text.Json library.
    /// Caution: When polymorphic deserialization is required, use a different library like
    /// Newtonsoft's Json.NET with the appropriate settings.
    /// It may also be a good idea to also include compression for larger messages.
    /// This is all transparent for this library, since it just provides the channel for
    /// transporting messages, not enforcing a specific format for the content.
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
            WriteIndented = false, // Compact representation
            Converters = { new JsonStringEnumConverter() }, // Enum values as strings, not as numbers
            // TODO: More compact by ignoring null value properties - but only available since .NET 5.0 - add later
            // DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

    }

}
