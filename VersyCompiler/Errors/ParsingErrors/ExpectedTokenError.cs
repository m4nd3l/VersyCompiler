using Versy.Lexer;

namespace Versy.Errors;

public class ExpectedTokenError : Error {
    public Tokens expected { get; set; }
    public Tokens received { get; set; }
    
    public ExpectedTokenError(Tokens expected, Tokens received, string code, int line, int position, int endPosition) : 
        base("Parsing Error", "Expected Token Error", $"Expected Token {expected} but received {received}," +
                                     $" at line {line}, position {position}.", code, line, position, endPosition) {
        this.expected = expected;
        this.received = received;
    }
}
