using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Translator
{
    public class Startup
    {
        public static bool DEBUG = false;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("usage: Translator.exe <input file>");
                return;
            }
            String inputFileName = args[0];
	        String productionsFileName = "productions.txt";
            Parser parser = new Parser(inputFileName);

	        LexemaSequence input = parser.getSequence();

            SyntaxAnalyser analyser = new SyntaxAnalyser(input, productionsFileName);
	        if(analyser.Process() == false)
	        {
	            Console.WriteLine("Input sequence syntax is incorrect.");
	            return;
	        }
	        //
	        TranslationUnit tr_unit = new TranslationUnit(input);

            StreamWriter sw = new StreamWriter(new FileStream(inputFileName.Split(new char[]{'.'})[0]+".asm", FileMode.OpenOrCreate, FileAccess.Write));
            sw.Write(tr_unit.GetAsmCode());
            sw.Close();

        }        
    }
}
