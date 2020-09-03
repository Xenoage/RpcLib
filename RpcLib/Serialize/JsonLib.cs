using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// This is the default (de)serializer for JSON in the RpcLib.
    /// Realized using the Newtonsoft.JSON library, using a $type property
    /// for serializing the .NET types of inner properties, camelCase format
    /// and ignoring null value properties.
    /// </summary>
    public class JsonLib : IJsonLib {

        /// <summary>
        /// Deserializes the given JSON string to an object of the given type.
        /// </summary>
        public T FromJson<T>(string json) =>
            JsonConvert.DeserializeObject<T>(json, settings)!;

        /// <summary>
        /// Serializes the given object to JSON.
        /// </summary>
        public string ToJson(object data) =>
            JsonConvert.SerializeObject(data, settings);

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented, // Intended is better readable for humans (while debugging)
            TypeNameHandling = TypeNameHandling.Auto, // Use $type properties to resolve types
            ContractResolver = new CamelCasePropertyNamesContractResolver(), // camelCaseNaming
            NullValueHandling = NullValueHandling.Ignore // More compact by ignoring null value properties
        };

    }

}
