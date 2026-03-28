using Versy.Lexer;

namespace Versy.AST.Expressions;

public class AssignmentExpression : Expression {
    public Expression assigne      { get; set; }
    public Token      operation { get; set; }
    public Expression right     { get; set; }

    public AssignmentExpression(Expression assigne, Token operation, Expression right) {
        this.assigne   = assigne;
        this.operation = operation;
        this.right     = right;
    }

    public void expression() {
        
    }
}
