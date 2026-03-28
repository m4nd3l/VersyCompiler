namespace Versy.AST.Statements;

public class VariableDeclarationStatement : Statement{
    public string      identifier    { get; set; }
    public bool        isConstant    { get; set; }
    public bool        initialized   { get; set; }
    public Expression? assignedValue { get; set; }
    public Type        type          { get; set; }

    public VariableDeclarationStatement(string identifier, bool isConstant, Expression? assignedValue, Type type) {
        this.identifier    = identifier;
        this.isConstant    = isConstant;
        this.assignedValue = assignedValue;
        this.type          = type;
        initialized        = assignedValue != null;
    }

    public void statement() {
        
    }
}
