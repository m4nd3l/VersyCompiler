namespace Versy.AST.Expressions;

public class SymbolExpression : Expression {
    public string value { get; set; }

    public SymbolExpression(string value) {
        this.value = value;
    }

    public void expression() {
        
    }
}
