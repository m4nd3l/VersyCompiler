using System.Text;
using Versy.Errors;

namespace Versy.Lexer;

public class VersyLexer {
    public  List<Token> tokens          { get; set; }
    private List<Error> errors          { get; set; }
    private string      sourceCode      { get; set; }
    private int         currentPosition { get; set; }

    public VersyLexer(string sourceCode) {
        this.sourceCode = sourceCode;
        tokens          = new List<Token>();
        errors          = new List<Error>();
        currentPosition = 0;
    }
    
    public bool tokenize() {
        Token token;
        do {
            token = getNextToken();
            tokens.Add(token);
        } while (!token.isEOF());
        if (errors.Count <= 0) return true;
        foreach (Error error in errors) error.print();
        return false;
    }

    private Token getNextToken() {
        // END OF FILE
        if (currentPosition >= sourceCode.Length) return new Token(Tokens.END_OF_FILE, "\0", null, currentPosition);
        #region DATA TYPES
        // NUMBER
        if (Char.IsDigit(getCurrentChar())) {
            int startPosition = currentPosition;
            
            while (Char.IsDigit(getCurrentChar())) advance();

            if (getCurrentChar() == '.' && Char.IsDigit(getNextChar())) {
                advance();
                while (Char.IsDigit(getCurrentChar())) advance();
            }

            int length = currentPosition - startPosition;
            string numberStr = sourceCode.Substring(startPosition, length);
            double value = double.Parse(numberStr, System.Globalization.CultureInfo.InvariantCulture);
            return new Token(Tokens.NUMBER, numberStr, value, startPosition);
        }
        // WHITESPACE
        if (Char.IsWhiteSpace(getCurrentChar())) {
            int startPosition = currentPosition;
            while (Char.IsWhiteSpace(getCurrentChar())) advance();
            int length = currentPosition - startPosition;
            string spaces = sourceCode.Substring(startPosition, length);
            return new Token(Tokens.WHITEPSPACE, spaces, null, startPosition);
        }
        // STRING
        if (getCurrentChar() == '"') {
            int startPosition = currentPosition;
            advance(); // Skips first "
            StringBuilder sb = new StringBuilder();
            bool closed = false;
            while (currentPosition < sourceCode.Length) {
                char c = getCurrentChar();
                if (c == '\\') {
                    advance(); // Skips \
                    char escapeChar = getCurrentChar();
                    switch (escapeChar) { // Decode char after \
                        case 'n':  sb.Append('\n'); break;
                        case 't':  sb.Append('\t'); break;
                        case 'r':  sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"':  sb.Append('"'); break;
                        case '0':  sb.Append('\0'); break;
                        default: 
                            sb.Append('\\').Append(escapeChar); 
                            break;
                    }
                    advance(); // Skip decoded char
                    continue;
                }
                if (c == '"') { advance(); closed = true; break; } // Skip last "
                sb.Append(c); // Adds char
                advance();
            }

            string code = sourceCode.Substring(startPosition, currentPosition - startPosition);
            if (!closed) errors.Add(new Error("Missing '\"' at the end of the string starting at", currentPosition));
            string value = sb.ToString();

            return new Token(Tokens.STRING, code, value, startPosition);
        }
        #endregion
        #region COMPARISON
        // ASSIGNMENT & EQUALS
        if (getCurrentChar() == '=') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.EQUALS, "==", null, startPosition); }
            if (getCurrentChar() == '>') { advance(); return new Token(Tokens.RARROW, "=>", null, startPosition); }
            return new Token(Tokens.ASSIGNMENT, "=", null, startPosition);
        }
        // NOT & NOT EQUALS
        if (getCurrentChar() == '!') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.NOT_EQUALS, "!=", null, startPosition); }
            return new Token(Tokens.NOT, "!", null, startPosition);
        }
        // GREATER & GREATER EQUALS
        if (getCurrentChar() == '>') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.GREATER_EQUALS, ">=", null, startPosition); }
            return new Token(Tokens.GREATER, ">", null, startPosition);
        }
        // NOT & NOT EQUALS
        if (getCurrentChar() == '<') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.LESS_EQUALS, "<=", null, startPosition); }
            return new Token(Tokens.LESS, "<", null, startPosition);
        }
        #endregion
        #region OPERATORS
        // PLUS, PLUS PLUS & PLUS EQUALS
        if (getCurrentChar() == '+') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '+') { advance(); return new Token(Tokens.PLUS_PLUS, "++", null, startPosition); }
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.PLUS_EQUALS, "+=", null, startPosition); }
            return new Token(Tokens.PLUS, "+", null, startPosition);
        }
        // MINUS, MINUS MINUS & MINUS EQUALS
        if (getCurrentChar() == '-') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '-') { advance(); return new Token(Tokens.MINUS_MINUS, "--", null, startPosition); }
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.MINUS_EQUALS, "-=", null, startPosition); }
            return new Token(Tokens.MINUS, "-", null, startPosition);
        }
        // STAR & STAR EQUALS
        if (getCurrentChar() == '*') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.STAR_EQUALS, "*=", null, startPosition); }
            return new Token(Tokens.STAR, "*", null, startPosition);
        }
        // SLASH, SLAHS EQUALS & COMMENTS
        if (getCurrentChar() == '/') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '/') {
                advance();
                while (getCurrentChar() != '\n' && getCurrentChar() != '\r' && getCurrentChar() != '\0') advance();
                string commentText = sourceCode.Substring(startPosition, currentPosition - startPosition);
                return new Token(Tokens.COMMENT, commentText, null, startPosition);
            }
            if (getCurrentChar() == '*') {
                advance();
                while (currentPosition < sourceCode.Length) {
                    if (getCurrentChar() == '*' && getNextChar() == '/') {
                        advance();
                        advance();
                        break;
                    }
                    advance();
                }
                string commentText = sourceCode.Substring(startPosition, currentPosition - startPosition);
                return new Token(Tokens.MULTILINE_COMMENT, commentText, null, startPosition);
            }
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.SLASH_EQUALS, "/=", null, startPosition); }
            return new Token(Tokens.SLASH, "/", null, startPosition);
        }
        // PERCENT & PERCENT EQUALS
        if (getCurrentChar() == '%') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return new Token(Tokens.PERCENT_EQUALS, "%=", null, startPosition); }
            return new Token(Tokens.PERCENT, "%", null, startPosition);
        }
        // POWER
        if (getCurrentChar() == '^') {
            advance();
            return new Token(Tokens.POWER, "^", null, currentPosition - 1);
        }
        #endregion
        // DOLLAR
        if (getCurrentChar() == '$') {
            advance();
            return new Token(Tokens.DOLLAR, "$", null, currentPosition - 1);
        }
        // IDENTIFIER
        if (Char.IsLetter(getCurrentChar()) || getCurrentChar() == '_') {
            int startPosition = currentPosition;
            
            while (Char.IsLetterOrDigit(getCurrentChar()) || getCurrentChar() == '_') advance();
    
            int length = currentPosition - startPosition;
            string name = sourceCode.Substring(startPosition, length);

            if (Keywords.keywords.TryGetValue(name, out Tokens type)) return new Token(type, name, null, startPosition);
            if (name == "true") return new Token(Tokens.TRUE, "true", true, startPosition);
            if (name == "false") return new Token(Tokens.FALSE, "false", false, startPosition);
            
            return new Token(Tokens.IDENTIFIER, name, name, startPosition);
        }
        // ||
        if (getCurrentChar() == '|') {
            if (getNextChar() == '|') {
                advance(); 
                advance();
                return new Token(Tokens.OR, "||", null, currentPosition - 2);
            }
        }
        // &&
        if (getCurrentChar() == '&') {
            if (getNextChar() == '&') {
                advance();
                advance();
                return new Token(Tokens.AND, "&&", null, currentPosition - 2);
            }
        }
        // SYMBOLS
        Token? operatorToken = getCurrentChar() switch {
            '(' => new Token(Tokens.LPAREN, "(", null, currentPosition),
            ')' => new Token(Tokens.RPAREN, ")", null, currentPosition),
            '[' => new Token(Tokens.LSBRACKET, "[", null, currentPosition),
            ']' => new Token(Tokens.RSBRACKET, "]", null, currentPosition),
            '{' => new Token(Tokens.LBRACKET, "{", null, currentPosition),
            '}' => new Token(Tokens.RBRACKET, "}", null, currentPosition),
            '>' => new Token(Tokens.GREATER, ">", null, currentPosition),
            '<' => new Token(Tokens.LESS, "<", null, currentPosition),
            '.' => new Token(Tokens.DOT, ".", null, currentPosition),
            ',' => new Token(Tokens.COMMA, ",", null, currentPosition),
            ':' => new Token(Tokens.COLON, ":", null, currentPosition),
            ';' => new Token(Tokens.SEMICOLON, ";", null, currentPosition),
            '?' => new Token(Tokens.QUESTION_MARK, "?", null, currentPosition),
              _ => null,
        };
        
        if (operatorToken != null) { advance(); return operatorToken; }
        // UNKNOWN
        char unknownChar = getCurrentChar();
        int errorPos = currentPosition;
        advance();
        errors.Add(new Error($"Token not recognized: {getCurrentChar().ToString()} at", errorPos));
        return new Token(Tokens.UNKNOWN, unknownChar.ToString(), null, errorPos);
    }
    
    private char getCurrentChar() {
        if (currentPosition >= sourceCode.Length) return '\0';
        return sourceCode[currentPosition];
    }
    private char getNextChar() {
        if (currentPosition + 1 >= sourceCode.Length) return '\0';
        return sourceCode[currentPosition + 1];
    }
    private void advance() { currentPosition++; }
}