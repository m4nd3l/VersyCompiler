using Dumpify;

using Spectre.Console;
using Spectre.Console.Cli;

using System.Diagnostics;

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
    private List<Error>    errors        { get; set; } = new();
    private List<Token>    tokens        { get; set; }
    private BlockStatement statement     { get; set; }
    private string         generatedCode { get; set; }
    
    public override int Execute(CommandContext context, Options settings, CancellationToken cancellationToken) {
        this.settings                 = settings;
        settings.OutputPath           = Path.GetFileNameWithoutExtension(settings.OutputPath);
        
        printPath("[yellow]Source code[/]", settings.InputFile);
        
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
            foreach (var error in lexer.errors) errors.Add(error);
            if (!succeded) exit("One or more errors occurred.", true, 1, true);
        } catch (Exception e) { error(e); throw; }
    }
    private void parse(StatusContext ctx) {
        try {
            VersyParser parser = new VersyParser(tokens);
            statement = parser.parse();
            if (parser.errors.Count > 0) // TODO : MANAGE ERRORS TO SINGLE FILE
                exit("One or more errors occurred.", true, 1, true);
            foreach (var error in parser.errors) errors.Add(error);
        } catch (Exception e) { error(e); throw; }
    }
    private void codegen(StatusContext ctx) {
        try {
            CSharpTranslator csTranslator = new CSharpTranslator(statement);
            generatedCode = csTranslator.translate();
        } catch (Exception e) { error(e); throw; }
    }
    private void compile(StatusContext ctx) {
        try {
            string sourceFileDir = Path.GetDirectoryName(Path.GetFullPath(settings.InputFile)) ?? "";
            
            string finalExeName = settings.OutputPath.EndsWith(".exe") ? settings.OutputPath : settings.OutputPath + ".exe";
            string absoluteOutputPath = Path.Combine(sourceFileDir, finalExeName);
            
            string baseFolder = string.IsNullOrWhiteSpace(settings.TranslatedOutputPath) 
                                    ? Path.Combine(Path.GetTempPath(), "VersyCompilerTemp") 
                                    : Path.GetDirectoryName(Path.GetFullPath(settings.TranslatedOutputPath));
        
            string workingFolder = Path.Combine(baseFolder, "BuildProject");
            Directory.CreateDirectory(workingFolder);
        
            string csout = Path.Combine(workingFolder, "Program");
            string csprojout = Path.Combine(workingFolder, settings.OutputPath + ".csproj");
            
            CSharpCompiler compiler = new CSharpCompiler(generatedCode, absoluteOutputPath, csout, csprojout, settings.SingleFile);
            bool succeded = compiler.compile();
            settings.OutputPath = absoluteOutputPath;

            if (!settings.DebugFiles) {
                if (!succeded) exit("One or more errors occurred.", true, 1, true);
                return;
            }
            
            string debugInfoFolder = Path.Combine(compiler.getOutputFolderPath(), "debug-info");
            Directory.CreateDirectory(debugInfoFolder);
            
            settings.TokensOutputPath     = Path.Join(debugInfoFolder, "tokens.txt");
            settings.ASTOutputPath        = Path.Join(debugInfoFolder, "ast.txt");
            settings.TranslatedOutputPath = Path.Join(debugInfoFolder, "newcode.txt");
            
            using var writer = new StringWriter();
            
            string text1 = "";
            foreach (Token token in tokens) text1 += token.ToString();
            if (settings.TokensOutputPath != "") File.WriteAllText(settings.TokensOutputPath , text1);
            
            var text = statement.DumpText(
                label: "Versy AST Visualizer",
                members: new MembersConfig { IncludeNonPublicMembers = true},
                typeNames: new TypeNamingConfig { ShowTypeNames      = true, UseFullName   = true, UseAliases = false},
                tableConfig: new TableConfig { ShowRowSeparators     = true }
            );
            if (settings.ASTOutputPath != "") File.WriteAllText(settings.ASTOutputPath , text);
            
            if (settings.TranslatedOutputPath != "") File.WriteAllText(settings.TranslatedOutputPath , generatedCode);
            string errorst = "";
            foreach (var error in errors) errorst += error + Environment.NewLine;
            if (errorst != "") File.WriteAllText(settings.ErrorsOutputPath, errorst);
            
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

    #region HELPER
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
    #endregion
}