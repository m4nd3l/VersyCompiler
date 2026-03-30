namespace Versy.AST.Types;

public class ArrayType : Type {
    public Type        underlying { get; set; }
    public Expression? size       { get; set; }

    public ArrayType(Type underlying, Expression? size = null) {
        this.underlying = underlying;
        this.size = size;
    }

    public void type() {
    }
}
