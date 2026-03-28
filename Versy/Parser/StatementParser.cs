using Versy.AST;
using Versy.AST.Statements;
using Versy.AST.Types;
using Versy.Errors;
using Versy.Lexer;
using Type = Versy.AST.Type;

namespace Versy.Parser;

public class StatementParser {
    public Statement parseStatement(VersyParser p) {
        var exists = p.statementLookup.ContainsKey(p.getCurrentTokenType());
        if (exists) {
            var statementFunction = p.statementLookup[p.getCurrentTokenType()];
            return statementFunction(p);
        }
        var expression = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expectSemicolon();
        return new ExpressionStatement(expression);
    }
    
    public Statement parseBlockStatement(VersyParser p) {
        p.expect(Tokens.LBRACKET); // Consuma '{'
        var statements = new List<Statement>();
        
        while (p.getCurrentTokenType() != Tokens.RBRACKET && p.getCurrentTokenType() != Tokens.END_OF_FILE) statements.Add(p.statementParser.parseStatement(p));

        p.expect(Tokens.RBRACKET); // Consuma '}'
        return new BlockStatement(statements);
    }
    
    public Statement parseVariableDeclarationStatement(VersyParser p) {
        // PATTERN: const/var identifier: type = value;
        // OR     : const/var identifier: type;
        // OR     : const/var identifier: type[];
        // OR     : const/var identifier: type[] = [value1, value2];
        VariableDeclarationStatement statement;
        bool isConst = p.advance().type == Tokens.CONST;
        string identifier = (string) p.expectError(Tokens.IDENTIFIER).value;
        p.expect(Tokens.COLON);
        Type type;
        parseType();
        if (p.getCurrentTokenType() == Tokens.SEMICOLON) {
            statement = new VariableDeclarationStatement(identifier, isConst, null, type);
            return statement;
        }
        p.expect(Tokens.ASSIGNMENT);
        Expression value = p.expressionParser.parseExpression(p, BindingPower.ASSIGNMENT);
        p.expectSemicolon();
        statement = new VariableDeclarationStatement(identifier, isConst, value, type);
        return statement;
        
        void parseType() {
            type = new SymbolType((string) p.expectError(Tokens.IDENTIFIER).value);
            if (p.getCurrentTokenType() != Tokens.LSBRACKET) return;
            bool firstTime = true;
            while (p.getCurrentTokenType() == Tokens.LSBRACKET) {
                if (firstTime) p.advance();
                firstTime = false;
                p.expect(Tokens.RSBRACKET);
                type = new ArrayType(type);
            }
        }
    }
    
    public Statement parseIfStatement(VersyParser p) {
        p.advance();
        p.expect(Tokens.LPAREN);
        var condition = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RPAREN);
        var body = p.getCurrentTokenType() != Tokens.LBRACKET ? parseStatement(p) : parseBlockStatement(p);

        Statement? elseBranch = null;
        if (p.getCurrentTokenType() == Tokens.ELSE) {
            p.advance(); // Consumes ELSE
            elseBranch = p.getCurrentTokenType() != Tokens.LBRACKET ? parseStatement(p) : parseBlockStatement(p);
        } else if (p.getCurrentTokenType() == Tokens.ELIF) elseBranch = parseIfStatement(p);
        return new IfStatement(condition, body, elseBranch);
    }
}
