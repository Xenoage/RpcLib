namespace Xenoage.RpcLib.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

/// <summary>
/// Generates the base class for the real implementation for a given interface,
/// based on the RpcMethods class.
/// It contains a Execute method with maps calls with serialized method calls
/// to real method calls within this class.
/// </summary>
internal class RpcMethodsGenerator : GeneratorBase {

    public RpcMethodsGenerator(InterfaceDeclarationSyntax intf) : base(intf) { }

    protected override string ClassNameSuffix => "Base";

    public override string GenerateCode() {
        var code = $@"// Auto-generated code by Xenoage.RpcLib.Generator

namespace {Namespace};

using System;
using System.Threading.Tasks;
using Xenoage.RpcLib.Methods;
using Xenoage.RpcLib.Model;

public abstract class {GeneratedClassName} : RpcMethods, {InterfaceName} {{

    // Abstract method implementations
";
        foreach (var method in Methods) {
            code += $"    public abstract {method.ReturnType} {method.Name}(" +
                string.Join(", ", method.Parameters.Select(param => param.Type + " " + param.Name)) + ");\n";
        }
        code += $@"
    /// <summary>
    /// Mapping of <see cref=""RpcMethod""/> to real method calls (just boilerplate code).
    /// </summary>
    public override Task<byte[]?>? Execute(RpcMethod method) => method.Name switch {{";
        code += "\n";
        foreach (var method in Methods) {
            code += $"        \"{method.Name}\" => {method.Name}(";
            code += string.Join(", ", method.Parameters.Select((p, index) =>
                $"method.GetParam<{p.Type}>({index})"));
            code += ").Serialize(),\n";
        }
        code += @"        _ => null
    };

}";
        return code;
    }

}