namespace Versy.AST.Expressions;

public class CastExpression :  Expression {
    public Type       toCastTo { get; set; }
    public Expression value    { get; set; }

    public CastExpression(Type toCastTo, Expression value) {
        this.toCastTo = toCastTo;
        this.value    = value;
    }

    public void expression() { }
}
