namespace Xenoage.RpcLib.Generator.Model;

using System.Collections.Generic;

/// <summary>
/// Method declaration in an IRpcMethods interface.
/// </summary>
internal record Method {

    public string Name { get; set; } = "";
    public string? TaskReturnType { get; set; } = "";
    public List<Parameter> Parameters { get; set; } = new();

    public string ReturnType =>
        TaskReturnType != null ? $"Task<{TaskReturnType}>" : "Task";

    public string TypeArgument =>
        TaskReturnType != null ? $"<{TaskReturnType}>" : "";

}
