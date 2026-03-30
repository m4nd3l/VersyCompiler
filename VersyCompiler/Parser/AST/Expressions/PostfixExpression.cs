using Versy.Lexer;

namespace Versy.AST.Expressions;

public class PostfixExpression : Expression {
    public Token      Operator { get; set; }
    public Expression left     { get; set; }

    public PostfixExpression(Token @operator, Expression left) {
        Operator   = @operator;
        this.left = left;
    }

    public void expression() {
    }
}
