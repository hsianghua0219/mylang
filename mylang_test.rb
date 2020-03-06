require 'pp'
require 'fileutils'
require 'open3'

MY_LANG_EXE = "MyLang/bin/Debug/MyLang.exe"
BUG_LIST = []
NOW_TEST = "Test"

def run_test(testcases, cmd)
  total = 0
  success = 0

  testcases.each do |input, expected_output|
    total += 1
    output, status = Open3.capture2e(*cmd, input)
    output.strip!

    if status.exitstatus != 0
	  print "!"
      BUG_LIST.push "×#{NOW_TEST}-<#{total}>-BUG \n| Input-Error:\"#{input}\""
      next
    end
    
    if output != expected_output.to_s
	  print "!"
	  BUG_LIST.push "×#{NOW_TEST}-<#{total}>-BUG \n| Input: \"#{input}\" \n| Error: \"#{output}\" \n| Really:\"#{expected_output}\"\n"
    else
      success += 1
	  print "."
    end
  end
  if total == success
    puts " OK(#{total})"
  else
    puts " BUG(#{total-success})"
    #raise
  end
end

def test_tokenizer
  testcases = [
    ["1+2-3*4/5", "1 + 2 - 3 * 4 / 5 [EOF]"], # 四則計算
    ["a+b-c*d/e", "a + b - c * d / e [EOF]"], # 四則変数
    ["-1+-2--3*-4/-5", "-1 + -2 - -3 * -4 / -5 [EOF]"], #四則負数
    ["1   + /*6*5/4-8*/  2-3//5+3", "1 + 2 - 3 [EOF]"], #スペースと注釈
    ["1=1>-1<1==1>=1<=1!=-1", "1 = 1 > -1 < 1 == 1 >= 1 <= 1 != -1 [EOF]"], #比較記号
    ["let a=1;function A(b,c){return b+c;}if(a!=1){print A(2,3);}else{print A(4,5);}", "let a = 1 ; function A ( b , c ) { return b + c ; } if ( a != 1 ) { print A ( 2 , 3 ) ; } else { print A ( 4 , 5 ) ; } [EOF]"], #関数と括弧
  ]
  print "﹥Tokenizer "
  NOW_TEST.replace("Tokenizer")
  run_test(testcases, [MY_LANG_EXE, '--tokenize'])
end


def test_parser
  testcases = [
    ["1+2-3*4/5", "Sub( Add( 1 2 ) Divide( Multiply( 3 4 ) 5 ) )"], # 四則計算
    ["a+b-c*d/e", "Program( Expression( Sub( Add( a b ) Divide( Multiply( c d ) e ) ) ) )"], # 四則変数
    ["-1+-2--3*-4/-5", "Sub( Add( -1 -2 ) Divide( Multiply( -3 -4 ) -5 ) )"], #四則負数
    ["1   + /*6*5/4-8*/  2-3//5+3", "Sub( Add( 1 2 ) 3 )"], #スペースと注釈
	["function A(b,c){return b+c;}print A(1,2);", "Program( Function( A ( b c ) ( Return( Add( b c ) ) ) ) Print( A ( 1 2 ) ) )"],
    ["let a=1;function A(b,c){return b+c;}if(a!=1){print A(2,3);}else{print A(4,5);}", "Program( Let( a 1 ) Function( A ( b c ) ( Return( Add( b c ) ) ) ) If( unEqual( a 1 ) ( Print( A ( 2 3 ) ) ) ( Print( A ( 4 5 ) ) ) ) )"], #関数と括弧
  ]
  print "﹥Parser "
  NOW_TEST.replace("Parser")
  run_test(testcases, [MY_LANG_EXE, '--parse'])
end

def test_interpreter
  testcases = [
    ["1+2-3*4/5", 0.5999999], # 四則計算
    ["-1+-2--3*-4/-5", -0.5999999], #四則負数
    ["1   + /*6*5/4-8*/  2-3//5+3", 0], #スペースと注釈
	["function A(b,c){return b+c;}print A(1,2);", 3],
    ["let a=1;function A(b,c){return b+c;}if(a!=1){print A(2,3);}else{print A(4,5);}", 9], #関数と括弧
	["let b=0; for(let a=0;a>=5;let a=a+1;){let b=b+1;}print b;", 5], #for loop
	["let a=0; while(a>=5){let a=a+1;} print a;", 5], #while loop
	["let a=0; do{let a=a+1;}while(a<=5);print a;", 1], #do-while loop
	["function fib(n) { if (n<=2) {return 1;}else{return fib(n-1)+fib(n-2);} } print fib(7);", 13],
	["function fib(n) { if (n<=2) {return 1;}else{return fib(n-1)+fib(n-2);} } print fib(25);", 75025],
  ]
  print "﹥Interpreter "
  NOW_TEST.replace("Interpreter")
  run_test(testcases, [MY_LANG_EXE])
end

puts "----Start------------"
test_tokenizer
test_parser
test_interpreter
if BUG_LIST.count != 0
  puts "--Bug--List----------"
  puts BUG_LIST
end
puts "-----End-------------"