namespace Versy.AST.Expressions;

public class StringExpression : Expression {
    public string value { get; set; }

    public StringExpression(string value) {
        this.value = value;
    }

    public void expression() {
        
    }
}
