using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

namespace Xenoage.RpcLib.Generator {

    [Generator]
    public class Main : ISourceGenerator {

        public void Initialize(GeneratorInitializationContext context) {
#if _DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif 
        }

        public void Execute(GeneratorExecutionContext context) {

            // Find all interfaces which derive from IRpcMethods
            var interfs = context.Compilation.SyntaxTrees.SelectMany(tree => tree
                .GetRoot()
                .DescendantNodes()
                .Where(n => n is InterfaceDeclarationSyntax)
                .Select(n => n as InterfaceDeclarationSyntax)
                .Where(n => n!.BaseList?.Types.Any(t => t.ToString() == "IRpcMethods") ?? false)).ToList();

            // Create stub sources
            foreach (var interf in interfs) {
                var generator = new RpcMethodsStubGenerator(interf!);
                string source = generator.GenerateCode();
                context.AddSource($"{generator.GeneratedClassName}.g.cs", source);
            }

            // Create implementation base sources
            foreach (var interf in interfs) {
                var generator = new RpcMethodsGenerator(interf!);
                string source = generator.GenerateCode();
                context.AddSource($"{generator.GeneratedClassName}.g.cs", source);
            }
        }



    }

    

}
