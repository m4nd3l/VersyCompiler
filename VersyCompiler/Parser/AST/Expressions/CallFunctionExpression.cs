namespace Versy.AST.Expressions;

public class CallFunctionExpression : Expression {
    public string           identifier { get; set; }
    public List<Expression> arguments  { get; set; }
    public bool             isBuiltin  { get; set; }

    public CallFunctionExpression(string identifier, List<Expression> arguments, bool isBuiltin) {
        this.identifier = identifier;
        this.arguments = arguments;
        this.isBuiltin = isBuiltin;
    }

    public void expression() { }
}
