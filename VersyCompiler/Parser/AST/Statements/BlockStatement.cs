namespace Versy.AST.Statements;

public class BlockStatement : Statement {
    public List<Statement> statements { get; set; }

    public BlockStatement(List<Statement> statements) {
        this.statements = statements;
    }
    
    public void statement() {
        
    }
}
