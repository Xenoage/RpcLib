namespace Xenoage.RpcLib.Generator.Model;

/// <summary>
/// Parameter within a method declaration in an IRpcMethods interface.
/// </summary>
internal record Parameter {
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}