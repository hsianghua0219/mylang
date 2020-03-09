using System;
using System.Collections.Generic;
using System.Linq;

namespace MyLang
{
    namespace Ast
    {
        /// <summary>
        /// AST(Abstract Syntax Tree)のベースクラス
        /// </summary>
        public abstract class Ast
        {
            /// <summary>
            /// 文字列表現を作成するための情報を取得する.
            /// 
            /// string は 文字列でのそのASTの種類を表す
            /// Ast[] は、子供のASTを返す。子供を取らないASTの場合は、nullが入る。
            /// </summary>
            /// <returns>文字列表現のための情報</returns>
            public abstract Tuple<string, Ast[]> GetDisplayInfo();
        }
        /// <summary>
        /// 式(Expression) のベースクラス
        /// </summary>
        public abstract class Exp : Ast { }
        /// <summary>
        /// ２項演算子の種類
        /// </summary>
        public enum BinOpType { Add, Sub, Multiply, Divide, Equal, Equals, unEqual, Greater, Less, GreaterEqual, LessEqual, binopType }
        /// <summary>
        /// 二項演算子(Binary Operator)を表すAST
        /// </summary>
        public class BinOp : Exp
        {
            public readonly BinOpType Operator;
            public readonly Exp Lhs;
            public readonly Exp Rhs;
            public BinOp(BinOpType op, Exp lhs, Exp rhs)
            {
                Operator = op;
                Lhs = lhs;
                Rhs = rhs;
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create(Operator.ToString(), new Ast[] { Lhs, Rhs }); }
        }
        /// <summary>
        /// 数値を表すAST
        /// </summary>
        public class Number : Exp
        {
            public readonly decimal Value;
            public Number(decimal value) { Value = value; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create(Value.ToString(), (Ast[])null); }
        }
        public class Symbol : Exp
        {
            public readonly string Name;
            public Symbol(string name) { Name = name; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create(Name, (Ast[])null); }
        }
        public class ApplyFunction : Exp
        {
            public readonly Symbol Symbol;
            public readonly Exp[] Args;
            public string symname;
            public ApplyFunction(Symbol name, Exp[] args)
            {
                Symbol = name;
                Args = args;
                symname = Symbol.Name;
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create(symname + " " , new Ast[] {}.Concat(Args).ToArray()); }
        }
        public class Program : Ast
        {
            public readonly Statement[] Statements;
            public Program(IList<Statement> statements) { Statements = statements.ToArray(); }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Program", Statements.Select(s => (Ast)s).ToArray()); }
        }
        public abstract class Statement : Ast { }
        public class AssignStatement : Statement
        {
            public readonly Exp Variable;
            public readonly Exp Exp;
            public AssignStatement(Exp variable, Exp exp)
            {
                Variable = variable;
                Exp = exp;
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Let", new Ast[] { Variable, Exp }); }
        }
        public class PrintStatement : Statement
        {
            public readonly Exp Exp; 
            public PrintStatement( Exp exp) { Exp = exp; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Print", new Ast[] { Exp }); }
        }
        public class ReturnStatement : Statement
        {
            public readonly Exp Exp;
            public ReturnStatement(Exp exp) { Exp = exp; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Return", new Ast[] { Exp }); }
        }
        public class FunctionStatement : Statement
        {
            public readonly Symbol SymName;
            public readonly Symbol[] Parameters;
            public readonly Statement[] Body;
            public FunctionStatement(Symbol name, IList<Symbol> parameters, IList<Statement> body)
            {
                SymName = name;
                Parameters = parameters.ToArray();
                Body = body.ToArray();
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Function", new Ast[] { SymName, new AstList(Parameters), new AstList(Body) }); }
        }
        public class IfStatement : Statement
        {
            public readonly Exp ifThis;
            public readonly Statement[] toDo;
            public readonly Statement[] orDo;
            public IfStatement(Exp ifthis, IEnumerable<Statement> todo, IEnumerable<Statement> ordo)
            {
                ifThis = ifthis;
                toDo = todo.ToArray();
                if (ordo != null) orDo = ordo.ToArray();
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("If", new Ast[] { ifThis, new AstList(toDo), new AstList(orDo) }); }
        }
        public class WhileStatement : Statement
        {
            public readonly Exp ifThis;
            public readonly Statement[] toDo;
            public WhileStatement(Exp ifthis, IEnumerable<Statement> todo)
            {
                ifThis = ifthis;
                toDo = todo.ToArray();
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("While", new Ast[] { ifThis, new AstList(toDo) }); }
        }
        public class DoWhileStatement : Statement
        {
            public readonly Exp ifThis;
            public readonly Statement[] toDo;
            public DoWhileStatement(Exp ifthis, IEnumerable<Statement> todo)
            {
                ifThis = ifthis;
                toDo = todo.ToArray();
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("DoWhile", new Ast[] { ifThis, new AstList(toDo) }); }
        }
        public class ForStatement : Statement
        {
            public readonly Statement[] Let;
            public readonly Exp ifThis;
            public readonly Statement[] Update;
            public readonly Statement[] toDo;
            public ForStatement(IEnumerable<Statement> let, Exp ifthis, IEnumerable<Statement> update, IEnumerable<Statement> todo)
            {
                Let = let.ToArray();
                ifThis = ifthis;
                Update = update.ToArray();
                toDo = todo.ToArray();
            }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("For", new Ast[] { new AstList(Let), ifThis, new AstList(Update), new AstList(toDo) }); }
        }
        public class ExpressionStatement : Statement
        {
            public readonly Exp Exp;
            public ExpressionStatement(Exp exp) { Exp = exp; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("Expression", new Ast[] { Exp }); }
        }
        public class AstList : Ast
        {
            public Ast[] List;
            public AstList(Ast[] list) { List = list; }
            public override Tuple<string, Ast[]> GetDisplayInfo() { return Tuple.Create("", List); }
        }
        /// <summary>
        /// ASTを文字列表現に変換するクラス
        /// </summary>
        public class AstDisplayer
        {
            List<Tuple<int, string>> list_;
            public AstDisplayer() { }
            /// <summary>
            /// ASTから、文字列表現に変換する.
            /// 
            /// prettyPrintにtrueを指定すると、改行やインデントを挟んだ読みやすい表現になる
            /// 
            /// BuildString(1 + 2 * 3 の AST, false) => "Add( 1 Multiply( 2 3 ) )"
            /// 
            /// BuildString(1 + 2 * 3 の AST, true) => 
            ///   "Add( 
            ///     1 
            ///     Multiply(
            ///       2
            ///       3
            ///     )
            ///    )"
            /// </summary>
            /// <param name="ast">対象のAST</param>
            /// <param name="prettyPrint">Pretty pring をするかどうか</param>
            /// <returns></returns>
            public string BuildString(Ast ast, bool prettyPrint = true)
            {
                list_ = new List<Tuple<int, string>>();
                Build(0, ast);
                if (prettyPrint) return string.Join("\n", list_.Select(s => new string(' ', s.Item1 * 2) + s.Item2).ToArray());
                else return string.Join(" ", list_.Select(s => s.Item2).ToArray());
            }
            void Build(int level, Ast ast)
            {
                var displayInfo = ast.GetDisplayInfo();
                if (displayInfo.Item2 == null) Add(level, displayInfo.Item1);
                else
                {
                    Add(level, displayInfo.Item1 + "(");
                    foreach (var child in displayInfo.Item2) Build(level + 1, child);
                    Add(level, ")");
                }
            }
            void Add(int level, string text) { list_.Add(Tuple.Create(level, text)); }
        }
    }
}
