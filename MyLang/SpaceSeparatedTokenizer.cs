using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyLang
{
    /// <summary>
    /// 単純なトークナイザ
    /// 
    /// トークンは、必ず一つ以上のスペースで区切られている必要がある
    /// </summary>
    class SpaceSeparatedTokenizer : ITokenizer
    {
        public SpaceSeparatedTokenizer() { }
        public IList<Token> Tokenize(string src)
        {
            // TODO: 仮のダミー実装
            var dummy = new List<Token>();
            //スペースと注釈の削除
            src = Regex.Replace(src, @"\s+", "");
            src = Regex.Replace(src, @"//.*", "");
            src = Regex.Replace(src, @"/\*.*\*/", "");
            //記号のスペースを対応する
            Dictionary<string, string> Regular = new Dictionary<string, string>
            { 
                /*Arithmetic*/ { "+", @"\+" }, { "-", @"\-" }, { "*", @"\*" }, { "/", @"\/" }, 
                /*Other*/ { "=", @"=" }, { ">", @">" }, { "<", @"<" }, 
                /*Equality*/ { "==", @"\s+=\s*?=\s+" }, { "!=", @"!\s*?=\s+" }, { ">=", @"\s+>\s*?=\s+" }, { "<=", @"\s+<\s*?=\s+" }, 
                /*Brackets*/ { "(", @"\(" }, { ")", @"\)" }, { "{", @"\{" }, { "}", @"\}" }, { ",", @"," }, { ";", @"\;" }, 
                /*Statements*/ { "let", @"let" }, { "print", @"print" }, { "return", @"return" }, { "function", @"function" }, { "if", @"if" }, { "else", @"else" }, { "while", @"while" }, { "for", @"for" }, { "do", @"do" },
            };
            foreach (KeyValuePair<string, string> regular in Regular) src = Regex.Replace(src, regular.Value, "\u0020" + regular.Key + "\u0020");
            //負数の文字が行の先頭にある場合にマッチします。
            src = Regex.Replace(src, @"^\s*?-\s+", "-");
            //正規表現で負の数
            foreach (KeyValuePair<string, string> regular in Regular) src = Regex.Replace(src, regular.Value + @"\s*?-\s+", regular.Key + "\u0020-");
            //スペースで分割する
            string[] match_src = src.Split('\u0020');

            //数字と記号を対応する
            Regex Num = new Regex(@"^-?\d+$");
            Regex Sym = new Regex(@"^[\w_][\w_0-9]*$");
            Dictionary<string, TokenType> Symbols = new Dictionary<string, TokenType>
            { 
                /*Arithmetic*/ { "+", TokenType.Plus }, { "-", TokenType.Minus }, { "*", TokenType.Star }, { "/", TokenType.Slash }, 
                /*Other*/ { "=", TokenType.Equal }, { ">", TokenType.G }, { "<", TokenType.L }, 
                /*Equality*/ { "==", TokenType.Equals }, { "!=", TokenType.unEqual }, { ">=", TokenType.GoE }, { "<=", TokenType.LoE }, 
                /*Brackets*/ { "(", TokenType.Round_L }, { ")", TokenType.Round_R }, { "{", TokenType.Curly_L }, { "}", TokenType.Curly_R }, { ",", TokenType.Comma }, { ";", TokenType.Semicolon }, 
                /*Statements*/ { "let", TokenType.Let }, { "print", TokenType.Print }, { "return", TokenType.Return }, { "function", TokenType.Function }, { "if", TokenType.If }, { "else", TokenType.Else }, { "do", TokenType.DoWhile }, { "while", TokenType.While }, { "for", TokenType.For },
            };
            for (int i = 0; i < match_src.Length; i++)
            {
                if (Symbols.ContainsKey(match_src[i])) dummy.Add(new Token(Symbols[match_src[i]], match_src[i]));
                else if (Num.IsMatch(match_src[i])) dummy.Add(new Token(TokenType.Number, match_src[i]));
                else if (Sym.IsMatch(match_src[i])) dummy.Add(new Token(TokenType.Symbol, match_src[i]));
            }
            dummy.Add(new Token(TokenType.Terminate, "[EOF]"));
            return dummy;
        }
    }
}
