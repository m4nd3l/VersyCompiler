namespace Versy.AST.Types;

public class ArrayType : Type {
    public Type underlying { get; set; }
    public int? size       { get; set; }

    public ArrayType(Type underlying, int? size = null) {
        this.underlying = underlying;
        this.size = size;
    }

    public void type() {
    }
}
