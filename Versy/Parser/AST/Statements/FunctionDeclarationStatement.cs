namespace Versy.AST.Statements;

public class FunctionDeclarationStatement : Statement {
    public string          identifier { get; set; }
    public List<Statement> args       { get; set; }
    public Type            returnType { get; set; }
    public Statement       body       { get; set; }

    public FunctionDeclarationStatement(string identifier, List<Statement> args, Type returnType, Statement body) {
        this.identifier = identifier;
        this.args       = args;
        this.returnType = returnType;
        this.body       = body;
    }

    public void statement() { }
}
