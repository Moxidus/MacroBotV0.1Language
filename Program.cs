using System;
using System.Collections.Generic;
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


            while (true) // Basic console simulator
            {
                Console.Write("MacroScript > "); 
                string txt = Console.ReadLine();
                // Console.WriteLine(txt);

                (Node, CustomError) tokensAndError = MainScript.Run("<TestFileName>" ,txt);
                CustomError error = tokensAndError.Item2;
                Node Node = tokensAndError.Item1;

                if (error != null)
                    Console.WriteLine(error);
                else
                    Console.WriteLine(Node.ToString());
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
