namespace Versy.AST.Types;

public class SymbolType : Type {
    public string name { get; set; }

    public SymbolType(string name) {
        this.name = name;
    }

    public void type() {
    }
}
