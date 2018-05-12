using System;
using System.Collections.Generic;
using System.Text;

namespace Translator
{
    public class Lexema
    {
        public char symbol; //символ, скойто се означава типа на лексемата
        public int row; //номер на ред във входния файл
        public int address; //само за идентификатори
        public int value; //само за константи
        
        public const int NO_VALUE = -1;
        public const int NO_ADDRESS = -1;
        
        public Lexema(char fsymbol, int frow, int faddress, int fvalue)
        {
	        symbol = fsymbol;
	        row = frow;
	        address = faddress;
	        value = fvalue;
        }
        
        public override String ToString()
        {
            return symbol.ToString();
        }
    }
}
