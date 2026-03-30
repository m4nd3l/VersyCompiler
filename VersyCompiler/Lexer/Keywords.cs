namespace Versy.Lexer;

public class Keywords {
    public static readonly Dictionary<string, Tokens> keywords = new() {
        { "const",      Tokens.CONST },
        { "var",        Tokens.VAR },
        { "null",       Tokens.NULL },
        { "new",      Tokens.NEW},
        { "&&",         Tokens.AND },
        { "||",         Tokens.OR },
        { "if",         Tokens.IF },
        { "else",       Tokens.ELSE },
        { "elif",       Tokens.ELIF },
        { "while",      Tokens.WHILE },
        { "for",        Tokens.FOR },
        { "foreach",    Tokens.FOREACH },
        { "func",       Tokens.FUNC },
        { "typeof",     Tokens.TYPEOF },
        { "instanceof", Tokens.INSTANCEOF },
        { "return",     Tokens.RETURN },
        { "class",      Tokens.CLASS }
    };
}
