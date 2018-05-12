using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.IO;

namespace Translator
{
    class SyntaxAnalyser
    {
        private Hashtable productionsTable = new Hashtable();   //<char, Hashtable<char, string>>
        
        private String stack; //съдържа междинните терм. и нетерм. символи при прилагане на продукциите
        private String productionsFileName = "productions.txt";
        private String elementsSeparator = " ";
        private char startSymbol;
        private char stopSymbol;
        private char emptyProduction;
        private LexemaSequence input;

        public SyntaxAnalyser(LexemaSequence inputSequence, String filName)
        {
            input = inputSequence;
	        productionsFileName = filName;
        }

        public bool Process()
        {
            if(ReadProductionsTable())
                DebugProductionsTAble();
            else
                return false;
            PreAnalyze();
            return Analyze();
        }

        private void PreAnalyze()
        {
            //в стека се слага символа за дъно и стартовия нетерминален символ на граматиката
            ClearStack();
            PushIntoStack(stopSymbol);
            PushIntoStack(startSymbol);
            //към входната редица се добавя символа за край (това не е ';')
            input.Add(stopSymbol);
        }

        private bool Analyze()
        {
	        PrintStackAndInput();
	        while(true)
	        {
	            PrintStackAndInput();
	            char stack_symb = PopFromStack();
	            if(stack_symb == emptyProduction)
		            continue;
                Lexema lexema = input.peekLexema();
                char seqence_symb = lexema.symbol;
	            if(stack_symb == seqence_symb)
	            {
		            if(stack_symb == stopSymbol)
		            {
		                Console.WriteLine("String is perfect.");
		                return true;
		            }
		            else
		                input.get();	//отстранява се следващия символ от потока
                }
                else
                {
		            //предполага се, че символа, взет от стека, е нетерминален
                    //търсене на нетерм. символ в table_entry
		            Hashtable table_entry = (Hashtable)productionsTable[stack_symb]; 
		            if(table_entry != null)
		            {
                        //търсене на продукция за замяна на нетерм. символ
		                String production = (String)table_entry[seqence_symb];
		                if(production != null)
		                {
			                //към стека се добавя нужната продукция
			                PushIntoStack(production);
			                PrintProduction(stack_symb, production);
		                }
		                else
		                {
			                //не съществува продукция, за да получим stack_symb => seqence_symb
			                SyntaxError(String.Format("Не съществува продукция {0} => {1}", 
                                stack_symb, seqence_symb), lexema.row);
			                return false;
		                }
		            }
		            else
		            {
		                //няма такъв нетерминален символ или това е терминален символ,
		                //различен от този във входната редица
		                SyntaxError(String.Format("Символът от стека '{0}' е терминален " +
			                "и не съвпада със символа от входната редица: '(1)' " +
			                "или е нетерминален и не образува продукции", stack_symb, seqence_symb), lexema.row);
		                return false;
		            }
	            }
	        }
        }
        
        private void SyntaxError(String expl, int row)
        {
	        Console.WriteLine("Syntax error <"+row+">: "+expl);
        }
        
        private void PrintStackAndInput()
        {
            if (Startup.DEBUG == false)
                return;
	        Console.Write('\n' + stack + '\t' + input);
        }
        
        private void PrintProduction(char non_terminal, String production)
        {
            if (Startup.DEBUG == false)
                return;
	        Console.Write("\t{0} => {1}", non_terminal, production);
        }
        
        private void ClearStack()
        {
            stack = "";
        }
        
        private void PushIntoStack(String str)
        {
            stack += Reverse(str);
        }
        
        private void PushIntoStack(char symb)
        {
            PushIntoStack(symb.ToString());
        }
        
        private char PopFromStack()
        {
            char last_elem = stack[stack.Length-1];
            stack = stack.Substring(0, stack.Length-1);
            return last_elem;
        }
        
        private String Reverse(String str)
        {
            char[] str_arr = str.ToCharArray();
            Array.Reverse(str_arr);
            return new String(str_arr); 
        }
        
        private void DebugProductionsTAble()
        {
            if (Startup.DEBUG == false)
                return;
            IEnumerator iter = productionsTable.Keys.GetEnumerator();
            Console.WriteLine("productionsTable elements => {0}", productionsTable.Count);
            while(iter.MoveNext())
            {
                char key = (char)iter.Current;
                Hashtable elem = (Hashtable)productionsTable[key];
                Console.WriteLine("For key: {0} => {1} elements.", key, elem.Count);
            }
        }
        
        private bool ReadProductionsTable()
        {
            bool result = true;
            const int NON_TERMINAL = 0;
            const int TERMINAL = 1;
            const int PRODUCTION = 2;

            const int START_SYMBOL = 0;
            const int STOP_SYMBOL = 1;
            const int EMPTY_PRODUCTION_SYMBOL = 2;

            String line;
            int line_number = 0;
            //отваря се файла с таблицата с продукциите за четене
            //чете се по редове
            if (File.Exists(productionsFileName))
            {
                StreamReader file = null;
                try
                {
                    file = new StreamReader(productionsFileName);
                    while ((line = file.ReadLine()) != null)
                    {
                        //реда се разделя на части
                        String[] table_line = line.Split(new String[] { elementsSeparator },
                            StringSplitOptions.RemoveEmptyEntries);
                        line_number++;
                        if (line_number == 1)
                        {
                            startSymbol = table_line[START_SYMBOL][0];
                            stopSymbol = table_line[STOP_SYMBOL][0];
                            emptyProduction = table_line[EMPTY_PRODUCTION_SYMBOL][0];
                        }
                        else
                        {
                            if (table_line.Length == 3)
                            {
                                Hashtable table_entry = (Hashtable)productionsTable[table_line[NON_TERMINAL][0]];
                                //попълва се таблицата
                                if (table_entry == null)
                                {
                                    table_entry = new Hashtable();
                                    productionsTable.Add(table_line[NON_TERMINAL][0], table_entry);
                                }
                                table_entry.Add(table_line[TERMINAL][0], table_line[PRODUCTION]);
                            }
                            else
                            {
                                SyntaxError(productionsFileName +" syntax invalid." ,line_number);
                                result = false;
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    if (file != null)
                        file.Close();
                }
            }
            else
            {
                Console.WriteLine(productionsFileName + " not found.");
                result = false;
            }
            return result;
        }
    }
}
