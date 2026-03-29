using Spectre.Console;
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
        
        writeLine("string tolower(string @string) { return @string.ToLower(); }");
        writeLine("string toupper(string @string) { return @string.ToUpper(); }");
        
        return code.ToString();
    }

    private string translateStatement(Statement? statement) {
        if (statement == null) return "";
        return statement switch {
            VariableDeclarationStatement varDeclaration  => translateVariableDeclaration(varDeclaration),
            FunctionDeclarationStatement funcDeclaration => translateFunctionDeclaration(funcDeclaration),
            FunctionArgumentStatement funcArgument       => translateFunctionArgument(funcArgument),
            IfStatement ifStmt                           => translateIf(ifStmt),
            ForStatement forStmt                         => translateFor(forStmt),
            ForeachStatement foreachStmt                 => translateForeach(foreachStmt),
            WhileStatement whileStmt                     => translateWhile(whileStmt),
            ExpressionStatement expression               => translateExpressionStatement(expression),
            BlockStatement block                         => translateBlock(block),
            ReturnStatement @return                      => translateReturnStatement(@return),
        };
    }

    private string translateVariableDeclaration(VariableDeclarationStatement variableDeclaration) {
        string typePart = mapType(variableDeclaration.type);
        string modifier = "";
        
        if (variableDeclaration.isConstant && variableDeclaration.assignedValue != null)      modifier = "const";
        else if (variableDeclaration.isConstant && variableDeclaration.assignedValue == null) modifier = ""; 
        
        string assignment = "";
        if (variableDeclaration.assignedValue != null && variableDeclaration.type is ArrayType) 
            assignment = $" = {translateExpression(variableDeclaration.assignedValue)}";
        else if (variableDeclaration.assignedValue != null) 
            assignment = $" = ({typePart}) {translateExpression(variableDeclaration.assignedValue)}";
        
        string result = $"{modifier} {typePart} {variableDeclaration.identifier}{assignment};";
        writeLine(result);
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

    private string translateFor(ForStatement forStatement) {
        string result = "for (";
        write(result);
        result += translateStatement(forStatement.initialization);
        result += write(translateExpression(forStatement.condition) + ";");
        result += write(translateExpression(forStatement.increment) + ")");
        string body = translateStatement(forStatement.body);
        result += body;
        
        return result;
    }
    
    private string translateForeach(ForeachStatement foreachStatement) {
        string result = "foreach (";
        result += translateExpression(foreachStatement.left) + " : ";
        result += translateExpression(foreachStatement.iterable) + ")";
        writeLine(result);
        string body = translateStatement(foreachStatement.body);
        result += body;
        return result;
    }
    
    private string translateWhile(WhileStatement whileStatement) {
        string result = "while (";
        result += translateExpression(whileStatement.condition) + ")";
        writeLine(result);
        string body = translateStatement(whileStatement.body);
        result += body;
        return result;
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
        string type = mapType(functionArgument.type, false);
        string identifier = functionArgument.identifier;
        string result = $"{type} {identifier}";
        return result;
    }

    private string translateReturnStatement(ReturnStatement returnStatement) {
        string result = "return ";
        if (returnStatement.expression != null) result += translateExpression(returnStatement.expression);
        result += ";";
        writeLine(result);
        return result;
    }
        
    private string translateExpressionStatement(ExpressionStatement expressionStatement) {
        string result = translateExpression(expressionStatement.expression) + ";";
        writeLine(result);
        return result;
    }
    
    private string translateExpression(Expression expression) {
        return expression switch {
            NumberExpression number                        => $"{number.value}",
            StringExpression @string                       => $"\"{@string.value}\"",
            PrefixExpression prefix                        => $"{prefix.Operator.codeReference} {translateExpression(prefix.right)}",
            PostfixExpression postfix                      => $"{translateExpression(postfix.left)} {postfix.Operator.codeReference}",
            SymbolExpression symbol                        => $"{symbol.value}",
            BinaryExpression binary                        => $"{translateExpression(binary.left)} {binary.operation.codeReference} {translateExpression(binary.right)}",
            ArrayAssignmentExpression array                => $"new [] {{{string.Join(", ", array.value.Select(e => translateExpression(e)))}}}",
            IndexAccessExpression access                   => $"{translateExpression(access.left)}[{translateExpression(access.index)}]",
            AssignmentExpression assi                      => $"{translateExpression(assi.assigne)} {assi.operation.codeReference} {translateExpression(assi.right)}",
            ForeachLeftExpression @foreach                 => $"var {@foreach.identifier}",
            CallFunctionExpression callFunc                => $"{translateCallFunctionExpression(callFunc)}",
            MemberAccessExpression member                  => $"{translateExpression(member.left)}.{translateExpression(member.right)}",
            VariableInitializationViaConstructor viaConstr => $"{translateVariableInitializationViaConstructorExpression(viaConstr)}",
                                          _                => throw new UnknownExpressionException(),
        };
    }

    private string translateCallFunctionExpression(CallFunctionExpression callFunc) {
        string identifier = callFunc.identifier;
        List<string> args = new();
        foreach (Expression expr in callFunc.arguments) args.Add(translateExpression(expr));
        string f_args = "";
        bool isLast;
        for (int i = 0; i < args.Count; i++) {
            isLast =  i == args.Count - 1;
            f_args  += args[i];
            if (!isLast) f_args += ", ";
        }
        if (!callFunc.isBuiltin) return $"{identifier}({f_args})";
        identifier = callFunc.identifier switch {
            "println"   => "Console.WriteLine",
            "print"     => "Console.Write",
            "read"      => "Console.ReadLine",
            "toInt"     => "Int32.Parse",
            "toDecimal" => "Double.Parse",
            "toLower"   => "tolower",
            "toUpper"   => "toupper",
        };
        return $"{identifier}({f_args})";
    }

    private string translateVariableInitializationViaConstructorExpression(VariableInitializationViaConstructor viaConstr) {
        if (viaConstr.isArray) return $"new {mapType(viaConstr.type)}";
        string args = "";
        foreach (var arg in viaConstr.args)  args += translateExpression(arg);
        return $"new {mapType(viaConstr.type)}({args})";
    }

    private string mapType(Type type, bool arrayNumbers = true) {
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
            ArrayType arrayType => getFullArray(arrayType, arrayNumbers),
            _                   => "object"
        };
    }
    
    private string getFullArray(ArrayType arrayType, bool arrayNumbers) {
        Type current = arrayType;
        StringBuilder builder = new StringBuilder();

        while (current is ArrayType at) {
            builder.Append('[');
            builder.Append(at.size != null && arrayNumbers ? at.size : "");
            builder.Append(']');
            current = at.underlying;
        }

        return mapType(current) + builder;
    }
    
    private string writeLine(string text) {
        code.AppendLine(new string(' ', indent * 4) + text);
        return text;
    }
    
    private string write(string text) {
        code.Append(new string(' ', indent * 4) + text);
        return text;
    }
    
}
