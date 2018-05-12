using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Translator
{
    class DescriptorTable
    {
        private Hashtable ht = new Hashtable();
        private int nextvalue = 0; //адрес в паметта спрямо bp на стека

        // регистрира променлива в таблицата на дескрипторите
        // ако вече съществува в таблицата, връща относителния й адрес в паметта
        public int Register(string str)
        {
            if (ht.ContainsKey(str))
            {
                return (int)ht[str];
            }
            else
            {
                nextvalue = nextvalue + 4;
                ht.Add(str, nextvalue);

                return nextvalue;
            }

        }

    }
}
