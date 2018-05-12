# Translator

Translator from a Simple C-like language to assembler.


Азбука
------

 Letter = 'a'..'z' | 'A'..'Z' | '_'.
 Digit = '0'..'9'.
 Space = \t | \n | \r | ' '.
 AnyChar = ' '..\0xff.

Лексеми
-------

  Number = Digit {Digit}.
  Ident = Letter {Letter | Digit}.
  Delimiter = Space
  Keyword = 'scanf' | 'printf'.
  SpecialSymbol = '=' | 
                  '(' | ')' | ';' | '+' | '-' | '*' | 
                  '|' | '/' | '%' | '&' | '~' | '++' | '--'.
  OtherSymbol = ...
Граматика
---------

[1] Program = {Statement}.
[2] Statement = [Expression] ';'.
[3] Expression = BitwiseAndExpression {'|' BitwiseAndExpression}.
[4] BitwiseAndExpression = AdditiveExpression {'&' AdditiveExpression}.
[5] AdditiveExpression = MultiplicativeExpression {('+' | '-') MultiplicativeExpression}.
[6] MultiplicativeExpression = PrimaryExpression {('*' | '/' | '%') PrimaryExpression}.
[7] PrimaryExpression = Ident ['=' Expression] | '~' PrimaryExpression | '++' Ident | '--' Ident | Ident '++' | Ident '--' | 
						Number | PrintFunc | ScanfFunc | '(' Expression ')'.
[8] PrintFunc = 'printf' '(' Expression ')'.
[9] ScanfFunc = 'scanf' '(' ')'.

---------------------
Граматиката като от драгон бук:

Program -> Statements
Statements -> Statements Statement | e

Statement -> Expression ; | ;

Expression -> BAndExpressions
BAndExpressions -> BAndExpressions '|' BitwiseAndExpression | BitwiseAndExpression 

BitwiseAndExpression = AddExpressions
AddExpressions -> AddExpressions '&' AdditiveExpression | AdditiveExpression

AdditiveExpression -> MExpressions
MExpressions -> MExpressions '+' MultiplicativeExpression | 
		MExpressions '-' MultiplicativeExpression | 
		MultiplicativeExpression

MultiplicativeExpression -> PExpressions
PExpressions  -> PExpressions '*' PrimaryExpression |  
		PExpressions '/' PrimaryExpression |  
		PExpressions '%' PrimaryExpression | 
		PrimaryExpression

PrimaryExpression -> Ident | Ident '=' Expression | '~' PrimaryExpression | '++' Ident | '--' Ident | 
			Ident '++' | Ident '--' | Number | PrintFunc | ScanfFunc | '(' Expression ')'
PrintFunc -> 'printf' '(' Expression ')'
ScanfFunc -> 'scanf' '(' ')'
-----------------------
-----------------------
Таблици
-------

  Дефинирани Идентификатори (Name, Value, ...)

  Вградени типове и функции:
    типове    { int }

Примери
-------

A = scanf();
B = A*2;
printf(B);


A = scanf();
B = scanf();
printf(A+B);

A = scanf();
B = scanf();
C = (A*A+B*B)*2;
printf(C);

A = scanf();
B = scanf();
A++;
C = A+B;
printf(C);
