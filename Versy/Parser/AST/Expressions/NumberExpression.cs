namespace Versy.AST.Expressions;

public class NumberExpression : Expression {
    public double value { get; set; }

    public NumberExpression(double value) {
        this.value = value;
    }

    public void expression() {
        
    }
}
