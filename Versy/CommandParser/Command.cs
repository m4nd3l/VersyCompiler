using Dumpify;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics;
using System.IO;

using Versy.AST;
using Versy.AST.Statements;
using Versy.Codegen;
using Versy.CommandParser;
using Versy.Compiler;
using Versy.Errors;
using Versy.Lexer;
using Versy.Parser;


public class VVCommand : Command<Options> {
    private Options        settings      { get; set; }
    private string         sourceCode    { get; set; }
    private List<Token>    tokens        { get; set; }
    private BlockStatement statement     { get; set; }
    private string         generatedCode { get; set; }
    
    protected override int Execute(CommandContext context, Options settings, CancellationToken cancellationToken) {
        this.settings       = settings;
        settings.OutputPath = Path.GetFileNameWithoutExtension(settings.OutputPath);
        
        settings.CSOutputPath = settings.CSOutputPath.Replace('#', 'S');
        
        printPath("[yellow]Source code[/]", settings.InputFile);
        
        if (settings.TokensOutputPath != "") printPath("[yellow]Tokens Output[/]", settings.TokensOutputPath);
        if (settings.ASTOutputPath != "") printPath("[yellow]AST Output[/]", settings.ASTOutputPath);
        if (settings.CSOutputPath != "") printPath("[yellow]C# Output[/]", settings.CSOutputPath);
        
        printPath("[yellow]Executable[/]", Path.Combine(Path.GetDirectoryName(settings.InputFile), settings.OutputPath + ".exe"));
        
        printStatus($"Getting {settings.InputFile} content...", get, "[lime]:check_mark:  Successfully extracted the input file's content.[/]");
        printStatus($"Lexing...", lex, "[lime]:check_mark:  Successfully lexed.[/]");
        printStatus($"Parsing...", parse, "[lime]:check_mark:  Successfully parsed.[/]");
        printStatus($"Generating C# code...", codegen, "[lime]:check_mark:  Successfully generated C# code.[/]");
        printStatus($"Compiling...", compile, "[lime]:check_mark:  Successfully compiled![/]");
        
        if (settings.AutoRun) printStatus($"Running...", run);
        else if (AnsiConsole.Confirm("Run it?")) printStatus($"Running...", run);
        
        return exit();
    }
    
