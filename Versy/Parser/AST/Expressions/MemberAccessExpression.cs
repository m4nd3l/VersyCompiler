using Versy.Lexer;

namespace Versy.AST.Expressions;

public class MemberAccessExpression : Expression {
    public Expression left       { get; set; }
    public Expression right      { get; set; }

    public MemberAccessExpression(Expression left, Expression right) {
        this.left      = left;
        this.right     = right;
    }

    public void expression() {
        
    }
}
