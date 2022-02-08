namespace Xenoage.RpcLib.Generator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using Xenoage.RpcLib.Generator.Model;

internal abstract class GeneratorBase {

    public GeneratorBase(InterfaceDeclarationSyntax intf) {
        // Analyse class name, namespace and methods
        InterfaceName = intf.Identifier.Text;
        GeneratedClassName = GetGeneratedClassName(intf);
        Namespace = GetNamespaceFrom(intf);
        Methods = intf.Members
            .Where(it => it is MethodDeclarationSyntax)
            .Select(it => (MethodDeclarationSyntax)it)
            .Select(method => new Method {
                Name = method.Identifier.Text,
                TaskReturnType = GetTaskReturnType(method.ReturnType.ToString()),
                Parameters = method.ParameterList.Parameters.Select(param => new Parameter {
                    Name = param.Identifier.Text,
                    Type = param.Type!.ToString()
                }).ToList()
            }).ToList();
    }

    public string InterfaceName { get; }

    public string GeneratedClassName { get; }

    public string Namespace { get; }

    protected List<Method> Methods { get; }

    /// <summary>
    /// Used for <see cref="GetStubClassName"/>.
    /// For example "Base" for "IMyInterfaceName" => "MyInterfaceNameBase".
    /// </summary>
    protected abstract string ClassNameSuffix { get; }

    public abstract string GenerateCode();

    /// <summary>
    /// Gets the name of the base class, derived from the name of the given interface
    /// and <see cref="ClassNameSuffix"/>, e.g. "IMyInterfaceName" => "MyInterfaceNameBase".
    /// </summary>
    protected string GetGeneratedClassName(InterfaceDeclarationSyntax intf) {
        string intfName = intf.Identifier.Text;
        return (intfName.StartsWith("I") ? intfName.Substring(1) : intfName) + ClassNameSuffix;
    }

    private string GetNamespaceFrom(SyntaxNode s) =>
        s.Parent switch {
            NamespaceDeclarationSyntax it => it.Name.ToString(),
            null => "",
            _ => GetNamespaceFrom(s.Parent)
        };

    /// <summary>
    /// Returns
    /// - null for Task
    /// - XYZ for Task&lt;XYZ&gt;
    /// </summary>
    private string? GetTaskReturnType(string type) {
        if (type == "Task")
            return null;
        else if (type.StartsWith("Task<") && type.EndsWith(">"))
            return type.Substring(5, type.Length - 6);
        else
            return "[Error: Invalid return type (use Task or Task<...>)]";
    }

}
