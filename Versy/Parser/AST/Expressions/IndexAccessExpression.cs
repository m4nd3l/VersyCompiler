namespace Versy.AST.Expressions;

public class IndexAccessExpression : Expression {
    public Expression left  { get; set; }
    public Expression index { get; set; }

    public IndexAccessExpression(Expression left, Expression index) {
        this.left  = left;
        this.index = index;
    }

    public void expression() {
        
    }
}
