using Spectre.Console.Cli;
using System.ComponentModel;

namespace Versy.CommandParser;

public class Options : CommandSettings{
    [CommandArgument(0, "<INPUT_FILE>")]
    [Description("Path of the file with the source code.")]
    public string InputFile { get; set; }
    
    [CommandArgument(1, "<OUTPUT_FILE>")]
    [Description("Specifies the output path for the generated files.")]
    public string OutputPath { get; set; }
    
    [CommandOption("-t|--outputtk <PATH>")]
    [Description("Specifies the output path for the tokens file.")]
    public string TokensOutputPath { get; set; } = "";

    [CommandOption("-a|--outputast <PATH>")]
    [Description("Specifies the output path for the ast tree file.")]
    public string ASTOutputPath { get; set; } = "";
    
    [CommandOption("-c|--csoutput <PATH>")]
    [Description("Specifies the output path for C# generated code.")]
    public string CSOutputPath { get; set; } = "";
    
    [CommandOption("-r|--run")]
    [Description("Run automatically after compiling.")]
    public bool AutoRun { get; set; }
        
    [CommandOption("-s|--singlefile")]
    [Description("Produce one exe.")]
    public bool singlefile { get; set; }
}
