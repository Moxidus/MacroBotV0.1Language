using System;
using System.Collections.Generic;
using System.IO;
using static MainScript;
using Emgu.CV;
using Emgu.CV.Structure;

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

            List<string> assetPaths = new List<string>();

            if (args.Length >= 2 && args[0] == "-f")
            {
                string path = Path.GetFullPath(args[1]);

                txt = System.IO.File.ReadAllText(args[1]);
                fileName = $"<{Path.GetFileName(path)}>";
            }
            else if (args.Length >= 2 && args[0] == "-t")
            {
                txt = args[1];
            }

            if (args.Length >= 4 && args[2] == "-a")
            {
                for (int i = 3; i < args.Length; i++)
                {
                    assetPaths.Add(args[i]);
                }
            }

            List<Image<Bgr, byte>> assetsImages = new List<Image<Bgr, byte>>();

            assetPaths.ForEach(x => Console.WriteLine(x));
            assetPaths.ForEach(x => assetsImages.Add(new Image<Bgr, byte>(x)));






            if (txt == "")
                return;

            txt = txt.Replace(System.Environment.NewLine, "\n");//Raplaces <CR> with new line

            Console.WriteLine(txt);
            //return;

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

            for (int i=0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
            }
            if(args.Length > 0)
            {


                string txt = "";
                string fileName = "<none>";

                List<string> assetPaths = new List<string>();

                if (args.Length >= 2 && args[0] == "-f")
                {
                    string path = Path.GetFullPath(args[1]);

                    txt = System.IO.File.ReadAllText(args[1]);
                    fileName = $"<{Path.GetFileName(path)}>";
                }
                else if (args.Length >= 2 && args[0] == "-t")
                {
                    txt = args[1];
                }

                if (args.Length >= 4 && args[2] == "-a")
                {
                    for (int i = 3; i < args.Length; i++)
                    {
                        assetPaths.Add(args[i]);
                    }
                }

                List<Image<Bgr, byte>> assetsImages = new List<Image<Bgr, byte>>();

                assetPaths.ForEach(x => Console.WriteLine(x));
                assetPaths.ForEach(x => assetsImages.Add(new Image<Bgr, byte>(x)));






                if (txt == "")
                    return;

                txt = txt.Replace(System.Environment.NewLine, "\n");//Raplaces <CR> with new line

                Console.WriteLine(txt);
                //return;

                //return;
                (ValueF, CustomError) tokensAndError = MainScript.Run(fileName, txt);
                CustomError error = tokensAndError.Item2;
                ValueF result = tokensAndError.Item1;

                if (error != null)
                    Console.WriteLine(error);
                else if (result != null)
                    Console.WriteLine(result.ToString());


            }

            if (args.Length > 0) return;
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
