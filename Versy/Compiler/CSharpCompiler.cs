using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Spectre.Console;

namespace Versy.Compiler;

public class CSharpCompiler {
    private string CSharpCode { get; set; }
    private string outputPath { get; set; }

    public CSharpCompiler(string CSharpCode, string outputPath) {
        this.CSharpCode      = CSharpCode;
        this.outputPath = outputPath;
    }

    public void compile() {
        MetadataReference[] references = new MetadataReference[] {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(System.Runtime.Loader.AssemblyLoadContext.Default
                                                 .LoadFromAssemblyName(new System.Reflection.AssemblyName("System.Runtime")).Location)
        };
        var syntaxTree = CSharpSyntaxTree.ParseText(CSharpCode);
        CSharpCompilation compiler = CSharpCompilation.Create(
            "GeneratedProgram",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );
        EmitResult result = compiler.Emit(outputPath);

        if (!result.Success) {
            AnsiConsole.MarkupLine("[red]:cross_mark:  Error during compilation:[/]");
            foreach (Diagnostic diagnostic in result.Diagnostics) 
                AnsiConsole.MarkupLine($"\t[red bold]{diagnostic.Id}:[/] [blue]{diagnostic.GetMessage()}[/]");
        }
    }
}
