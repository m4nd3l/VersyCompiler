namespace Versy.AST.Types;

public class ArrayType : Type {
    public Type underlying { get; set; }

    public ArrayType(Type underlying) {
        this.underlying = underlying;
    }

    public void type() {
    }
}
