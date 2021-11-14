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
            $"File {pos_start.fn}, line {pos_start.ln + 1}";
    }

    public class IllegalCharError : CustomError
    {
        public IllegalCharError(Position pos_start, Position pos_end, string errorName, string details) : base(pos_start, pos_end, "Illegal Character", details) { }
    }

    public class InvalidSyntaxError : CustomError
    {
        public InvalidSyntaxError(Position pos_start, Position pos_end, string errorName, string details = "") : base(pos_start, pos_end, "Invalid Syntax", details) { }
    }
}
