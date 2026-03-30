using Versy.Lexer;

namespace Versy.Errors;

public class NUDHandlerNotFoundError : Error {
    public Tokens missingNUD { get; set; }
    
    public NUDHandlerNotFoundError(Tokens missingNUD, string code, int line, int position, int endPosition) : 
        base("Parsing Error", "NUD Handler Not Found Error", $"Unrecognized Token at line {line}, position {position}.", 
             code, line, position, endPosition) {
        this.missingNUD = missingNUD;
    }
}
