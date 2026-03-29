using System.ComponentModel.DataAnnotations;
using Versy.AST;
using Versy.AST.Expressions;
using Versy.Errors;
using Versy.Lexer;

namespace Versy.Parser;

public class ExpressionParser {
    
    public Expression parseExpression(VersyParser p, BindingPower bindingPower) {
        var tokenType = p.getCurrentTokenType();
    
        if (!p.nudLookup.ContainsKey(tokenType)) {
            p.errors.Add(new Error($"NUD Handler expected for token {tokenType}.", p.getCurrentToken().position));
            return null;
        }
    
        var nudFunction = p.nudLookup[tokenType];
        var left = nudFunction(p);

        while (bindingPower < p.bpLookup.GetValueOrDefault(p.getCurrentTokenType(), BindingPower.DEFAULT)) {
            var currentType = p.getCurrentTokenType();
            var ledFunction = p.ledLookup.GetValueOrDefault(currentType);
    
            if (ledFunction == null) break; 
            
            left = ledFunction(p, left, p.bpLookup.GetValueOrDefault(currentType, BindingPower.DEFAULT));
        }

        return left;
    }
    
    public Expression parseBinaryExpression(VersyParser p, Expression left, BindingPower operatorPrecedence) {
        var operatorToken = p.advance();
        var right = parseExpression(p, operatorPrecedence); 
        return new BinaryExpression(left, operatorToken, right);
    }
        
    public Expression parsePrimaryExpression(VersyParser p) {
        var oldToken = p.advance();
        if (oldToken.type == Tokens.IDENTIFIER && p.getCurrentTokenType() == Tokens.LPAREN) return parseFunctionCallExpression(p, oldToken);
        if (oldToken.type == Tokens.IDENTIFIER && 
                   oldToken.value.Equals("versy") && 
                   p.getCurrentTokenType() == Tokens.DOT) {
            p.expect(Tokens.DOT);
            return parseFunctionCallExpression(p, p.advance(), true);
        }
        // TODO : Add member access for classes and classes methods and variables
        return oldToken.type switch {
            Tokens.NUMBER     => new NumberExpression((double) oldToken.value),
            Tokens.STRING     => new StringExpression((string) oldToken.value),
            Tokens.IDENTIFIER => new SymbolExpression((string) oldToken.value),
        };
    }
    
    public Expression parseAssignmentExpression(VersyParser p, Expression left, BindingPower operatorPrecedence) {
        var operatorToken = p.advance();
        var value = parseExpression(p, BindingPower.ASSIGNMENT);
        return new AssignmentExpression(left, operatorToken, value);
    }
    
    public Expression parsePrefixExpression(VersyParser p) {
        var operatorToken = p.advance();
        var right = parseExpression(p, BindingPower.DEFAULT);
        return new PrefixExpression(operatorToken, right);
    }
    
    public Expression parsePostfixExpression(VersyParser p, Expression left, BindingPower operatorPrecedence) {
        var operatorToken = p.advance();
        return new PostfixExpression(operatorToken, left);
    }

    public Expression parseGroupingExpression(VersyParser p) {
        p.advance();
        var expression = parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RPAREN);
        return expression;
    }
    
    public Expression parseArrayAssignmentExpression(VersyParser p) {
        p.advance();
        List<Expression> elements = new List<Expression>();
        
        if (p.getCurrentTokenType() != Tokens.RSBRACKET) {
            while (true) {
                elements.Add(parseExpression(p, BindingPower.DEFAULT));
                
                if (p.getCurrentTokenType() == Tokens.COMMA) {
                    p.advance();
                    if (p.getCurrentTokenType() == Tokens.RSBRACKET) 
                        break;
                } else break;
            }
        }
        
        p.expect(Tokens.RSBRACKET); 
        return new ArrayAssignmentExpression(elements);
    }
    
    public Expression parseIndexAccessExpression(VersyParser p, Expression left, BindingPower operatorPrecedence) {
        p.advance();
        
        var index = parseExpression(p, BindingPower.DEFAULT);
        p.expect(Tokens.RSBRACKET);
        
        return new IndexAccessExpression(left, index);
    }

    public Expression parseFunctionCallExpression(VersyParser p, Token oldToken, bool isBuiltin = false) {
        string identifier = (string) oldToken.value;
        p.expect(Tokens.LPAREN);
        List<Expression> args = new List<Expression>();
        while (p.getCurrentTokenType() != Tokens.RPAREN && p.getCurrentTokenType() != Tokens.END_OF_FILE) {
            Expression argExpression = parseExpression(p, BindingPower.DEFAULT);
            if (p.getCurrentTokenType() != Tokens.RPAREN) p.expect(Tokens.COMMA);
            args.Add(argExpression);
        }
        p.expect(Tokens.RPAREN);
        return new CallFunctionExpression(identifier, args, isBuiltin);
    }
}
