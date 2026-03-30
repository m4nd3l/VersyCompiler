using Versy.Lexer;

namespace Versy.Errors;

public class UnrecognizedTokenError : Error {
    public Token unrecognizedToken { get; set; }
    
    public UnrecognizedTokenError(Token unrecognizedToken, string code, int line, int position, int endPosition) : 
        base("Lexing Error","Unrecognized Token Error", $"Unrecognized Token at line {line}, position {position}.\nUnrecognized: {code}", 
             code, line, position, endPosition) {
        this.unrecognizedToken = unrecognizedToken;
    }
}
