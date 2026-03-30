using System.Text;
using Versy.Errors;

namespace Versy.Lexer;

public class VersyLexer {
    public  List<Token> tokens              { get; set; }
    private List<Error> errors              { get; set; }
    private string      sourceCode          { get; set; }
    private int         currentPosition     { get; set; }
    private int         currentLine         { get; set; }
    private int         currentLinePosition { get; set; }

    public VersyLexer(string sourceCode) {
        this.sourceCode = sourceCode;
        tokens          = new List<Token>();
        errors          = new List<Error>();
        currentPosition = 0;
        currentLine = 0;
        currentLinePosition = 0;
    }
    
    public bool tokenize() {
        Token token;
        do {
            token = getNextToken();
            tokens.Add(token);
        } while (!token.isEOF());
        if (errors.Count <= 0) return true;
        return false;
    }

    private Token getNextToken() {
        // END OF FILE
        if (currentPosition >= sourceCode.Length) return createToken(Tokens.END_OF_FILE, "\0", null);
        if (getCurrentChar() == '\n') {
            advance();
            currentLine++;
            currentLinePosition = 0;
            return getNextToken();
        }
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
            return createTokenaa(Tokens.NUMBER, numberStr, value, convertToLinePos(startPosition));
        }
        // WHITESPACE
        if (Char.IsWhiteSpace(getCurrentChar())) {
            int startPosition = currentPosition;
            while (Char.IsWhiteSpace(getCurrentChar())) advance();
            int length = currentPosition - startPosition;
            string spaces = sourceCode.Substring(startPosition, length);
            return createTokenaa(Tokens.WHITEPSPACE, spaces, null, convertToLinePos(startPosition));
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
            if (!closed) errors.Add(new MissingQuotationMarkError(code, currentLine, convertToLinePos(startPosition), 
                                                             convertToLinePos(currentPosition)));
            string value = sb.ToString();

            return createTokenaa(Tokens.STRING, code, value, convertToLinePos(startPosition));
        }
        #endregion
        #region COMPARISON
        // ASSIGNMENT & EQUALS
        if (getCurrentChar() == '=') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.EQUALS, "==", null, convertToLinePos(startPosition)); }
            if (getCurrentChar() == '>') { advance(); return createTokenaa(Tokens.RARROW, "=>", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.ASSIGNMENT, "=", null, convertToLinePos(startPosition));
        }
        // NOT & NOT EQUALS
        if (getCurrentChar() == '!') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.NOT_EQUALS, "!=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.NOT, "!", null, convertToLinePos(startPosition));
        }
        // GREATER & GREATER EQUALS
        if (getCurrentChar() == '>') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.GREATER_EQUALS, ">=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.GREATER, ">", null, convertToLinePos(startPosition));
        }
        // NOT & NOT EQUALS
        if (getCurrentChar() == '<') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.LESS_EQUALS, "<=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.LESS, "<", null, convertToLinePos(startPosition));
        }
        #endregion
        #region OPERATORS
        // PLUS, PLUS PLUS & PLUS EQUALS
        if (getCurrentChar() == '+') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '+') { advance(); return createTokenaa(Tokens.PLUS_PLUS, "++", null, currentLinePosition - 2); }
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.PLUS_EQUALS, "+=", null, currentLinePosition - 2); }
            return createTokenaa(Tokens.PLUS, "+", null);
        }
        // MINUS, MINUS MINUS & MINUS EQUALS
        if (getCurrentChar() == '-') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '-') { advance(); return createTokenaa(Tokens.MINUS_MINUS, "--", null, currentLinePosition - 2); }
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.MINUS_EQUALS, "-=", null, currentLinePosition - 2); }
            return createTokenaa(Tokens.MINUS, "-", null);
        }
        // STAR & STAR EQUALS
        if (getCurrentChar() == '*') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.STAR_EQUALS, "*=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.STAR, "*", null);
        }
        // SLASH, SLAHS EQUALS & COMMENTS
        if (getCurrentChar() == '/') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '/') {
                advance();
                while (getCurrentChar() != '\n' && getCurrentChar() != '\r' && getCurrentChar() != '\0') advance();
                string commentText = sourceCode.Substring(startPosition, currentPosition - startPosition);
                return createTokenaa(Tokens.COMMENT, commentText, null, convertToLinePos(startPosition));
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
                return createTokenaa(Tokens.MULTILINE_COMMENT, commentText, null, startPosition);
            }
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.SLASH_EQUALS, "/=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.SLASH, "/", null, convertToLinePos(startPosition));
        }
        // PERCENT & PERCENT EQUALS
        if (getCurrentChar() == '%') {
            int startPosition = currentPosition;
            advance();
            if (getCurrentChar() == '=') { advance(); return createTokenaa(Tokens.PERCENT_EQUALS, "%=", null, convertToLinePos(startPosition)); }
            return createTokenaa(Tokens.PERCENT, "%", null, convertToLinePos(startPosition));
        }
        // POWER
        if (getCurrentChar() == '^') {
            advance();
            return createTokenaa(Tokens.POWER, "^", null);
        }
        #endregion
        // DOLLAR
        if (getCurrentChar() == '$') {
            advance();
            return createTokenaa(Tokens.DOLLAR, "$", null);
        }
        // IDENTIFIER
        if (Char.IsLetter(getCurrentChar()) || getCurrentChar() == '_') {
            int startPosition = currentPosition;
            
            while (Char.IsLetterOrDigit(getCurrentChar()) || getCurrentChar() == '_') advance();
    
            int length = currentPosition - startPosition;
            string name = sourceCode.Substring(startPosition, length);

            if (Keywords.keywords.TryGetValue(name, out Tokens type)) return createTokenaa(type, name, null, convertToLinePos(startPosition));
            if (name == "true") return createTokenaa(Tokens.TRUE, "true", true, convertToLinePos(startPosition));
            if (name == "false") return createTokenaa(Tokens.FALSE, "false", false, convertToLinePos(startPosition));
            
            return createTokenaa(Tokens.IDENTIFIER, name, name, startPosition);
        }
        // ||
        if (getCurrentChar() == '|') {
            if (getNextChar() == '|') {
                advance(); 
                advance();
                return createTokenaa(Tokens.OR, "||", null, currentLinePosition - 2);
            }
        }
        // &&
        if (getCurrentChar() == '&') {
            if (getNextChar() == '&') {
                advance();
                advance();
                return createTokenaa(Tokens.AND, "&&", null, currentLinePosition - 2);
            }
        }
        // SYMBOLS
        Token? operatorToken = getCurrentChar() switch {
            '(' => createToken(Tokens.LPAREN, "(", null),
            ')' => createToken(Tokens.RPAREN, ")", null),
            '[' => createToken(Tokens.LSBRACKET, "[", null),
            ']' => createToken(Tokens.RSBRACKET, "]", null),
            '{' => createToken(Tokens.LBRACKET, "{", null),
            '}' => createToken(Tokens.RBRACKET, "}", null),
            '>' => createToken(Tokens.GREATER, ">", null),
            '<' => createToken(Tokens.LESS, "<", null),
            '.' => createToken(Tokens.DOT, ".", null),
            ',' => createToken(Tokens.COMMA, ",", null),
            ':' => createToken(Tokens.COLON, ":", null),
            ';' => createToken(Tokens.SEMICOLON, ";", null),
            '?' => createToken(Tokens.QUESTION_MARK, "?", null),
              _ => null,
        };
        
        if (operatorToken != null) { advance(); return operatorToken; }
        // UNKNOWN
        char unknownChar = getCurrentChar();
        int errorPos = currentPosition;
        advance();
        var token =  createToken(Tokens.UNKNOWN, unknownChar.ToString(), null, errorPos);
        errors.Add(new UnrecognizedTokenError(token, unknownChar.ToString(), currentLine, currentLinePosition - 1, currentLinePosition));
        return token;
    }
    
    private char getCurrentChar() {
        if (currentPosition >= sourceCode.Length) return '\0';
        return sourceCode[currentPosition];
    }
    private char getNextChar() {
        if (currentPosition + 1 >= sourceCode.Length) return '\0';
        return sourceCode[currentPosition + 1];
    }
    private void advance() { currentPosition++; currentLinePosition++; }
    private Token createToken(Tokens type, string reference, object? value, int startPosition = -1, int endPosition = -1) {
        if (startPosition < 0) startPosition = currentLinePosition;
        if (endPosition < 0) endPosition = currentLinePosition;
        return new Token(type, reference, value, currentLine, startPosition, endPosition);
    } 
    
    private Token createTokenaa(Tokens type, string reference, object? value, int startPosition = -1, int endPosition = -1) {
        if (startPosition < 0) startPosition = currentLinePosition - 1;
        if (endPosition < 0) endPosition     = currentLinePosition - 1;
        return new Token(type, reference, value, currentLine, startPosition, endPosition);
    }

    private int convertToLinePos(int absolutePosition) {
        if (absolutePosition <= 0) return 0;
        int lastNewLineIndex = sourceCode.LastIndexOf('\n', absolutePosition - 1);
        return lastNewLineIndex == -1 ? absolutePosition : absolutePosition - lastNewLineIndex - 1;
    }
}