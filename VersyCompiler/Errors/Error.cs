using Spectre.Console;

namespace Versy.Errors;

public abstract class Error {
    public string category     { get; set; }
    public string errorName    { get; set; }
    public string errorMessage { get; set; }
    public string code         { get; set; }
    public int    line         { get; set; }
    public int    position     { get; set; }
    public int    endPosition  { get; set; }

    public Error(string category, string errorName, string errorMessage, string code, int line, int position, int endPosition) {
        this.category     = category;
        this.errorName    = errorName;
        this.errorMessage = errorMessage;
        this.code         = code;
        this.line         = line;
        this.position     = position;
        this.endPosition  = endPosition;
    }

    public override string ToString() => $"{category} -> {errorName}:\n{errorMessage}";
}
