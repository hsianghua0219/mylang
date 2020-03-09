using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyLang;

class Program
{
    /// <summary>
    /// コマンド の entry point
    /// </summary>
    /// <param name="args">コマンドライン引数</param>
    static void Main(string[] args)
    {
        bool tokenizeOnly = false; // tokenize だけで終わるかどうか
        bool parseOnly = false; // parse だけで終わるかどうか
        bool replEnabled = false;

    // 引数をparseする
    var rest = new List<string>();
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-h":
                case "--help":
                    ShowHelpAndExit();
                    break;
                case "-t":
                case "--tokenize":
                    tokenizeOnly = true;
                    break;
                case "-p":
                case "--parse":
                    parseOnly = true;
                    break;
                case "-d":
                case "--debug":
                    Logger.LogEnabled = true;
                    break;
                case "-e":
                case "--repl":
                    replEnabled = true;
                    break;
                default:
                    rest.Add(arg);
                    break;
            }
        }

        // 引数がないなら、ヘルプを表示して終わる
        if( rest.Count <= 0 && !replEnabled)
        {
            ShowHelpAndExit();
        }
        
        // 各実行器を用意する
        ITokenizer tokenizer = new SpaceSeparatedTokenizer();
        var parser = new Parser();
        var interpreter = new Interpreter();

        do {
            // Tokenize を行う
            var tokens = tokenizer.Tokenize(string.Join(" ", rest.ToArray()));

            if (tokenizeOnly)
            {
                Console.WriteLine(string.Join(" ", tokens.Select(t => t.Text).ToArray()));
                Exit(0);
            }

            // Parse を行う
            var ast = parser.Parse(tokens);

            if (parseOnly)
            {
                Console.WriteLine(new MyLang.Ast.AstDisplayer().BuildString(ast, false));
                Exit(0);
            }
            
            interpreter.Run(ast);

            if (replEnabled)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(">");
                string arg = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.White;
                rest = new List<string>() { arg };
                if (arg == "exit" || arg == "exit;") {
                    replEnabled = false;
                    break;
                }
            }
        }
        while (replEnabled);

        Exit(0);
    }

    /// <summary>
    /// ヘルプを表示して終わる
    /// </summary>
    static void ShowHelpAndExit()
    {
        Console.WriteLine(@"
My Small Language.

Usage:
    > MyLang.exe [options...] ""program""

Options:
    -e, --repl     : Read Eval Print Loop.
    -t, --tokenize : Show token list.
    -p, --parse    : Show parsed abstract syntax tree.
    -d, --debug    : Print debug log (for debug).
    -h, --help     : Show help.

Example:
    > MyLang.exe ""1 + 2""
    > MyLang.exe --repl ""2 * 3"" ""6 + 1"" ""exit""
    > MyLang.exe --debug ""1 + 2 * 3""
    > MyLang.exe --tokenize ""1 + 2 * 3""
    > MyLang.exe --parse ""1 + 2 * 3""
");
        Exit(0);
    }

    /// <summary>
    /// デバッガがアタッチされている場合は、キーの入力を待つ
    ///
    /// Visual Studioの開津環境で、コンソールがすぐに閉じてしまうのの対策として使用している
    /// </summary>
    static void WaitKey()
    {
        if (Debugger.IsAttached)
        {
            Console.ReadKey();
        }
    }

    /// <summary>
    /// 終了する
    /// </summary>
    /// <param name="resultCode"></param>
    static void Exit(int resultCode) 
    {
        WaitKey();
        Console.ForegroundColor = ConsoleColor.Gray;
        Environment.Exit(resultCode);
    }
}

