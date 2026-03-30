namespace Versy.AST.Statements;

public class ReturnStatement : Statement {
    public Expression? expression { get; set; }

    public ReturnStatement(Expression? expression) {
        this.expression = expression;
    }

    public void statement() { }
}
