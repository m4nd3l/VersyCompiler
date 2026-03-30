namespace Versy.AST.Statements;

public class FunctionArgumentStatement : Statement {
    public Type   type       { get; set; }
    public string identifier { get; set; }

    public FunctionArgumentStatement(Type type, string identifier) {
        this.type       = type;
        this.identifier = identifier;
    }

    public void statement() { }
}
