using System;
using System.Collections.Generic;

namespace MacroBotV0._1Language
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;// Need to do this because windows likes to use "," instead of "."


            while (true) // Basic console simulator
            {
                Console.Write("MacroScript > "); 
                string txt = Console.ReadLine();
               // Console.WriteLine(txt);

                TokensAndError tokensAndError = MainScript.Run("<TestFileName>" ,txt);
                if (tokensAndError.error.errorName != null)
                    Console.WriteLine(tokensAndError.error);
                else
                    Console.WriteLine(tokensAndError.tokens.ToDelimitedString());
            }
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
