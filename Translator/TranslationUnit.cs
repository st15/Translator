using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Translator
{
    public class TranslationUnit
    {
        private LexemaSequence input;
        private static Hashtable expressions;  //<Character, Expression>
        private static RPNStack stack = new RPNStack();
        private static String newLine = "\r\n";
        private String asmHeader;
        private String asmFooter;
        private String asmCode;
        private String returnDOS;
        private static int nextAvailableAddress = 396;
        private static Hashtable includedProcs = new Hashtable();  //<String, String>

        public TranslationUnit(LexemaSequence input_seq)
        {
            input = input_seq;
            loadExpressions();

            asmHeader =
                "STACK_SEG SEGMENT STACK" + newLine +
                "	DW 512 DUP(?)" + newLine +
                "STACK_SEG ENDS" + newLine +
                                newLine +
                "CODESC	SEGMENT 'CODE'" + newLine +
                "	ASSUME CS:CODESC, SS:STACK_SEG" + newLine +
                                newLine +
                "BEGIN:	.386" + newLine;
            returnDOS =
                "MOV AH, 4CH" + newLine +
                "INT 21H" + newLine;
            asmFooter =
                "CODESC	ENDS" + newLine +
                "   END BEGIN" + newLine;

            parseStatements();
        }

        public string GetAsmCode()
        {
            return this.asmCode;
        }

        private static void includeProc(String proc_code)
        {
            //ако процедурата вече не е включена се включва
            if (!includedProcs.ContainsKey(proc_code))
                includedProcs.Add(proc_code.ToString(), proc_code);
        }

        private String writeIncludedProcs()
        {
            String procs_code = "";
            IEnumerator iter = includedProcs.Values.GetEnumerator();
            while (iter.MoveNext())
                procs_code += iter.Current;
            return procs_code;
        }

        //взима следващия свободен адрес от стека на програмата,
        //където ще се съхраняват междинни резултати
        private static int getNextAvailableAddress()
        {
            //TODO: да работи заедно с дескрипторната таблица,
            //за да знае от кой адрес да започва да раздава
            nextAvailableAddress += 4;
            return nextAvailableAddress;
        }

        private static int getLastAddress()
        {
            return nextAvailableAddress;
        }

        private void checkUnaryMinus(LexemaSequence input, Lexema lexema)
        {
            if (lexema.symbol == Expression.MINUS)
            {
                bool must_change = false;
                if (input.GetPosition() > 1)
                {
                    char symbol = input.getLexema(input.GetPosition() - 2).symbol;
                    if ((symbol == Expression.ASSIGNMENT) ||
                        (symbol == Expression.OPEN_BRACE) ||
                        (symbol == Expression.PLUS) ||
                        (symbol == Expression.MULTUPLICATION) ||
                        (symbol == Expression.BITWISE_OR) ||
                        (symbol == Expression.DIVISION) ||
                        (symbol == Expression.MODUL) ||
                        (symbol == Expression.BITWISE_AND) ||
                        (symbol == Expression.BITWISE_INVERSION) ||
                        (symbol == Expression.BOOLEAN_INVERSION))
                        must_change = true;
                }
                else
                    must_change = true;
                if (must_change)
                    lexema.symbol = Expression.UNARY_MINUS;
            }
        }

        //открива и заменя постфиксни ++ и -- с други символи
        private bool incAndDecSymbolDeal(LexemaSequence input, Lexema lexema)
        {
            bool ret = true;
            ret = changeSymbol(input, lexema, Expression.PREFIX_INCREMENT, Expression.POSTFIX_INCREMENT);
            if (ret == true)
                ret = changeSymbol(input, lexema, Expression.PREFIX_DECREMENT, Expression.POSTFIX_DECREMENT);
            return ret;
        }

        private bool changeSymbol(LexemaSequence input, Lexema lexema, char search_symbol,
            char replace_symbol)
        {
            if (lexema.symbol == search_symbol)
            {
                if (input.GetPosition() > 1)
                {
                    if (input.getLexema(input.GetPosition() - 2).symbol == Expression.IDENTIFIER)
                    {
                        lexema.symbol = replace_symbol;
                        return true;
                    }
                }
                if (input.GetPosition() < input.size())
                {
                    if (input.peekLexema().symbol == Expression.IDENTIFIER)
                    {
                        //това е префиксна операция, не се прави заместване
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(search_symbol + " can be applyed only to identifier.");
                        return false;
                    }
                }
                Console.WriteLine("Error 1002.");
                return false;
            }
            else
                return true;
        }

        private void loadExpressions()
        {
            expressions = new Hashtable();  //<Character, Expression>

            expressions.Add(Expression.ASSIGNMENT,
                new Expression(Expression.ASSIGNMENT, -1, 0));
            expressions.Add(Expression.PRINTF,
                new Expression(Expression.PRINTF, 0, 0));
            expressions.Add(Expression.OPEN_BRACE,
                new Expression(Expression.OPEN_BRACE, -100, 100));
            expressions.Add(Expression.CLOSE_BRACE,
                new Expression(Expression.CLOSE_BRACE, Expression.NO_MATTER,
                Expression.NO_MATTER));
            expressions.Add(Expression.SEMICOLON,
                new Expression(Expression.SEMICOLON, Expression.NO_MATTER,
                Expression.NO_MATTER));
            expressions.Add(Expression.BITWISE_AND,
                new Expression(Expression.BITWISE_AND, 2, 2));
            expressions.Add(Expression.BITWISE_OR,
                new Expression(Expression.BITWISE_OR, 2, 2));
            expressions.Add(Expression.PLUS,
                new Expression(Expression.PLUS, 3, 3));
            expressions.Add(Expression.MINUS,
                new Expression(Expression.MINUS, 3, 3));
            expressions.Add(Expression.MULTUPLICATION,
                new Expression(Expression.MULTUPLICATION, 4, 4));
            expressions.Add(Expression.DIVISION,
                new Expression(Expression.DIVISION, 4, 4));
            expressions.Add(Expression.MODUL,
                new Expression(Expression.MODUL, 4, 4));
            expressions.Add(Expression.UNARY_MINUS,
                new Expression(Expression.UNARY_MINUS, 5, 5));
            expressions.Add(Expression.PREFIX_INCREMENT,
                new Expression(Expression.PREFIX_INCREMENT, 5, 5));
            expressions.Add(Expression.PREFIX_DECREMENT,
                new Expression(Expression.PREFIX_DECREMENT, 5, 5));
            expressions.Add(Expression.POSTFIX_INCREMENT,
                new Expression(Expression.POSTFIX_INCREMENT, 5, 5));
            expressions.Add(Expression.POSTFIX_DECREMENT,
                new Expression(Expression.POSTFIX_DECREMENT, 5, 5));
            expressions.Add(Expression.BITWISE_INVERSION,
                new Expression(Expression.BITWISE_INVERSION, 5, 5));
            expressions.Add(Expression.BOOLEAN_INVERSION,
                new Expression(Expression.BOOLEAN_INVERSION, 5, 5));
        }

        private bool parseStatements()
        {
            //входната поредица ще се обработва израз по израз (разделени са с ';')
            try
            {
                asmCode = asmHeader;
                input.rewind();
                int input_size = input.size();
                Statement stmt = new Statement();
                for (int i = 0; i < input_size; i++)
                {
                    Lexema lexema = input.getLexema();
                    if (lexema.symbol != Expression.SEMICOLON)
                    {
                        incAndDecSymbolDeal(input, lexema);
                        checkUnaryMinus(input, lexema);
                        if (stmt.convertIntoRPN(lexema) == false)
                            return false;
                    }
                    else
                    {
                        //прехвърля всичко от стека на изходната редица
                        ArrayList postfix_expressions = stmt.completeRPN(); //<Lexema>
                        //израза се преобразува в асемблерен код
                        asmCode += stmt.generateAsembler();
                        //добавя се израза, съдържащ само постфиксни оператори
                        if (postfix_expressions.Count != 0)
                        {
                            Statement postfix_stmt = new Statement(postfix_expressions);
                            //израза се преобразува в асемблерен код
                            asmCode += postfix_stmt.generateAsembler();
                        }
                        stmt = new Statement();
                    }
                }
                asmCode += returnDOS;
                asmCode += writeIncludedProcs();
                asmCode += asmFooter;
                Console.WriteLine("\nASSEMBLER CODE:\n" + asmCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return false;
            }
            return true;
        }

        private class Statement
        {
            private ArrayList stmt;
            //съдържа идентификаторите с постфиксни ++ и --
            private ArrayList postfix;
            //дали този Statement съдържа само постфиксни оператори
            private bool postfixStatement;
            //на върха на стека за ОПЗ => ИЗ ще се слага лексема, която
            //ще се означава с долния символ и показва, че това не е
            //истинска лексема, а междинен резултат
            private static readonly char intermediateLexemaSymbol = 'x';

            public Statement()
            {
                stmt = new ArrayList(); //<Lexema>
                postfix = new ArrayList();  //<Lexema>
                postfixStatement = false;
            }

            public Statement(ArrayList fstmt)
            {
                stmt = fstmt;
                postfixStatement = true;
            }

            public bool convertIntoRPN(Lexema lexema)
            {
                Expression expression_sign = (Expression)expressions[lexema.symbol];
                //ако поредния символ е операнд или константа го пишем направо на изходната редица
                if (expression_sign == null)
                    stmt.Add(lexema);
                else
                {
                    //това е оператор		
                    if (expression_sign.symbol == Expression.CLOSE_BRACE)
                    {
                        //затварящата скоба предизвиква изхвърляне на символи от стека
                        //докато се срещне отваряща скоба. Двете се унищожават.
                        //взима се лексемата от върха на стека
                        Lexema TOS_lexema = stack.peek();
                        //проверка за празен стек
                        if (TOS_lexema != null)
                        {
                            //стекът не е празен
                            Expression TOS_expression_sign = (Expression)expressions[TOS_lexema.symbol];
                            while (TOS_expression_sign.symbol != Expression.OPEN_BRACE)
                            {
                                //предизвиква се изхвърляне на символи от стека към изходната редица
                                TOS_lexema = stack.pop();
                                stmt.Add(TOS_lexema);
                                //поглежда се какъв е следващия символ от стека
                                TOS_lexema = stack.peek();
                                if (TOS_lexema != null)
                                    TOS_expression_sign = (Expression)expressions[TOS_lexema.symbol];
                                else
                                {
                                    //стекът вече е празен
                                    Console.WriteLine("ERROR 1000: " +
                                        "Can not find open brace while executing RPN");
                                    return false;
                                }
                            }
                            //премахва се отварящата скоба от стека
                            stack.pop();
                        }
                        else
                        {
                            Console.WriteLine("ERROR 1001: " +
                                "Can not find open brace while executing RPN");
                            return false;
                        }
                    }
                    else
                    {
                        //постфиксните ++ и -- се обработват по различен начин начин
                        if ((expression_sign.symbol == Expression.POSTFIX_INCREMENT) ||
                            (expression_sign.symbol == Expression.POSTFIX_DECREMENT))
                        {
                            //добавя се предишния идентификатор
                            postfix.Add(stmt[stmt.Count - 1]);
                            //добавя се и самата лексема
                            postfix.Add(lexema);
                        }
                        else
                        {
                            //обработват се останалите лексеми
                            //взима се лексемата от върха на стека
                            Lexema TOS_lexema = stack.peek();
                            //проверка за празен стек
                            if (TOS_lexema != null)
                            {
                                //стекът не е празен
                                Expression TOS_expression_sign = (Expression)expressions[TOS_lexema.symbol];
                                while (expression_sign.outOfStackPriority <= TOS_expression_sign.inStackPriority)
                                {
                                    //текъщия символ е с по-нисък или равен приоритет и ще
                                    //предизвика изхвърляне на символи от стека към изходната редица
                                    TOS_lexema = stack.pop();
                                    stmt.Add(TOS_lexema);
                                    //поглежда се какъв е следващия символ от стека
                                    TOS_lexema = stack.peek();
                                    if (TOS_lexema != null)
                                        TOS_expression_sign = (Expression)expressions[TOS_lexema.symbol];
                                    else
                                        break;	//стекът вече е празен
                                }
                            }
                            stack.push(lexema);
                        }
                    }
                    if (expression_sign.symbol == Expression.ASSIGNMENT)
                        if (assignmentSignDeal() == false)
                            return false;
                }
                return true;
            }

            private bool assignmentSignDeal()
            {
                //претърсват се символите от входната редица и ако има различни
                //от идентификатор или знак '=' значи има семантична грешка
                IEnumerator iter = stmt.GetEnumerator();    //<Lexema>
                while (iter.MoveNext())
                {
                    Lexema lexema = (Lexema)iter.Current;
                    if ((lexema.symbol != Expression.ASSIGNMENT) &&
                        (lexema.symbol != Expression.IDENTIFIER) &&
                        (lexema.symbol != Expression.OPEN_BRACE) &&
                        (lexema.symbol != Expression.CLOSE_BRACE))
                    {
                        Console.WriteLine("Left of assigment sign '=' can be only" +
                            " identifier.");
                        return false;
                    }
                }
                return true;
            }

            public ArrayList completeRPN()
            {
                //прехвърля всичко от стека на изходната редица
                while (stack.peek() != null)
                    stmt.Add(stack.pop());
                stack.clear();
                return postfix;
            }

            //генерира асмеблерен код за целия израз (statement)
            public String generateAsembler()
            {
                Asembler asm = new Asembler();
                String asmCode = "";
                IEnumerator iter = stmt.GetEnumerator();    //<Lexema>
                while (iter.MoveNext())
                {
                    Lexema lexema = (Lexema)iter.Current;
                    Expression expression_sign = (Expression)expressions[lexema.symbol];
                    //ако поредния символ е операнд го пишем в стека
                    if (expression_sign == null)
                        stack.push(lexema);

                    //това е оператор, може да е унарен или да работи с две стойности
                    Asembler.Result result = asm.processSingleOperation(lexema);
                    if (result != null)
                    {
                        asmCode += result.code;
                        if (result.savedIn == Asembler.Result.IN_SAVE_PLACE)
                        {
                            //индикация, че на тази позиция има нещо,
                            //чиято стойност е вече сметната
                            //и трябва да се вземе например от програмния стек
                            stack.push(new Lexema(intermediateLexemaSymbol, 0,
                                getLastAddress(), Lexema.NO_VALUE));
                        }
                    }
                }
                //TODO: провери дали стека е празен, защото ако не е празен и
                //израза не е бил само с постфиксни оператори значи има грешка
                stack.clear();
                Console.WriteLine("CODE FOR STATEMENT: " + this.ToString() + "\n" + asmCode + "\n");
                return asmCode;
            }

            private class Asembler
            {
                String savePlace = "dword ptr [ebp+{0}]";

                public class Result
                {
                    public String code;
                    public bool savedIn;

                    public const bool IN_MEMORY = true;
                    public const bool IN_SAVE_PLACE = false;

                    public Result(String fcode, bool finMemory)
                    {
                        code = fcode;
                        savedIn = finMemory;
                    }
                }

                private String getAddressOrValue(Lexema lexema)
                {
                    if (lexema.symbol == intermediateLexemaSymbol)
                    {
                        //това е on-the-fly стойност
                        return String.Format(savePlace, lexema.address);
                    }
                    else if (lexema.symbol == Expression.IDENTIFIER)
                    {
                        return String.Format("dword ptr [ebp+{0}]", lexema.address);
                    }
                    else if (lexema.symbol == Expression.NUMBER)
                    {
                        return lexema.value.ToString();
                    }
                    else
                        throw new Exception("Error 1005: no match found for symbol: " +
                            lexema.symbol);
                }

                //генерира асемблерен код за единичен оператор
                public Result processSingleOperation(Lexema lexema)
                {
                    switch (lexema.symbol)
                    {
                        case Expression.PLUS:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doPlus(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.ASSIGNMENT:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.peek();
                                return new Result(doAssignment(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_MEMORY);
                            }
                        case Expression.MINUS:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doMinus(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.PREFIX_INCREMENT:
                        case Expression.POSTFIX_INCREMENT:
                            {
                                Lexema only_side_lexema = stack.peek();
                                return new Result(doIncrement(getAddressOrValue(only_side_lexema)),
                                    Result.IN_MEMORY);
                            }
                        case Expression.PREFIX_DECREMENT:
                        case Expression.POSTFIX_DECREMENT:
                            {
                                Lexema only_side_lexema = stack.peek();
                                return new Result(doDecrement(getAddressOrValue(only_side_lexema)),
                                    Result.IN_MEMORY);
                            }
                        case Expression.BITWISE_INVERSION:
                            {
                                Lexema only_side_lexema = stack.pop();
                                return new Result(doBitwiseInversion(getAddressOrValue(only_side_lexema)),
                                    Result.IN_SAVE_PLACE);
                            }
                        case Expression.BITWISE_AND:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doBitwiseAnd(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.BITWISE_OR:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doBitwiseOr(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.BOOLEAN_INVERSION:
                            {
                                Lexema only_side_lexema = stack.pop();
                                return new Result(doBooleanInversion(getAddressOrValue(only_side_lexema)),
                                    Result.IN_SAVE_PLACE);
                            }
                        case Expression.UNARY_MINUS:
                            {
                                Lexema only_side_lexema = stack.pop();
                                return new Result(doUnaryMinus(getAddressOrValue(only_side_lexema)),
                                    Result.IN_SAVE_PLACE);
                            }
                        case Expression.MULTUPLICATION:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doMultiplication(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.DIVISION:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doDivision(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.MODUL:
                            {
                                Lexema right_side_lexema = stack.pop();
                                Lexema left_side_lexema = stack.pop();
                                return new Result(doModul(getAddressOrValue(right_side_lexema),
                                    getAddressOrValue(left_side_lexema)), Result.IN_SAVE_PLACE);
                            }
                        case Expression.PRINTF:
                            {
                                Lexema only_side_lexema = stack.pop();
                                return new Result(doPrintf(getAddressOrValue(only_side_lexema)),
                                    Result.IN_MEMORY);
                            }
                        case Expression.SCANF:
                            {
                                stack.pop();
                                return new Result(doScanf(),
                                    Result.IN_SAVE_PLACE);
                            }
                    }
                    return null;
                }

                private String getSavePlace()
                {
                    return String.Format(savePlace, getNextAvailableAddress());
                }

                private String doPlus(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "add    eax, {1}" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doAssignment(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "mov    {1}, eax" + newLine,
                        right_side, left_side);
                    return code;
                }

                private String doMinus(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "sub    eax, {1}" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doIncrement(String only_side)
                {
                    String code = String.Format(
                        "inc    {0}" + newLine,
                        only_side);
                    return code;
                }

                private String doDecrement(String only_side)
                {
                    String code = String.Format(
                        "dec    {0}" + newLine,
                        only_side);
                    return code;
                }

                private String doBitwiseInversion(String only_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "not    eax" + newLine +
                        "mov    {1}, eax" + newLine,
                        only_side, getSavePlace());
                    return code;
                }

                private String doBitwiseAnd(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "and    eax, {1}" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doBitwiseOr(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "or    eax, {1}" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doBooleanInversion(String only_side)
                {
                    String code = String.Format(
                        "xor    eax, eax" + newLine +
                        "mov    ebx, {0}" + newLine +
                        "cmp    ebx, 0" + newLine +
                        "sete   al" + newLine +
                        "mov    {1}, eax" + newLine,
                        only_side, getSavePlace());
                    return code;
                }

                private String doUnaryMinus(String only_side)
                {
                    String code = String.Format(
                        "mov    eax, 0" + newLine +
                        "sub    eax, {0}" + newLine +
                        "mov    {1}, eax" + newLine,
                        only_side, getSavePlace());
                    return code;
                }

                private String doMultiplication(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "imul   eax, {1}" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doDivision(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "mov    ebx, {1}" + newLine +
                        "cdq" + newLine +
                        "idiv   ebx" + newLine +
                        "mov    {2}, eax" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doModul(String right_side, String left_side)
                {
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "mov    ebx, {1}" + newLine +
                        "cdq" + newLine +
                        "idiv   ebx" + newLine +
                        "mov    {2}, edx" + newLine,
                        left_side, right_side, getSavePlace());
                    return code;
                }

                private String doPrintf(String only_side)
                {
                    //тази функция очаква параметър в регистър AX
                    String proc_code =
                        "PRINTF PROC" + newLine +
                        //проверка за отрицателно число
                        "	    MOV    EBX, EAX" + newLine +
                        "	    AND    EBX, 80000000H" + newLine +
                        "	    CMP    EBX, 0" + newLine +
                        "	    JZ	   M0" + newLine +

                        "	    MOV    ECX, EAX" + newLine +

                        "	    MOV    AH,	2" + newLine +
                        "	    MOV    DL, 2DH" + newLine +
                        "	    INT    21H" + newLine +

                        "	    MOV    EDX, 0" + newLine +
                        "	    SUB    EDX, ECX" + newLine +
                        "	    MOV    EAX, EDX" + newLine +

                        "M0:    MOV ECX, 0" + newLine +
                        "	    MOV EBX, 10" + newLine +
                        "M1:    MOV EDX, 0" + newLine +
                        "	    DIV EBX" + newLine +
                        "	    PUSH DX" + newLine +
                        "	    INC ECX" + newLine +
                        "	    CMP EAX, 0" + newLine +
                        "	    JNZ M1" + newLine +
                        "M2:    POP DX" + newLine +
                        "	    OR DL, 30H" + newLine +
                        "	    MOV AH,2" + newLine +
                        "	    INT 21h" + newLine +

                        "	    LOOP M2" + newLine +
                        ";print new line (LF, CR)" + newLine +
                        "   MOV DL, 0AH" + newLine +
                        "	MOV AH,2" + newLine +
                        "	INT 21h" + newLine +
                        "	MOV DL, 0DH" + newLine +
                        "	MOV AH,2" + newLine +
                        "	INT 21h" + newLine +
                        "	RET" + newLine +
                        "PRINTF ENDP" + newLine;
                    includeProc(proc_code);
                    String code = String.Format(
                        "mov    eax, {0}" + newLine +
                        "call   printf" + newLine,
                        only_side);
                    return code;
                }

                private String doScanf()
                {
                    //тази процедура връща резултат в регистър CX
                    String proc_code =
                        "SCANF  PROC" + newLine +
                        "MOV SI, 0	;флаг дали числото е отрицателно" + newLine +
                        "    MOV EBX, 10" + newLine +
                        "    MOV ECX, 0" + newLine +
                        "M21:MOV AH, 0" + newLine +
                        "    ;прочита се число от клавиатурата" + newLine +
                        "    PUSH EBX" + newLine +
                        "    MOV BH, AH" + newLine +
                        "    MOV AH, 1" + newLine +
                        "    INT 21H" + newLine +
                        "    MOV AH, BH" + newLine +
                        "    POP EBX" + newLine +
                        "    ;край на четенето от клавиатурата" + newLine +

                        "    CMP AL, 0DH	;дали е натиснат CR?" + newLine +
                        "    JE M22" + newLine +

                        "    ;проверка за въведен знак -" + newLine +
                        "    CMP AL, 2DH" + newLine +
                        "    JNE M20" + newLine +
                        "    MOV SI, 1	;числото е отрицателно" + newLine +
                        "    JMP M21" + newLine +

                        "M20:SUB AL, 30H	;преобразуване на ASCII цифрата в двоична" + newLine +
                        "    PUSH EAX	;съхраняване на последното въвеждане" + newLine +
                        "    MOV EAX, ECX" + newLine +
                        "    MUL EBX	;умножаване на десетичната сума с 10" + newLine +
                        "    POP ECX	;последното въвеждане се извлича обратно" + newLine +
                        "    ADD ECX, EAX	;в CX и се сумира" + newLine +
                        "    JMP M21" + newLine +
                        "    ;проверка дали числото е отрицателно (със знак - отпред)" + newLine +
                        "M22:CMP SI, 1" + newLine +
                        "    JNE M23" + newLine +
                        "    NEG ECX" + newLine +
                        ";print new line (LF, CR)" + newLine +
                        "M23:MOV DL, 0AH" + newLine +
                        "	MOV AH,2" + newLine +
                        "	INT 21h" + newLine +
                        "	MOV DL, 0DH" + newLine +
                        "	MOV AH,2" + newLine +
                        "	INT 21h" + newLine +
                        "	RET" + newLine +
                        "SCANF  ENDP" + newLine;
                    includeProc(proc_code);
                    String code = String.Format(
                        "call   scanf" + newLine +
                        "mov    {0}, ECX" + newLine,
                        getSavePlace());
                    return code;
                }
            }

            public override String ToString()
            {
                String lexemas_sequence = "Statement object: ";
                IEnumerator iter = stmt.GetEnumerator();    //<Lexema>
                while (iter.MoveNext())
                    lexemas_sequence += iter.Current.ToString();
                return lexemas_sequence + "\t" + postfixStatement;
            }
        }

    }
}
