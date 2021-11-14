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
            int idxStart = Math.Max(text.LastIndexOf("\n", 0, posStart.idx), 0);
            int idxEnd = text.IndexOf("\n", idxStart + 1);
            if (idxEnd < 0)
                idxEnd = text.Length;

            //generate each line
            int lineCount = posEnd.ln - posStart.ln + 1;
            for(int i = 0; i < lineCount; i++)
            {
                //calculate line columns 
                string line = text.Substring(idxStart, idxStart - idxEnd);
            }

            return result;
        } 

    }
}
