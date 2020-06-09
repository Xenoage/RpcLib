﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RpcLib.Rpc.Utils {

    /// <summary>
    /// Serialization to JSON and deserialization from JSON.
    /// Realized using the Newtonsoft.JSON library, using a $type property
    /// for serializing the .NET types of inner properties.
    /// </summary>
    public static class JsonLib {

        /// <summary>
        /// Deserializes the given JSON string to an object of the given type.
        /// </summary>
        public static T? FromJson<T>(string json) where T : class =>
            JsonConvert.DeserializeObject<T>(json, settings);

        /// <summary>
        /// Serializes the given object to JSON.
        /// </summary>
        public static string ToJson(object data) =>
            JsonConvert.SerializeObject(data, settings);

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented, // Intended is better readable for humans (while debugging)
            TypeNameHandling = TypeNameHandling.Auto, // Use $type properties to resolve types
            ContractResolver = new CamelCasePropertyNamesContractResolver(), // C# style, CamelCaseNaming
            NullValueHandling = NullValueHandling.Ignore // More compact by ignoring null value properties
        };

    }

}
