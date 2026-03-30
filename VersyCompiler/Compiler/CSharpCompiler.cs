using Spectre.Console;
using System.Diagnostics;

namespace Versy.Compiler;

public class CSharpCompiler {
    private string CSharpCode    { get; set; }
    private string outputPath    { get; set; } // .exe
    private string csOutPath     { get; set; } // .cs
    private string csprojOutPath { get; set; } // .csproj
    private bool   singlefile    { get; set; }

    public CSharpCompiler(string CSharpCode, string outputPath, string csOutPath, string csprojOutPath, bool singlefile) {
        this.CSharpCode     = CSharpCode;
        this.outputPath     = outputPath;
        this.csOutPath      = csOutPath + ".cs";
        this.csprojOutPath  = csprojOutPath;
        this.singlefile     = singlefile;
    }   

    public bool compile() {
        string assemblyName = Path.GetFileNameWithoutExtension(outputPath);
        string csprojContent = $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>{assemblyName}</AssemblyName>
    <PublishSingleFile>{singlefile}</PublishSingleFile>
    <SelfContained>{singlefile}</SelfContained>
    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
    <DebugType>none</DebugType>
    <DebugSymbols>false</DebugSymbols>
  </PropertyGroup>
</Project>";
        
        try {
            File.WriteAllText(csprojOutPath, csprojContent);
            File.WriteAllText(csOutPath, CSharpCode);
        } catch (Exception ex) {
            AnsiConsole.MarkupLine($"[red]✘ Error while genereting .cs & .csproj files:[/]");
            AnsiConsole.WriteException(ex);
            return false;
        }
        
        string tempOutputDir = outputPath.Replace(".exe", "");
        
        ProcessStartInfo psi = new ProcessStartInfo {
            FileName               = "dotnet",
            Arguments              = $"publish \"{csprojOutPath}\" -c Release -o \"{tempOutputDir}\" --nologo -v quiet",
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };
        
        using var process = Process.Start(psi);
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode == 0) {
            try {
                string sourceExe = Path.Combine(tempOutputDir, assemblyName + ".exe");
                if (File.Exists(sourceExe)) AnsiConsole.MarkupLine($"[lime]✔  Executable moved to:[/] {outputPath}");
                return true;
            } catch (Exception ex) { AnsiConsole.WriteException(ex); }
        } else AnsiConsole.MarkupLine($"[red]✘ Compilation failed.[/]\n{error}");
        return false;
    }
}