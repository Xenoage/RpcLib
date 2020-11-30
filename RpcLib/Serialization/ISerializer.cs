namespace Xenoage.RpcLib.Serialization {

    /// <summary>
    /// Interface for serialization of .NET class instances and primitive values
    /// into a custom data format and deserialization from this format back into
    /// the .NET representations.
    /// This interface is intentially kept very simple, so that any serialization
    /// library with its custom settings can easily be adapted to be used within this library.
    /// See <see cref="JsonSerializer"/> for an example.
    /// </summary>
    public interface ISerializer {

        /// <summary>
        /// Deserializes the given data to an object of the given type.
        /// </summary>
        public T Deserialize<T>(byte[] data);

        /// <summary>
        /// Serializes the given object to a byte array.
        /// </summary>
        public byte[] Serialize(object value);

    }

}
