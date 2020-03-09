using System;
using System.Collections.Generic; 
using System.Linq;
using MyLang.Ast;

namespace MyLang
{
    public class Interpreter
    {
        decimal Value;
        public bool fib = false;
        public Stack<decimal> stack = new Stack<decimal>();
        readonly Dictionary<string, decimal> var = new Dictionary<string, decimal>();
        readonly Dictionary<string, FunctionStatement> fun = new Dictionary<string, FunctionStatement>();
        public void Run(Ast.Ast ast) { if (ast is Exp) Console.WriteLine(Exp((Exp)ast)); else Block(((Ast.Program)ast).Statements); }
        public void Block(Statement[] statements)
        {
            foreach (var statement in statements)
            {
                if (statement is ExpressionStatement Expression) Console.WriteLine(Exp(Expression.Exp));
                else if (statement is AssignStatement Assign) var[((Symbol)Assign.Variable).Name] = Exp(Assign.Exp);
                else if (statement is IfStatement If) { if (Exp(If.ifThis) != 0) Block(If.toDo); else if (If.orDo != null) Block(If.orDo); }
                else if (statement is WhileStatement While) while (Exp(While.ifThis) == 0) Block(While.toDo);
                else if (statement is DoWhileStatement DoWhile) { do Block(DoWhile.toDo); while (Exp(DoWhile.ifThis) == 0); }
                else if (statement is ForStatement For) for (Block(For.Let); Exp(For.ifThis) == 0; Block(For.Update)) Block(For.toDo);
                else if (statement is FunctionStatement Function) fun[Function.SymName.Name]=Function;
                else if (statement is ReturnStatement Return) { Value = Exp(Return.Exp); if (stack.Count > 1) stack.Pop(); }
                else if (statement is PrintStatement Print) Console.WriteLine(Exp(Print.Exp));
            }
        }
        decimal Exp(Exp exp)
        {
            if (exp is BinOp binop)
            {
                decimal lhs = Exp(binop.Lhs), rhs = Exp(binop.Rhs);
                switch (binop.Operator)
                {
                    case BinOpType.Add: return lhs + rhs;
                    case BinOpType.Sub: return lhs - rhs;
                    case BinOpType.Divide: return lhs / rhs;
                    case BinOpType.Multiply: return lhs * rhs;
                    case BinOpType.Less: return (lhs < rhs) ? 1 : 0;
                    case BinOpType.Equals: return (lhs == rhs) ? 1 : 0;
                    case BinOpType.Greater: return (lhs > rhs) ? 1 : 0;
                    case BinOpType.unEqual: return (lhs != rhs) ? 1 : 0;
                    case BinOpType.LessEqual: return (lhs <= rhs) ? 1 : 0;
                    case BinOpType.GreaterEqual: return (lhs >= rhs) ? 1 : 0;
                    default: return 0;
                }
            }
            else if (exp is Number number) return number.Value;
            else if (exp is Symbol symbol) { if (!fib) return var[symbol.Name]; else return stack.Peek(); }
            else if (exp is ApplyFunction expAF)
            {
                var funName = fun[expAF.Symbol.Name];
                var args = expAF.Args.Select(arg => Exp(arg)).ToArray();
                if (funName.Parameters.Length == 1) fib = true;
                for (int i = 0; i < funName.Parameters.Length; i++) { stack.Push(args[i]); var[funName.Parameters[i].Name] = args[i]; }
                Block(funName.Body);
                return Value;
            }
            else return 0;
        }
    }
}
 