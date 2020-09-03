using System.Threading.Tasks;

namespace RpcLib.Utils {

    /// <summary>
    /// Interface for serialization into JSON and deserialization from JSON.
    /// It is intentially kept very simple, so that any JSON library with its
    /// custom settings can easily be adapted to be used within this library.
    /// </summary>
    public interface IJsonLib {

        /// <summary>
        /// Deserializes the given JSON string to an object of the given type.
        /// </summary>
        public T FromJson<T>(string json);

        /// <summary>
        /// Serializes the given object to JSON.
        /// </summary>
        public string ToJson(object data);

    }

}
