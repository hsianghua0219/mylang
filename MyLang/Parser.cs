using System.Collections.Generic;

namespace MyLang
{
    public class Parser
    {
        IList<Token> tokens_;
        int pos_ = 0;
        static readonly Dictionary<TokenType, Ast.BinOpType> BinOpMap = new Dictionary<TokenType, Ast.BinOpType> { { TokenType.Plus, Ast.BinOpType.Add }, { TokenType.Minus, Ast.BinOpType.Sub }, { TokenType.Star, Ast.BinOpType.Multiply }, { TokenType.Slash, Ast.BinOpType.Divide }, { TokenType.Equal, Ast.BinOpType.Equal }, { TokenType.Equals, Ast.BinOpType.Equals }, { TokenType.unEqual, Ast.BinOpType.unEqual }, { TokenType.L, Ast.BinOpType.Less }, { TokenType.G, Ast.BinOpType.Greater }, { TokenType.LoE, Ast.BinOpType.LessEqual }, { TokenType.GoE, Ast.BinOpType.GreaterEqual } };
        Token CurrentToken() { return tokens_[pos_]; }
        void Progress()
        {
            Logger.Trace($"progress {CurrentToken().Text}");
            pos_++;
        }
        Token Consume(TokenType expected)
        {
            var token = CurrentToken();
            Progress();
            return token;
        }
        public Ast.Ast Parse(IList<Token> tokens)
        {
            tokens_ = tokens;
            pos_ = 0;
            if (CurrentToken().IsNumber) return Exp();
            else return Program();
        }
        Ast.Program Program()
        {
            var Ast = new List<Ast.Statement>();
            while (!CurrentToken().IsTerminate) Ast.Add(Statement());
            return new Ast.Program(Ast);
        }
        Ast.Statement[] Block()
        {
            var statements = new List<Ast.Statement> { Statement() };
            return statements.ToArray();
        }
        Ast.Statement Statement()
        {
            switch (CurrentToken().Type)
            {
                case TokenType.If: return If();
                case TokenType.For: return For();
                case TokenType.Let: return Assign();
                case TokenType.Print: return Print();
                case TokenType.While: return While();
                case TokenType.Return: return Return();
                case TokenType.DoWhile: return DoWhile();
                case TokenType.Function: return Function();
                default:
                    if (CurrentToken().IsNumber || CurrentToken().IsSymbol || CurrentToken().IsBinaryOperator) return Expression();
                    else return null;
            }
        }
        Ast.Statement Assign()
        {
            Consume(TokenType.Let);
            var symbol = Exp();
            Consume(TokenType.Equal);
            var exp = Exp();
            Consume(TokenType.Semicolon);
            return new Ast.AssignStatement(symbol, exp);
        }
        Ast.Statement Print()
        {
            Consume(TokenType.Print);
            var exp = Exp();
            Consume(TokenType.Semicolon);
            return new Ast.PrintStatement(exp);
        }
        Ast.Statement Return()
        {
            Consume(TokenType.Return);
            var exp = Exp();
            Consume(TokenType.Semicolon);
            return new Ast.ReturnStatement(exp);
        }
        Ast.Statement Function()
        {
            Consume(TokenType.Function);
            var symbol = Symbol();
            var parameters = new List<Ast.Symbol>();
            if (CurrentToken().Type == TokenType.Round_L)
            {
                Consume(TokenType.Round_L);
                if (CurrentToken().Type != TokenType.Round_R)
                {
                    while (true)
                    {
                        parameters.Add(Symbol());
                        if (CurrentToken().Type != TokenType.Comma) break;
                        Consume(TokenType.Comma);
                    }
                }
                Consume(TokenType.Round_R);
            }
            Consume(TokenType.Curly_L);
            var block = Block();
            Consume(TokenType.Curly_R);
            return new Ast.FunctionStatement(symbol, parameters.ToArray(), block);
        }
        Ast.Statement If()
        {
            Consume(TokenType.If);
            Consume(TokenType.Round_L);
            var ifthis = Exp();
            Consume(TokenType.Round_R);
            Consume(TokenType.Curly_L);
            var todo = Block();
            Consume(TokenType.Curly_R);
            Ast.Statement[] ordo = null;
            if (CurrentToken().Type == TokenType.Else)
            {
                Consume(TokenType.Else);
                if (CurrentToken().Type == TokenType.If) ordo = Block(); 
                else
                { 
                    Consume(TokenType.Curly_L);
                    ordo = Block();
                    Consume(TokenType.Curly_R);
                }
            }
            return new Ast.IfStatement(ifthis, todo, ordo);
        }
        Ast.Statement While()
        {
            Consume(TokenType.While);
            Consume(TokenType.Round_L);
            var ifthis = Exp();
            Consume(TokenType.Round_R);
            Consume(TokenType.Curly_L);
            var todo = Block();
            Consume(TokenType.Curly_R);
            return new Ast.WhileStatement(ifthis, todo);
        }
        Ast.Statement DoWhile()
        {
            Consume(TokenType.DoWhile);
            Consume(TokenType.Curly_L);
            var todo = Block();
            Consume(TokenType.Curly_R);
            Consume(TokenType.While);
            Consume(TokenType.Round_L);
            var ifthis = Exp();
            Consume(TokenType.Round_R);
            Consume(TokenType.Comma);
            return new Ast.DoWhileStatement(ifthis, todo);
        }
        Ast.Statement For()
        {
            Consume(TokenType.For);
            Consume(TokenType.Round_L);
            var let = Block();
            var ifthis = Exp();
            Consume(TokenType.Comma);
            var update = Block();
            Consume(TokenType.Round_R);
            Consume(TokenType.Curly_L);
            var todo = Block();
            Consume(TokenType.Curly_R);
            return new Ast.ForStatement(let, ifthis, update, todo);
        }
        Ast.Statement Expression() { return new Ast.ExpressionStatement(Exp()); }
        Ast.Symbol Symbol() { return new Ast.Symbol(Consume(TokenType.Symbol).Text); }
        Ast.Exp Exp() { return Exp_Condition(); }
        Ast.Exp Exp_Condition() { return Exp_Condition_R(Exp1()); }
        Ast.Exp Exp_Condition_R(Ast.Exp lhs)
        {
            var token_type = CurrentToken().Type;
            if (token_type == TokenType.G || token_type == TokenType.GoE || token_type == TokenType.L || token_type == TokenType.LoE || token_type == TokenType.Equals || token_type == TokenType.unEqual)
            {
                var binopType = BinOpMap[token_type];
                Progress();
                var rhs = Exp1();
                return Exp_Condition_R(new Ast.BinOp(binopType, lhs, rhs));
            }
            else return lhs;
        }
        Ast.Exp Exp1() { return Exp1_R(Exp2()); }
        Ast.Exp Exp1_R(Ast.Exp lhs)
        {
            var token_type = CurrentToken().Type;
            if (token_type == TokenType.Plus || token_type == TokenType.Minus)
            {
                Progress();
                return Exp1_R(new Ast.BinOp(BinOpMap[token_type], lhs, Exp2()));
            }
            else return lhs;
        }
        Ast.Exp Exp2() { return Exp2_R(Exp_V()); }
        Ast.Exp Exp2_R(Ast.Exp lhs)
        {
            var token_type = CurrentToken().Type;
            if (token_type == TokenType.Round_L) return Exp_V();
            if (token_type == TokenType.Star || token_type == TokenType.Slash)
            {
                Progress();
                return Exp2_R(new Ast.BinOp(BinOpMap[token_type], lhs, Exp_V()));
            }
            else return lhs;
        }
        Ast.Exp Exp_V()
        {
            var token = CurrentToken();
            if (token.IsNumber)
            {
                Progress();
                return new Ast.Number(float.Parse(token.Text));
            }
            else if (token.IsSymbol) {
                var name = Symbol();
                if (CurrentToken().Type == TokenType.Round_L)
                {
                    Consume(TokenType.Round_L);
                    var exp = new List<Ast.Exp>();
                    while (true)
                    {
                        exp.Add(Exp());
                        if (CurrentToken().Type != TokenType.Comma) break;
                        Consume(TokenType.Comma);
                    }
                    Consume(TokenType.Round_R);
                    return new Ast.ApplyFunction(name, exp.ToArray());
                }
                else return name;
            }
            else return null;
        }
    }
}


