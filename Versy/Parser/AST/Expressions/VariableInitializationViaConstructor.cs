using Versy.AST.Types;

namespace Versy.AST.Expressions;

public class VariableInitializationViaConstructor : Expression {
    public Type             type    { get; set; }
    public List<Expression> args    { get; set; }
    public bool             isArray { get; set; }

    public VariableInitializationViaConstructor(Type type, List<Expression> args) {
        this.type = type;
        this.args = args;
        isArray = type is ArrayType;
    }

    public void expression() { }
}
