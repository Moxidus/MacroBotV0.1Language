using System;
using System.Collections.Generic;
using System.Text;

namespace MacroBotV0._1Language
{
    public static class StringsWithArrows
    {
        public static string StringWithArrows(string text, Position posStart, Position posEnd)
        {
            string result = "";

            // Calculate indicies
            int idxStart = Math.Max(text.Substring(0, posStart.idx).LastIndexOf("\n"), 0);
            int idxEnd = text.IndexOf("\n", idxStart + 1);
            if (idxEnd < 0)
                idxEnd = text.Length;

            //####################################
            //Console.WriteLine(idxStart.ToString() + ":" + idxEnd.ToString());
            //####################################

            //generate each line
            int lineCount = posEnd.ln - posStart.ln + 1;



            //####################################
            //Console.WriteLine(lineCount);
            //####################################

            for (int i = 0; i < lineCount; i++)
            {
                //calculate line columns 
                string line = text.Substring(idxStart, idxEnd - idxStart);
                int colStart = 0;
                if (i == 0)
                    colStart = posStart.col;

                int colEnd = line.Length - 1;
                if (i == lineCount - 1)
                    colEnd = posEnd.col;



                //append to result
                result += line + "\n";
                result += new string(' ', colStart) + new string('^', (colEnd - colStart));

                //recalculate indices
                idxStart = idxEnd;
                idxEnd = text.IndexOf("\n", idxStart);
                if (idxEnd < 0)
                    idxEnd = text.Length;

            }

            return result.Replace("\t", "");
        } 

    }
}
