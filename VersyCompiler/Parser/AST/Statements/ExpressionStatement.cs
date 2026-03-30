namespace Versy.AST.Statements;

public class ExpressionStatement : Statement {
    public Expression expression { get; set; }

    public ExpressionStatement(Expression expression) {
        this.expression = expression;
    }

    public void statement() { }
}
