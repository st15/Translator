using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Translator
{
    //този стек се използва при преобразуванията между Обратен Полски Запис и Инфиксен запис
    //съдържа лексвмите от входния поток
    public class RPNStack
    {
        private ArrayList lexemas;

        public RPNStack()
        {
            lexemas = new ArrayList();  //<Lexema>
        }

        public void push(Lexema lexema)
        {
            lexemas.Add(lexema);
        }

        public Lexema pop()
        {
            if (lexemas.Count > 0)
            {
                Lexema ret = (Lexema)lexemas[lexemas.Count - 1];
                lexemas.RemoveAt(lexemas.Count - 1);
                return ret;
            }
            else
                return null;
        }

        public Lexema peek()
        {
            if (lexemas.Count > 0)
                return (Lexema)lexemas[lexemas.Count - 1];
            else
                return null;
        }

        public void clear()
        {
            lexemas.Clear();
        }

        public override String ToString()
        {
            String lexemas_sequence = "RPNStack object: ";

            IEnumerator iter = lexemas.GetEnumerator();
            while (iter.MoveNext())
                lexemas_sequence += iter.Current.ToString();
            return lexemas_sequence;
        }
    }
}
