namespace Versy.AST.Expressions;

public class ForeachLeftExpression : Expression {
    public string identifier { get; set; }

    public ForeachLeftExpression(string identifier) {
        this.identifier = identifier;
    }

    public void expression() { }
}
