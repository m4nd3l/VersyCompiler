namespace Versy.AST.Statements;

public class WhileStatement : Statement {
    public Expression condition { get; set; }
    public Statement  body      { get; set; }

    public WhileStatement(Expression condition, Statement body) {
        this.condition = condition;
        this.body      = body;
    }

    public void statement() { }
}
