namespace Versy.AST.Expressions;

public class ArrayAssignmentExpression : Expression {
    public List<Expression> value   { get; set; }

    public ArrayAssignmentExpression(List<Expression> value) {
        this.value     = value;
    }

    public void expression() {
        
    }
}
