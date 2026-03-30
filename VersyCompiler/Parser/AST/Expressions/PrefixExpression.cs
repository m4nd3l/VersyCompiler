using Versy.Lexer;

namespace Versy.AST.Expressions;

public class PrefixExpression : Expression {
    public Token      Operator { get; set; }
    public Expression right    { get; set; }

    public PrefixExpression(Token @operator, Expression right) {
        Operator   = @operator;
        this.right = right;
    }

    public void expression() {
    }
}
