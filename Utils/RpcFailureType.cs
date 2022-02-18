namespace Utils;

/// <summary>
/// Type of RPC failure.
/// </summary>
public enum RpcFailureType {

    /// <summary>
    /// An exception happened on the remote side, when executing the call.
    /// Typical examples are an I/O error or a division-by-0 exception.
    /// Since this is no problem of the RPC engine itself, the RPC call should only be
    /// repeated by the caller if there is a reasonable chance that it will work
    /// the next time.
    /// </summary>
    RemoteException,

    /// <summary>
    /// The client could not reach the server, either immediately or after a timeout.
    /// This is probably a network problem or the server does not respond.
    /// In this case, the RPC call could be repeated later.
    /// </summary>
    NetworkFailure

}