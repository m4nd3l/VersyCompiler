using Versy.Lexer;

namespace Versy.Errors;

public class MissingQuotationMarkError : Error {
    
    public MissingQuotationMarkError(string code, int line, int position, int endPosition) : 
        base("Lexing Error", "Missing Quotation Mark Error", 
             $"Missing the quotation mark to end the string starting at line {line}, position {position}.\nMissing ending '\"': {code}", 
             code, line, position, endPosition) {
    }
}
