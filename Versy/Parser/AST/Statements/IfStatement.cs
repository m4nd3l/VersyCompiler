namespace Versy.AST.Statements;

public class IfStatement : Statement {
    public Expression condition { get; set; }
    public Statement  body      { get; set; }
    public Statement? elses     { get; set; }

    public IfStatement(Expression condition, Statement body, Statement? elses) {
        this.condition = condition;
        this.body      = body;
        this.elses     = elses;
    }

    public void statement() {
        
    }
}
