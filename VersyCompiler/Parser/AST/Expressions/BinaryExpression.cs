using Versy.Lexer;

namespace Versy.AST.Expressions;

public class BinaryExpression : Expression {
    public Expression left       { get; set; }
    public Token     operation   { get; set; }
    public Expression right      { get; set; }

    public BinaryExpression(Expression left, Token operation, Expression right) {
        this.left      = left;
        this.operation = operation;
        this.right     = right;
    }

    public void expression() {
        
    }
}
