using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Translator
{
    public class LexemaSequence
    {
        private ArrayList sequence = new ArrayList(); //<Lexema>
        private int position;
        
        
        public char get()
        {
	        char symbol = peek();
            position++;
            return symbol;
        }
        
        public Lexema getLexema()
        {
	        Lexema lexema = peekLexema();
            position++;
            return lexema;
        }
        
        public Lexema getLexema(int pos)
        {
            return (Lexema)sequence[pos];
        }
        
        public Lexema peekLexema()
        {
	        if(position >= sequence.Count)
	            throw new Exception("InputSequence out of bounds.");
	        return (Lexema)sequence[position];
        }
        
        public char peek()
        {
	        if(position >= sequence.Count)
	            throw new Exception("InputSequence out of bounds.");
	        return ((Lexema)sequence[position]).symbol;
        }
        
        public void Add(char symbol)
        {
	        sequence.Add(new Lexema(symbol, 0, Lexema.NO_ADDRESS, Lexema.NO_VALUE));
        }

        public void Add(char symbol, int line, int address, int value)
        {
            sequence.Add(new Lexema(symbol, line, address, value));
        }
        
        public int GetPosition()
        {
	        return position;
        }
        
        
        public int size()
        {
	        return sequence.Count;
        }
        
        public void rewind()
        {
	     position = 0;
        }

        //връща последователността от лексеми от текущата позиция до края
        public override String ToString()
        {
	        String symbol_sequence = "";
	        int pos = 0;
	        IEnumerator iter = sequence.GetEnumerator();
	        while(iter.MoveNext())
	        {
	            Lexema lexema = (Lexema)iter.Current;
	            if(pos >= position)
		        symbol_sequence += lexema.symbol;
	            pos++;
	        }
	        return symbol_sequence;
        }
    }
}
