using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Translator
{
    class Parser
    {
        private LexemaSequence lexSeq;
        private DescriptorTable descTable = new DescriptorTable();

        // парсваме в конструктора
        public Parser(String file_name)
        {
            if (File.Exists(file_name))
            {
                StreamReader file = null;
                try
                {
                    String line = null;
                    int line_number = 0;
                    file = new StreamReader(file_name);
                    lexSeq = new LexemaSequence();
                    while ((line = file.ReadLine()) != null)
                    {
                        line_number++;
                        ParseString(line, line_number);
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
                Console.WriteLine(file_name + " not found.");
            }

        }

        public LexemaSequence getSequence()
        {
            return lexSeq;
        }


        private void ParseString(String strInput, int line_number)
        {
            while (strInput.Length > 0)
            {
                Match identifier = Regex.Match(strInput, @"^[_A-Za-z][_A-Za-z0-9]*");
                if (identifier.Success)
                {
                    strInput = strInput.Remove(0, identifier.Value.Length);
                    if (identifier.Value.Equals("printf"))
                    {
                        lexSeq.Add(Expression.PRINTF, line_number, Lexema.NO_ADDRESS, Lexema.NO_VALUE);
                    }
                    else if (identifier.Value.Equals("scanf"))
                    {
                        lexSeq.Add(Expression.SCANF, line_number, Lexema.NO_ADDRESS, Lexema.NO_VALUE);
                    }
                    else //значи е променлива
                    {
                        lexSeq.Add(Expression.IDENTIFIER, line_number, descTable.Register(identifier.Value), Lexema.NO_VALUE);
                    }
                }
                else
                {
                    Match num = Regex.Match(strInput, @"^[0-9]+");
                    if (num.Success)
                    {
                        strInput = strInput.Remove(0, num.Value.Length);
                        lexSeq.Add(Expression.NUMBER, line_number, Lexema.NO_ADDRESS, Int32.Parse(num.Value));
                    }

                    else
                    {
                        Match delimiter = Regex.Match(strInput, @"^\s+");
                        if (delimiter.Success)
                        {
                            strInput = strInput.Remove(0, delimiter.Value.Length);
                        }
                        else
                        {
                            Match incr = Regex.Match(strInput, @"^\+\+");
                            if (incr.Success)
                            {
                                strInput = strInput.Remove(0, incr.Value.Length);
                                lexSeq.Add(Expression.PREFIX_INCREMENT, line_number, Lexema.NO_ADDRESS, Lexema.NO_VALUE);
                            }
                            else
                            {
                                Match decr = Regex.Match(strInput, @"^\-\-");
                                if (decr.Success)
                                {
                                    strInput = strInput.Remove(0, decr.Value.Length);
                                    lexSeq.Add(Expression.PREFIX_DECREMENT, line_number, Lexema.NO_ADDRESS, Lexema.NO_VALUE);
                                }
                                else
                                {

                                    Match operation = Regex.Match(strInput, @"^[=\|\+\-\*\/%~();&!]");
                                    if (operation.Success)
                                    {
                                        strInput = strInput.Remove(0, operation.Value.Length);
                                        lexSeq.Add(operation.Value[0], line_number, Lexema.NO_ADDRESS, Lexema.NO_VALUE);

                                    }
                                    else
                                        Console.WriteLine("Error");

                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
