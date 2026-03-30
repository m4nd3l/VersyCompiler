using Versy.AST;
using Versy.AST.Expressions;
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
        p.expect(Tokens.LBRACKET); // Consumes '{'
        var statements = new List<Statement>();
        
        while (p.getCurrentTokenType() != Tokens.RBRACKET && p.getCurrentTokenType() != Tokens.END_OF_FILE) statements.Add(p.statementParser.parseStatement(p));

        p.expect(Tokens.RBRACKET); // Consumes '}'
        return new BlockStatement(statements);
    }
    
    public Statement parseVariableDeclarationStatement(VersyParser p) {
        VariableDeclarationStatement statement;
        bool isConst = p.advance().type == Tokens.CONST;
        string identifier = (string) p.expectError(Tokens.IDENTIFIER).value;
        p.expect(Tokens.COLON);
        Type type = parseType(p);
        if (p.getCurrentTokenType() == Tokens.SEMICOLON) {
            statement = new VariableDeclarationStatement(identifier, isConst, null, type);
            p.advance();
            return statement;
        }
        p.expect(Tokens.ASSIGNMENT);
        Expression? value = null;
        if (p.getCurrentTokenType() == Tokens.NEW) {
            p.advance();
            // new(arg1, arg2);
            if (p.getCurrentTokenType() == Tokens.LPAREN) value = new VariableInitializationViaConstructor(type, getArgs());
            // new xyz(arg1, arg2); OR new xyz[a];
            else if (p.getCurrentTokenType() == Tokens.IDENTIFIER) {
                Type newType = parseType(p);
                value = new VariableInitializationViaConstructor(newType, getArgs());
            }
        } else value = p.expressionParser.parseExpression(p, BindingPower.ASSIGNMENT);
        p.expectSemicolon();
        statement = new VariableDeclarationStatement(identifier, isConst, value, type);
        return statement;

        List<Expression> getArgs() {
            if (p.getCurrentTokenType() != Tokens.LPAREN) return new();
            p.advance();
            List<Expression> args = new List<Expression>();
            while (p.getCurrentTokenType() != Tokens.RPAREN && p.getCurrentTokenType() != Tokens.END_OF_FILE) {
                Expression argExpression = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
                if (p.getCurrentTokenType() != Tokens.RPAREN) p.expect(Tokens.COMMA);
                args.Add(argExpression);
            }
            p.advance();
            return args;
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

    public Statement parseForStatement(VersyParser p) {
        p.advance();
        p.expect(Tokens.LPAREN);
        var initialize = parseVariableDeclarationStatement(p);
        var condition = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.SEMICOLON);
        var increment = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RPAREN);
        var body = p.getCurrentTokenType() != Tokens.LBRACKET ? parseStatement(p) : parseBlockStatement(p);
        return new ForStatement(initialize, condition, increment, body);
    }
    
    public Statement parseForeachStatement(VersyParser p) {
        p.advance();
        p.expect(Tokens.LPAREN);
        var left = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.COLON);
        var iterable = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RPAREN);
        var body = p.getCurrentTokenType() != Tokens.LBRACKET ? parseStatement(p) : parseBlockStatement(p);
        return new ForeachStatement(left, iterable, body);
    }
    
    public Statement parseWhileStatement(VersyParser p) {
        p.advance();
        p.expect(Tokens.LPAREN);
        var condition = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RPAREN);
        var body = p.getCurrentTokenType() != Tokens.LBRACKET ? parseStatement(p) : parseBlockStatement(p);
        return new WhileStatement(condition, body);
    }

    public Statement parseFunctionDeclarationStatement(VersyParser p) {
        p.expect(Tokens.FUNC);
        string identifier = (string) p.expect(Tokens.IDENTIFIER).value;
        p.expect(Tokens.LPAREN);
        List<Statement> args = new List<Statement>();
        while (p.getCurrentTokenType() != Tokens.RPAREN && p.getCurrentTokenType() != Tokens.END_OF_FILE) {
            string argIdentifier = (string) p.expect(Tokens.IDENTIFIER).value;
            p.expect(Tokens.COLON);
            Type argType = parseType(p);
            if (p.getCurrentTokenType() != Tokens.RPAREN) p.expect(Tokens.COMMA);
            args.Add(new FunctionArgumentStatement(argType, argIdentifier));
        }
        p.expect(Tokens.RPAREN);
        p.expect(Tokens.RARROW);
        Type @return = parseType(p);
        Statement body = parseBlockStatement(p);
        return new FunctionDeclarationStatement(identifier, args, @return, body);
    }

    public Statement parseReturnStatement(VersyParser p) {
        p.advance();
        Expression value = null;
        if (p.getCurrentTokenType() != Tokens.SEMICOLON) value = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
        p.expectSemicolon();
        return new ReturnStatement(value);
    }
    
    public Type parseType(VersyParser p) {
        Type type = new SymbolType((string) p.expectError(Tokens.IDENTIFIER).value);
        if (p.getCurrentTokenType() != Tokens.LSBRACKET) return type;
        bool firstTime = true;
        while (p.getCurrentTokenType() == Tokens.LSBRACKET) {
            if (firstTime) p.advance();
            firstTime = false;
            Expression? size = null;
            if (p.getCurrentTokenType() != Tokens.RSBRACKET) size = p.expressionParser.parseExpression(p, BindingPower.DEFAULT);
            p.expect(Tokens.RSBRACKET);
            type = new ArrayType(type, size);
        }
        return type;
    }
}