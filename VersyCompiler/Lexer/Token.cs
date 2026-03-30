using Spectre.Console;

namespace Versy.Lexer;

public class Token {
    public Tokens type          { get; set; }
    public string codeReference { get; set; }
    public object value         { get; set; }
    public int    line          { get; set; }
    public int    position      { get; set; }
    public int    endPosition   { get; set; }

    public Token(Tokens type, string codeReference, object value, int line, int position, int endPosition) {
        this.type          = type;
        this.codeReference = codeReference;
        this.value         = value;
        this.line          = line;
        this.position      = position;
        this.endPosition   = endPosition;
    }
    public bool isEOF() {
        return type == Tokens.END_OF_FILE;
    }

    public void print() {
        if (type == Tokens.UNKNOWN) AnsiConsole.MarkupLine($"[DarkOrange3_1 bold]:warning:  {ToString()}[/]");
        else AnsiConsole.MarkupLine($"[yellow bold]{ToString()}[/]");
    }
    
    public override string ToString() {
        return $"{type} ({value}) - Pos: {position}\n";
    }
}
