namespace Versy.AST.Statements;

public class ForStatement : Statement {
    public Statement  initialization { get; set;}
    public Expression condition      { get; set;}
    public Expression increment      { get; set;}
    public Statement  body           { get; set;}

    public ForStatement(Statement initialization, Expression condition, Expression increment, Statement body) {
        this.initialization = initialization;
        this.condition      = condition;
        this.increment      = increment;
        this.body           = body;
    }

    public void statement() { }
}
