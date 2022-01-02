using System;
using System.Collections.Generic;
using System.IO;
using static MainScript;

namespace MacroBotV0._1Language
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;// Need to do this because windows likes to use "," instead of "."


#if RELEASE
            string txt = "";
            string fileName = "<none>";
            if (args.Length == 2 && args[0] == "-f")
            {
                string path = Path.GetFullPath(args[1]);

                txt = System.IO.File.ReadAllText(args[1]);
                fileName = $"<{Path.GetFileName(path)}>";
            }
            else if (args.Length == 2 && args[0] == "-t")
            {
                txt = args[1];
            }

            if (txt == "")
                return;

            txt = txt.Replace(System.Environment.NewLine, "\n");//Raplaces <CR> with new line

            Console.WriteLine(txt);
            //return;
            (ValueF, CustomError) tokensAndError = MainScript.Run(fileName, txt);
            CustomError error = tokensAndError.Item2;
            ValueF result = tokensAndError.Item1;

            if (error != null)
                Console.WriteLine(error);
            else if (result != null)
                Console.WriteLine(result.ToString());
#endif
            
#if DEBUG
            for(int i=0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }
            while (true) // Basic console simulator
            {
                Console.Write("MacroScript > "); 
                string txt = Console.ReadLine();
                if (txt.Trim() == "") continue;
                // Console.WriteLine(txt);

                (ValueF, CustomError) tokensAndError = MainScript.Run("<TestFileName>" ,txt);
                CustomError error = tokensAndError.Item2;
                ValueF result = tokensAndError.Item1;

                if (error != null)
                    Console.WriteLine(error);
                else if(result != null)
                    Console.WriteLine(result.ToString());
            }

#endif

        }
    }
    
    public static class MyExtensionMethods
    {
        public static string ToDelimitedString(this List<Token> source)
        {
            string temp = "[";
            source.ForEach(x => temp += x + ", ");
            temp = temp.Remove(temp.Length - 2);
            temp += "]";
            return temp;
        }
    }

}
