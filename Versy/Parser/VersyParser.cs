using Versy.AST;
using Versy.AST.Expressions;
using Versy.AST.Statements;
using Versy.Errors;
using Versy.Lexer;
using Type = Versy.AST.Type;

namespace Versy.Parser;

public class VersyParser {
    private List<Token>              tokens    { get; set; }
    private int                      position  { get; set; }
    public  List<Error>              errors    { get; set; }

    public ExpressionParser expressionParser { get; set; } = new();
    public StatementParser  statementParser  { get; set; } = new();
    
    public delegate Statement statementHandler(VersyParser parser);
    public delegate Expression nudHandler(VersyParser parser);
    public delegate Expression ledHandler(VersyParser parser, Expression left, BindingPower bindingPower);
    
    public Dictionary<Tokens, statementHandler> statementLookup { get; set; } = new();
    public Dictionary<Tokens, nudHandler>       nudLookup  { get; set; } = new();
    public Dictionary<Tokens, ledHandler>       ledLookup  { get; set; } = new();
    public Dictionary<Tokens, BindingPower>     bpLookup   { get; set; } = new();

    public VersyParser(List<Token> tokens) {
        this.tokens = tokens.Where(t => 
                                       t.type != Tokens.WHITEPSPACE && 
                                       t.type != Tokens.COMMENT && 
                                       t.type != Tokens.MULTILINE_COMMENT).ToList();
        errors      = new List<Error>();
        createTokenLookups();
    }
    public BlockStatement parse() {
        var body = new List<Statement>();
        while (hasTokens()) body.Add(statementParser.parseStatement(this));
        return new BlockStatement(body);
    }
    private void createTokenLookups() {
        bpLookup[Tokens.SEMICOLON]   = BindingPower.DEFAULT;
        bpLookup[Tokens.END_OF_FILE] = BindingPower.DEFAULT;
        bpLookup[Tokens.WHITEPSPACE] = BindingPower.DEFAULT;
        
        // Literals & Identifiers
        nud(Tokens.NUMBER,  expressionParser.parsePrimaryExpression);
        nud(Tokens.STRING,  expressionParser.parsePrimaryExpression);
        nud(Tokens.IDENTIFIER,  expressionParser.parsePrimaryExpression);
        
        nud(Tokens.MINUS,  expressionParser.parsePrefixExpression);

        nud(Tokens.LPAREN,  expressionParser.parseGroupingExpression);
        
        nud(Tokens.LSBRACKET, expressionParser.parseArrayAssignmentExpression);
        
        // Logical
        led(Tokens.AND, BindingPower.LOGICAL, expressionParser.parseBinaryExpression);
        led(Tokens.OR, BindingPower.LOGICAL, expressionParser.parseBinaryExpression);
        
        // Relational
        led(Tokens.GREATER, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        led(Tokens.GREATER_EQUALS, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        led(Tokens.LESS, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        led(Tokens.LESS_EQUALS, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        led(Tokens.EQUALS, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        led(Tokens.NOT_EQUALS, BindingPower.RELATIONAL, expressionParser.parseBinaryExpression);
        
        // Additive & Multiplicative
        led(Tokens.PLUS, BindingPower.ADDITIVE, expressionParser.parseBinaryExpression);
        led(Tokens.MINUS, BindingPower.ADDITIVE, expressionParser.parseBinaryExpression);
        led(Tokens.STAR, BindingPower.MULTIPLICATIVE, expressionParser.parseBinaryExpression);
        led(Tokens.SLASH, BindingPower.MULTIPLICATIVE, expressionParser.parseBinaryExpression);
        led(Tokens.PERCENT, BindingPower.MULTIPLICATIVE, expressionParser.parseBinaryExpression);
        led(Tokens.POWER, BindingPower.MULTIPLICATIVE, expressionParser.parseBinaryExpression);
        
        led(Tokens.ASSIGNMENT, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);
        led(Tokens.PLUS_EQUALS, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);
        led(Tokens.MINUS_EQUALS, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);
        led(Tokens.STAR_EQUALS, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);
        led(Tokens.SLASH_EQUALS, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);
        led(Tokens.PERCENT_EQUALS, BindingPower.ASSIGNMENT, expressionParser.parseAssignmentExpression);

        led(Tokens.PLUS_PLUS, BindingPower.MEMBER, expressionParser.parsePostfixExpression);
        led(Tokens.MINUS_MINUS, BindingPower.MEMBER, expressionParser.parsePostfixExpression);
        
        // Statements
        statement(Tokens.CONST, statementParser.parseVariableDeclarationStatement);
        statement(Tokens.VAR, statementParser.parseVariableDeclarationStatement);
        statement(Tokens.IF, statementParser.parseIfStatement);

        // Access
        led(Tokens.LSBRACKET, BindingPower.MEMBER, expressionParser.parseIndexAccessExpression);
    }
    
    #region HELPERS
    private void led(Tokens type, BindingPower bindingPower, ledHandler handler) { bpLookup[type] = bindingPower; ledLookup[type] = handler; }
    private void nud(Tokens type, nudHandler handler) { nudLookup[type] = handler; }
    private void statement(Tokens type, statementHandler handler) { bpLookup[type] = BindingPower.DEFAULT; statementLookup[type] = handler; }
    public bool match(Tokens token) { advance(); return getPreviousToken().type == token; }
    public bool matchNoAdvance(Tokens token) { return getCurrentTokenType() == token; }
    public Token expectSemicolon() { return expectError(Tokens.SEMICOLON); }
    public Token expect(Tokens token) { return expectError(token); }
    public Token expectError(Tokens token, Error? error = null) {
        var oldToken = getCurrentToken();
        if (oldToken.type == token) return advance();
        
        if (error == null) error = new Error($"Expected {token} but got {oldToken.type}. Position",  oldToken.position);
        errors.Add(error);
        return advance();
    }
    public Token getCurrentToken() { return tokens[position]; }
    public Token getNextToken() { return tokens[position + 1]; }
    public Tokens getCurrentTokenType() { return getCurrentToken().type; }
    public Token advance() { var token = getCurrentToken(); position++; return token; }
    public Token getPreviousToken() { return tokens[position - 1]; }
    public bool hasTokens() { return position < tokens.Count && !getCurrentToken().isEOF(); }
    #endregion
}