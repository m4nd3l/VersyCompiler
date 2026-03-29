using System.Text;

using Versy.AST;
using Versy.AST.Expressions;
using Versy.AST.Statements;
using Versy.AST.Types;
using Versy.CustomExceptions;
using Type = Versy.AST.Type;

namespace Versy.Codegen;

public class CSharpTranslator : Translator {
    private BlockStatement AST           { get; set; }
    private StringBuilder  code          { get; set; } = new();
    private int            indent        { get; set; } = 0;


    public CSharpTranslator(BlockStatement AST) {
        this.AST       = AST;
    }
    
    public string translate() {
        code.Clear();
        writeLine("using System;");
        writeLine("using System.Text;");
        writeLine("using System.Collections.Generic;");
        writeLine("Console.OutputEncoding = Encoding.UTF8;");

        var tops = AST.statements;//AST.statements.Where(s => s is not ClassDefinition && s is not FunctionDefinition);
        var definitions = AST.statements;//AST.statements.Where(s => s is ClassDefinition || s is FunctionDefinition);
        
        foreach (var top in tops) translateStatement(top);
        //foreach (var definition in definitions) translateDefinition(definition);
        
        return code.ToString();
    }

    private string translateStatement(Statement? statement) {
        if (statement == null) return "";
        return statement switch {
            VariableDeclarationStatement varDeclaration  => translateVariableDeclaration(varDeclaration),
            FunctionDeclarationStatement funcDeclaration => translateFunctionDeclaration(funcDeclaration),
            FunctionArgumentStatement funcArgument       => translateFunctionArgument(funcArgument),
            IfStatement ifStmt                           => translateIf(ifStmt),
            ExpressionStatement expression               => translateExpressionStatement(expression),
            BlockStatement block                         => translateBlock(block),
        };
    }

    private string translateVariableDeclaration(VariableDeclarationStatement variableDeclaration) {
        string typePart = mapType(variableDeclaration.type);
        string modifier = "";
        
        if (variableDeclaration.isConstant && variableDeclaration.assignedValue != null)      modifier = "const";
        else if (variableDeclaration.isConstant && variableDeclaration.assignedValue == null) modifier = ""; 
        
        string assignment = ";";
        if (variableDeclaration.assignedValue != null) assignment = $" = ({typePart}) {translateExpression(variableDeclaration.assignedValue)}";
        
        string result = $"{modifier} {typePart} {variableDeclaration.identifier}{assignment};";
        writeLine(result);
        return result;
    }

    private string translateFunctionDeclaration(FunctionDeclarationStatement functionDeclaration) {
        string result;
        string returnType = mapType(functionDeclaration.returnType);
        result = $"{returnType} {functionDeclaration.identifier}(";
        write(result);
        List<string> args = new();
        foreach (Statement stmt in functionDeclaration.args) args.Add(translateStatement(stmt));
        string fargs = "";
        bool isLast;
        for (int i = 0; i < args.Count; i++) {
            isLast =  i == args.Count - 1;
            fargs  += args[i];
            if (!isLast) fargs += ", ";
        }
        result += fargs;
        write(fargs);
        result += ")";
        write(")");
        string body = translateStatement(functionDeclaration.body);
        result += body;
        return result;
    }
    
    private string translateFunctionArgument(FunctionArgumentStatement functionArgument) {
        string type = mapType(functionArgument.type);
        string identifier = functionArgument.identifier;
        string result = $"{type} {identifier}";
        return result;
    }
    
    private string translateIf(IfStatement ifStatement, bool nested = false) {
        StringBuilder localResult = new();

        string condition = $"if ({translateExpression(ifStatement.condition)})";
        if (!nested) writeLine(condition);
        else writeLine(condition);
        localResult.Append(condition);
        
        string body = translateStatement(ifStatement.body);
        localResult.Append(body);
        
        if (ifStatement.elses != null) {
            if (ifStatement.elses is IfStatement elseIf) {
                writeLine("else");
                string nextIf = translateIf(elseIf, true); 
                localResult.Append(" else " + nextIf);
            } else {
                writeLine("else");
                string elseBody = translateStatement(ifStatement.elses);
                localResult.Append(" else " + elseBody);
            }
        }

        return localResult.ToString();
    }

    private string translateBlock(BlockStatement block) {
        string result = "{";
        writeLine("{");
        indent++;
        foreach (var statement in block.statements) result += translateStatement(statement);
        indent--;
        writeLine("}");
        return result + "}";
    }
        
    private string translateExpressionStatement(ExpressionStatement expressionStatement) {
        string result = translateExpression(expressionStatement.expression) + ";";
        writeLine(result);
        return result;
    }
    
    private string translateExpression(Expression expression) {
        return expression switch {
            NumberExpression number         => $"{number.value}",
            StringExpression @string        => $"\"{@string.value}\"",
            PrefixExpression prefix         => $"{prefix.Operator.codeReference} {translateExpression(prefix.right)}",
            PostfixExpression postfix       => $"{translateExpression(postfix.left)} {postfix.Operator.codeReference}",
            SymbolExpression symbol         => symbol.value,
            BinaryExpression binary         => $"{translateExpression(binary.left)} {binary.operation.codeReference} {translateExpression(binary.right)}",
            ArrayAssignmentExpression array => $"new [] {{{string.Join(", ", array.value.Select(e => translateExpression(e)))}}}",
            IndexAccessExpression access    => $"{translateExpression(access.left)}[{translateExpression(access.index)}]",
            AssignmentExpression assi       => $"{translateExpression(assi.assigne)} {assi.operation.codeReference} {translateExpression(assi.right)}",
            CallFunctionExpression callFunc => translateCallFunctionExpression(callFunc),
                                          _ => throw new UnknownExpressionException(),
        };
    }

    private string translateCallFunctionExpression(CallFunctionExpression callFunc) {
        string identifier = callFunc.identifier;
        List<string> args = new();
        foreach (Expression expr in callFunc.arguments) args.Add(translateExpression(expr));
        string fargs = "";
        bool isLast;
        for (int i = 0; i < args.Count; i++) {
            isLast =  i == args.Count - 1;
            fargs  += args[i];
            if (!isLast) fargs += ", ";
        }
        if (!callFunc.isBuiltin) return $"{identifier}({fargs})";
        identifier = callFunc.identifier switch {
            "println" => "Console.WriteLine",
            "print"   => "Console.Write",
            "read"    => "Console.ReadLine"
        };
        return $"{identifier}({fargs})";
    }

    private string mapType(Type type) {
        return type switch {
            SymbolType symbolType => symbolType.name switch {
                "int"     => "int",
                "decimal" => "double",
                "string"  => "string",
                "bool"    => "bool",
                "char"    => "char",
                "object"  => "object",
                       _  => symbolType.name
            },
            ArrayType arrayType => getFullArray(arrayType),
            _ => "object"
        };
    }
    
    private string getFullArray(ArrayType arrayType) {
        int dimensions = 0;
        Type current = arrayType;

        while (current is ArrayType at) {
            dimensions++;
            current = at.underlying;
        }

        string baseTypeName = mapType(current);
        return baseTypeName + string.Concat(Enumerable.Repeat("[]", dimensions));
    }
    
    private void writeLine(string text) {
        code.AppendLine(new string(' ', indent * 4) + text);
    }
    
    private void write(string text) {
        code.Append(new string(' ', indent * 4) + text);
    }
    
}
