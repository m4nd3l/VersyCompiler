namespace Versy.Lexer;

public enum Tokens {
    
    // BASIC SIGNS
    
    END_OF_FILE = 1,            // Last position of the source code
    
    LPAREN,                     // (
    RPAREN,                     // )
    
    LBRACKET,                   // {
    RBRACKET,                   // }
    
    LSBRACKET,                  // [
    RSBRACKET,                  // ]
    
    WHITEPSPACE,                //  
    COMMA,                      // ,
    COLON,                      // :
    SEMICOLON,                  // ;
    DOT,                        // .
    QUESTION_MARK,              // ?
    
    PLUS,                       // +
    PLUS_PLUS,                  // ++
    PLUS_EQUALS,                // +=
    MINUS,                      // -
    MINUS_MINUS,                // --
    MINUS_EQUALS,               // -=
    STAR,                       // *
    STAR_EQUALS,                // *=
    SLASH,                      // /
    SLASH_EQUALS,               // /=
    POWER,                      // ^
    PERCENT,                    // %
    PERCENT_EQUALS,             // %=
    
    ASSIGNMENT,                 // =
    IDENTIFIER,                 // Ex. Variable name, function name etc...
    NUMBER,                     // 1
    STRING,                     // "Hello\nWorld!"
    NULL,                       // null
    TRUE,                       // true
    FALSE,                      // false
    
    RARROW,                     // =>
    
    
    EQUALS,                     // ==     |
    NOT_EQUALS,                 // !=     | Used just for conditionals
    NOT,                        // !      |
    GREATER,                    // >
    GREATER_EQUALS,             // >=
    LESS,                       // <
    LESS_EQUALS,                // <=
    
    // KEYWORDS
    
    CONST,                      // const
    VAR,                        // var
    IF,                         // if
    ELSE,                       // else
    ELIF,                       // elif
    AND,                        // &&
    OR,                         // ||
    WHILE,                      // while
    FOR,                        // for
    FOREACH,                    // foreach
    FUNC,                       // func
    TYPEOF,                     // typeof
    INSTANCEOF,                 // instanceof
    RETURN,                     // return
    CLASS,                      // class
    
    // OTHER
    
    COMMENT,
    MULTILINE_COMMENT,
    UNKNOWN,
}