using System;
using System.Collections.Generic;
using System.Text;

namespace Translator
{
    public class Expression
    {
        public char symbol;
        public int inStackPriority;
        public int outOfStackPriority;

        public const char ASSIGNMENT = '=';
        public const char SCANF = 'a';
        public const char PRINTF = 'p';
        public const char IDENTIFIER = 'n';
        public const char NUMBER = 'b';
        public const char OPEN_BRACE = '(';
        public const char CLOSE_BRACE = ')';
        public const char BITWISE_AND = '&';
        public const char BITWISE_OR = '|';
        public const char BITWISE_INVERSION = '~';
        public const char BOOLEAN_INVERSION = '!';
        public const char PLUS = '+';
        public const char MINUS = '-';
        public const char UNARY_MINUS = 'z';
        public const char MULTUPLICATION = '*';
        public const char DIVISION = '/';
        public const char MODUL = '%';
        public const char PREFIX_INCREMENT = 'i';
        public const char POSTFIX_INCREMENT = 'w';
        public const char PREFIX_DECREMENT = 'd';
        public const char POSTFIX_DECREMENT = 'v';
        public const char SEMICOLON = ';';

        public static readonly int NO_MATTER = -1000000;

        public Expression(char s, int i, int o)
        {
            symbol = s;
            inStackPriority = i;
            outOfStackPriority = o;
        }
    }
}
