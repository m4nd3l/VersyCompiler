namespace Versy.CustomExceptions;

public sealed class UnknownExpressionException  : SystemException {
    public UnknownExpressionException() { }
    public UnknownExpressionException(string? message) : base(message) { }
    public UnknownExpressionException(string? message, Exception? inner) : base(message, inner) { }
}
