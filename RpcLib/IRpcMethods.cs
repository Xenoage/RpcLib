namespace Xenoage.RpcLib {

    /// <summary>
    /// Marker interface for all classes/interfaces that define methods which can be called by
    /// RPC calls on the client side by the server or on the server side by the client.
    /// Each method must return a Task with either a single (de)serializable value or no data,
    /// and accept any number of (de)serializable parameters (or none).
    /// </summary>
    public interface IRpcMethods {
    }

}
