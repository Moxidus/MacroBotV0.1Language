using MacroBotV0._1Language;
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



    //Run-------------------------------------------------------------
    public static (Node, CustomError) Run(string fn, string text)
    {
        //generate tokens
        Lexer lexer = new Lexer(fn, text);
        (List<Token>, CustomError) tokensAndError = lexer.make_tokens();
        List<Token> tokens = tokensAndError.Item1;
        //Console.WriteLine(tokens.ToDelimitedString()); //debug
        CustomError error = tokensAndError.Item2;
        if (error != null)
            return (null, error);

        //generate AST
        Parser parser = new Parser(tokens);
        Node ast = parser.parse();


        return (ast, null);
    }


}




//Nodes------------------------------------------------------------

public class Node
{
    protected Token tok;

    public Node(Token tok)
    {
        this.tok = tok;
    }

    public override string ToString() => tok.ToString();

}


public class NumberNode : Node
{

    public NumberNode(Token tok) : base(tok)
    {
    }

    public override string ToString() => tok.ToString();

}

public class BinOpNode : Node
{
    Node left_node;
    Node right_node;

    public BinOpNode(Node left_node, Token op_tok, Node right_node) : base(op_tok)
    {
        this.left_node = left_node;
        this.right_node = right_node;
    }

    public override string ToString() => $"({left_node}, {tok}, {right_node})";
}





//Parser----------------------------------------------------------
class Parser
{
    List<Token> tokens;
    int tok_idx;
    Token current_tok;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
        tok_idx = -1;
        advance();
    }

    Token advance()
    {
        tok_idx++;
        if (tok_idx < tokens.Count)
        {
            current_tok = tokens[tok_idx];
        }
        return current_tok;
    }

    //#################################################################
    public Node parse() => expr();


    NumberNode factor()
    {
        Token tok = current_tok;

        if (MainScript.TT_INT.type == tok.type || MainScript.TT_FLOAT.type == tok.type)
        {
            advance();
            return new NumberNode(tok);
        }
        return null;
    }

    Node term()
    {
        Node left = factor();

        while (current_tok.type == MainScript.TT_MUL.type || current_tok.type == MainScript.TT_DIV.type)
        {
            Token op_tok = current_tok;
            advance();
            NumberNode right = factor();
            left = new BinOpNode(left, op_tok, right);
        }
        return left;
    }

    Node expr()
    {
        Node left = term();

        while (current_tok.type == MainScript.TT_PLUS.type || current_tok.type == MainScript.TT_MINUS.type)
        {
            Token op_tok = current_tok;
            advance();
            Node right = term();
            left = new BinOpNode(left, op_tok, right);
        }
        return left;
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
            if (current_char == '.')
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

    public (List<Token>, CustomError) make_tokens()
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
                            return  (new List<Token>(), new IllegalCharError(pos_start, pos, "Illegal Character", "'" + tempChar + "'"));
                        }
                        break;
                }
            }
            else
                advance();
        }

        return (tokens, null);
    }



}



//TOKENS--------------------------------------------------------------

public class Token
{
    public string type;
    object value;

    public Token(string type_, object value = null)
    {
        this.type = type_;
        this.value = value;
    }

    public Token return_new_with_value(object val)
    {
        return new Token(type, val);
    }

    public override string ToString()
    {
        if (value != null)
            return type + ":" + value.ToString();
        return type;
    }

}
