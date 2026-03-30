using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Versy.AST.Statements;
using Versy.Lexer;
using Versy.Parser;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace Versy.LSP;

public class DocumentSession {
    public string           code        { get; set; } = string.Empty;
    public BlockStatement?  tree        { get; set; }
    //public SymbolTable?     symbols     { get; set; }
    public List<Diagnostic> diagnostics { get; set; } = new();
    
    public void update(string newCode) {
        code = newCode;
        
        var lexer = new VersyLexer(newCode);
        lexer.tokenize();
        var tokens = lexer.tokens;

        var parser = new VersyParser(tokens);
        tree = parser.parse();
        
        diagnostics = parser.errors.Select(error => new Diagnostic {
            Message = error.errorMessage,
            Range = new Range(
                new Position(error.line - 1, error.position), 
                new Position(error.line - 1, error.endPosition)
            ),
            Severity = DiagnosticSeverity.Error
        }).ToList();
    }
}
