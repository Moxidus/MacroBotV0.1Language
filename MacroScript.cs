using System;
using System.Collections.Generic;
using System.Globalization;

public static class MainScript
{
    //digit constants
    public static string DIGITS = "0123456789";

    //tokens constants
    public static Token TT_INT = new Token("TT_INT");
    public static Token TT_FLOAT = new Token("TT_FLOAT");
    public static Token TT_PLUS = new Token("TT_PLUS");
    public static Token TT_MINUS = new Token("TT_MINUS");
    public static Token TT_MUL = new Token("TT_MUL");
    public static Token TT_DIV = new Token("TT_DIV");
    public static Token TT_LPAREN = new Token("TT_LPAREN");
    public static Token TT_RPAREN = new Token("TT_RPAREN");


    public static TokensAndError Run(string fn, string text)
    {
        Lexer lexer = new Lexer( fn, text);
        TokensAndError tokensAndError = lexer.make_tokens();
        return tokensAndError;
    }


}


//Custom Errors-------------------------------------------------------
public class CustomError{
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

    public override string ToString()
    {
        return errorName + ":" + details + "\nFile " + pos_start.fn + ", line " + (pos_start.ln + 1);
    }
}
//Position------------------------------------------------------------

public class Position
{
    public int idx;
    public int ln;
    public int col;
    public string fn;
    public string ftxt;

    public Position(int idx, int ln, int col, string fn, string ftxt)
    {
        this.idx = idx;
        this.ln = ln;
        this.col = col;
        this.fn = fn;
        this.ftxt = ftxt;
    }

    public Position advance(char? current_char)
    {
        idx++;
        col++;

        if(current_char == '\n')
        {
            ln++;
            col = 0;
        }
        return this;
    }

    public Position copy()
    {
        return new Position(idx, ln, col, fn, ftxt);
    }

}



//Structure for return------------------------------------------------
public struct TokensAndError{
    public List<Token> tokens;
    public CustomError error;

    public TokensAndError(List<Token> tokens, CustomError error)
    {
        this.tokens = tokens;
        this.error = error;
    }
}




//LEXER---------------------------------------------------------------

public class Lexer
{
    string text;
    Position pos;
    char? current_char;
    string fn;

    public Lexer(string fn, string text)
    {
        this.fn = fn;
        this.text = text;
        this.pos = new Position(-1, 0, -1, fn, text);
        this.current_char = null;
        advance();
    }
    void advance()
    {
        pos.advance(current_char);
        if (pos.idx < text.Length)
            current_char = text[pos.idx];
        else
            current_char = null;
    }

    Token make_number()
    {
        string numb_str = "";
        int dot_count = 0;

        while (current_char != null && (MainScript.DIGITS + ".").Contains((char)current_char))
        {
            if(current_char == '.')
            {
                if (dot_count == 1)
                    break;
                dot_count += 1;
                numb_str += ".";
            }
            else
            {
                numb_str += current_char;
            }
            advance();
        }

        if (dot_count == 0)
            return MainScript.TT_INT.return_new_with_value(Int32.Parse(numb_str));
        else
        {
            return MainScript.TT_FLOAT.return_new_with_value(float.Parse(numb_str));
        }

    }

     public TokensAndError make_tokens()
    {
        List<Token> tokens = new List<Token>();

        while(current_char != null)
        {
            if (!" \t".Contains((char)current_char))
            {
                switch (current_char)
                {
                    case '+':
                        tokens.Add(MainScript.TT_PLUS);
                        advance();
                        break;
                    case '-':
                        tokens.Add(MainScript.TT_MINUS);
                        advance();
                        break;
                    case '*':
                        tokens.Add(MainScript.TT_MUL);
                        advance();
                        break;
                    case '/':
                        tokens.Add(MainScript.TT_DIV);
                        advance();
                        break;
                    case '(':
                        tokens.Add(MainScript.TT_LPAREN);
                        advance();
                        break;
                    case ')':
                        tokens.Add(MainScript.TT_RPAREN);
                        advance();
                        break;
                    default:
                        if (MainScript.DIGITS.Contains((char)current_char))
                        {
                            tokens.Add(make_number());
                        }
                        else
                        {
                            Position pos_start = pos.copy();
                            char tempChar = (char)current_char;
                            advance();
                            return new TokensAndError(new List<Token>(), new CustomError(pos_start, pos, "Illegal Character", "'" + tempChar + "'"));
                        }
                        break;
                }
            }
            else
                advance();
        }

        return new TokensAndError(tokens, new CustomError(new Position(0, 0, 0, null, null), new Position(0, 0, 0, null, null), null, null));
    }



}



//TOKENS--------------------------------------------------------------

public class Token
{
    string type_;
    object value;

    public Token(string type_, object value = null)
    {
        this.type_ = type_;
        this.value = value;
    }

    public Token return_new_with_value(object val)
    {
        return new Token(type_, val);
    }

    public override string ToString()
    {
        if (value != null)
            return type_ + ":" + value.ToString();
        return type_;
    }

}
