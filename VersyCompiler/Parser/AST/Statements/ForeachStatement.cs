namespace Versy.AST.Statements;

public class ForeachStatement : Statement {
    public Expression left     { get; set;}
    public Expression iterable { get; set;}
    public Statement  body      { get; set;}

    public ForeachStatement(Expression left, Expression iterable, Statement body) {
        this.left     = left;
        this.iterable = iterable;
        this.body     = body;
    }

    public void statement() { }
}
