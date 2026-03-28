using Spectre.Console;

namespace Versy.Errors;

public class Error {
    private string errorMessage { get; set; }
    private int    position     { get; set; }

    public Error(string errorMessage, int position) {
        this.errorMessage = errorMessage;
        this.position     = position;
    }

    public void print() {
        AnsiConsole.MarkupLine($"[red]{errorMessage}: {position}[/]");
    }
}