    private void get(StatusContext ctx) {
        try { sourceCode = File.ReadAllText(settings.InputFile); }
        catch (Exception ex) { error(ex); exit(null, true, 1, true); }
    }
    private void lex(StatusContext ctx) {
        try {
            VersyLexer lexer = new VersyLexer(sourceCode);
            bool succeded = lexer.tokenize();
            tokens = lexer.tokens;
            using var writer = new StringWriter();
            string text = "";
            foreach (Token token in tokens) text += token.ToString();
            if (settings.TokensOutputPath != "") File.WriteAllText(settings.TokensOutputPath , text);
            if (!succeded) exit("One or more errors occurred.", true, 1, true);
        } catch (Exception e) { error(e); throw; }
    }
    private void parse(StatusContext ctx) {
        try {
            VersyParser parser = new VersyParser(tokens);
            statement = parser.parse();
            if (parser.errors.Count > 0) {
                foreach (Error error in parser.errors) error.print();
                exit("One or more errors occurred.", true, 1, true);
            }
            if (settings.ASTOutputPath == "") return;
            using var writer = new StringWriter();
            var text = statement.DumpText(
                label: "Versy AST Visualizer",
                members: new MembersConfig { IncludeNonPublicMembers = true},
                typeNames: new TypeNamingConfig { ShowTypeNames      = true, UseFullName   = true, UseAliases = false},
                tableConfig: new TableConfig { ShowRowSeparators     = true }
            );
            if (settings.ASTOutputPath != "") File.WriteAllText(settings.ASTOutputPath , text);
        } catch (Exception e) { error(e); throw; }
    }
    private void codegen(StatusContext ctx) {
        try {
            CSharpTranslator csTranslator = new CSharpTranslator(statement);
            generatedCode = csTranslator.translate();
            if (settings.CSOutputPath == "") return;
            using var writer = new StringWriter();
            if (settings.CSOutputPath != "") File.WriteAllText(settings.CSOutputPath , generatedCode);
        } catch (Exception e) { error(e); throw; }
    }
    private void compile(StatusContext ctx) {
        try {
            string sourceFileDir = Path.GetDirectoryName(Path.GetFullPath(settings.InputFile)) ?? "";
            
            string finalExeName = settings.OutputPath.EndsWith(".exe") ? settings.OutputPath : settings.OutputPath + ".exe";
            string absoluteOutputPath = Path.Combine(sourceFileDir, finalExeName);
            
            string baseFolder = string.IsNullOrWhiteSpace(settings.CSOutputPath) 
                                    ? Path.Combine(Path.GetTempPath(), "VersyCompilerTemp") 
                                    : Path.GetDirectoryName(Path.GetFullPath(settings.CSOutputPath));
        
            string workingFolder = Path.Combine(baseFolder, "BuildProject");
            Directory.CreateDirectory(workingFolder);
        
            string csout = Path.Combine(workingFolder, "Program"); // Il .cs viene aggiunto dal costruttore del compiler
            string csprojout = Path.Combine(workingFolder, settings.OutputPath + ".csproj");
            
            CSharpCompiler compiler = new CSharpCompiler(generatedCode, absoluteOutputPath, csout, csprojout, settings.singlefile);
            bool succeded = compiler.compile();
            settings.OutputPath = absoluteOutputPath;

            if (!succeded) exit("One or more errors occurred.", true, 1, true);
        } catch (Exception e) { error(e); throw; }
    }
    private void run(StatusContext ctx) {
        try {
            Process process = new Process();
            process.StartInfo.FileName               = settings.OutputPath;
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError  = true;
            process.StartInfo.CreateNoWindow         = true;
            
            process.OutputDataReceived += (sender, e) => {
                                              if (!string.IsNullOrEmpty(e.Data)) AnsiConsole.MarkupLine($"[blue bold]OUTPUT[/]: {e.Data}");
                                          };
            
            process.ErrorDataReceived += (sender, e) => {
                                             if (!string.IsNullOrEmpty(e.Data)) AnsiConsole.MarkupLine($"[red bold]ERROR:[/] {e.Data}");
                                         };

            process.Start();
            
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        } catch (Exception e) { AnsiConsole.WriteException(e); throw; }
    }

    private void printStatus(string statusText, Action<StatusContext> action, string? success = null, Spinner? spinner = null, Style? style = null) {
        if (spinner == null) spinner = Spinner.Known.Default;
        if (style == null)   style   = Style.Parse("yellow bold");
        
        AnsiConsole.Status()
            .Spinner(spinner)
            .SpinnerStyle(style)
            .Start(statusText, ctx => { action.Invoke(ctx); });
        
        if (success != null) AnsiConsole.MarkupLine(success);
    }
    private void printPath(string header, string path) {
        var text = new TextPath(path)
            .RootStyle(new Style(Color.Blue, decoration: Decoration.Bold))
            .SeparatorStyle(new Style(Color.Grey))
            .StemStyle(new Style(Color.White))
            .LeafStyle(new Style(Color.Green, decoration: Decoration.Underline));
        var panel = new Panel(text)
            .Header(header)
            .HeavyBorder()
            .BorderColor(Color.Grey)
            .RoundedBorder();
        AnsiConsole.Write(panel);
    }
    private void error(Exception exception, ExceptionFormats format = ExceptionFormats.Default) {
        AnsiConsole.WriteException(exception, format);
    }
    private int exit(string? message = null, bool error = false, int exitCode = 0, bool force = false) {
        if (message == null)
            message = error
                          ? $"[red]:cross_mark:  An error occurred, could not continue the compilation for {new FileInfo(settings.InputFile).Name}.[/]"
                          : $"[lime]:check_mark:  Successfully compiled {new FileInfo(settings.InputFile).Name}.[/]";
        else message = $"[red]:cross_mark:  {message}[/]";
        AnsiConsole.MarkupLine(message);
        if (force) Environment.Exit(exitCode);
        return exitCode;
    }
}