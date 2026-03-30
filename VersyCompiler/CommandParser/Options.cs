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

    [CommandOption("-d|--debugfiles")]
    [Description("Add debug files.")]
    public bool DebugFiles { get; set; } = true;

    [CommandOption("-f|--singlefile")]
    [Description("Compile in a single file.")]
    public bool SingleFile { get; set; } = false;
    
    [CommandOption("-r|--run")]
    [Description("Run automatically after compiling.")]
    public bool AutoRun { get; set; }

    public string TokensOutputPath     { get; set; } = "";
    public string ASTOutputPath        { get; set; } = "";
    public string TranslatedOutputPath { get; set; } = "";
    public string ErrorsOutputPath     { get; set; } = "";
}
