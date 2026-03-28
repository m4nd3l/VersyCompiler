using Spectre.Console;

namespace Versy.Lexer;

public class Token {
    public Tokens type          { get; set; }
    public string codeReference { get; set; }
    public object value         { get; set; }
    public int    position      { get; set; }

    public Token(Tokens type, string codeReference, object? value, int position) {
        this.type          = type;
        this.codeReference = codeReference;
        this.value         = value;
        this.position      = position;
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
