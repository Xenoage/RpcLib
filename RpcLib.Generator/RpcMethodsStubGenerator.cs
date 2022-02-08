namespace Xenoage.RpcLib.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

/// <summary>
/// Generates the RPC stub implementation for a given interface,
/// based on the RpcMethodsStub class.
/// It implements each method by calling ExecuteOnRemotePeer
/// with the serialized method name and forwarding its parameters.
/// </summary>
internal class RpcMethodsStubGenerator : GeneratorBase {

    public RpcMethodsStubGenerator(InterfaceDeclarationSyntax intf) : base(intf) { }

    protected override string ClassNameSuffix => "Stub";

    public override string GenerateCode() {
        var code = $@"// Auto-generated code by Xenoage.RpcLib.Generator

namespace {Namespace};

using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Peers;

public class {GeneratedClassName} : RpcMethodsStub, {InterfaceName} {{

    /// <summary>
    /// Use this constructor for stubs on the server side,
    /// i.e. from server-to-client calls when the real implementation of the interface (RpcMethods) is on the client side.
    /// </summary>
    public {GeneratedClassName}(RpcServer localServer, string remoteClientID) : base(localServer, remoteClientID) {{
    }}

    /// <summary>
    /// Use this constructor for stubs on the client side,
    /// i.e. from client-to-server calls when the real implementation of the interface (RpcMethods) is on the server side.
    /// </summary>
    public {GeneratedClassName}(RpcClient localClient) : base(localClient) {{
    }}

";
        foreach (var method in Methods) {
            code += $"    public {method.ReturnType} {method.Name}(" +
                string.Join(", ", method.Parameters.Select(param => param.Type + " " + param.Name)) + ") =>\n";
            code += $"        ExecuteOnRemotePeer{method.TypeArgument}(\"{method.Name}\"" +
                (method.Parameters.Count > 0 ? ", " : "") +
                string.Join(", ", method.Parameters.Select(param => param.Name)) + ");";
            code += "\n\n";
        }
        code += "}";
        return code;
    }

}