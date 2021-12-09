using System;
using System.Collections.Generic;
using System.Text;

namespace MacroBotV0._1Language
{
    //Custom Errors-------------------------------------------------------
    public class CustomError
    {
        public string errorName;
        public string details;
        public Position pos_start;
        public Position pos_end;

        public CustomError(Position pos_start, Position pos_end, string errorName, string details)
        {
            this.pos_start = pos_start;
            this.pos_end = pos_end;
            this.errorName = errorName;
            this.details = details;
        }

        public override string ToString() =>
            $"{errorName}: {details}\n" +
            $"File {pos_start.fn}, line {pos_start.ln + 1}" +
            "\n\n" + StringsWithArrows.StringWithArrows(pos_start.ftxt, pos_start, pos_end);
    }

    public class IllegalCharError : CustomError
    {
        public IllegalCharError(Position pos_start, Position pos_end, string details) : base(pos_start, pos_end, "Illegal Character", details) { }
    }


    public class ExpectedCharError : CustomError
    {
        public ExpectedCharError(Position pos_start, Position pos_end, string details) : base(pos_start, pos_end, "Expected Character", details) { }
    }





    public class InvalidSyntaxError : CustomError
    {
        public InvalidSyntaxError(Position pos_start, Position pos_end, string details) : base(pos_start, pos_end, "Invalid Syntax", details) { }
    }

    public class RTError : CustomError
    {
        Context context;
        public RTError(Position pos_start, Position pos_end, string details, Context context) : base(pos_start, pos_end, "Runtime Error", details) {
            this.context = context;
        }

        public override string ToString() =>
            this.generateTraceback() +
           $"{errorName}: {details}" +
           "\n\n" + StringsWithArrows.StringWithArrows(pos_start.ftxt, pos_start, pos_end);

        private string generateTraceback()
        {
            string result = "";
            Position pos = pos_start;
            Context ctx = context;

            while(ctx != null)
            {
                result += $"File {pos.fn}, line {pos.ln + 1}, in {ctx.displayName}\n";
                pos = ctx.parentEtrPos;
                ctx = ctx.parent;
            }
            return "Traceback (most recent call last):\n" + result;
        }
    }

}
