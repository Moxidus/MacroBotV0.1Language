using MacroBotV0._1Language;
using System;
using System.Collections.Generic;
using System.Globalization;

public static class MainScript
{
    //digit constants
    public static string DIGITS = "0123456789";

    //tokens constants
    public static string TT_INT = "TT_INT";
    public static string TT_FLOAT = "TT_FLOAT";
    public static string TT_PLUS = "TT_PLUS";
    public static string TT_MINUS = "TT_MINUS";
    public static string TT_MUL = "TT_MUL";
    public static string TT_DIV = "TT_DIV";
    public static string TT_LPAREN = "TT_LPAREN";
    public static string TT_RPAREN = "TT_RPAREN";
    public static string TT_EOF = "TT_EOF";



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
        ParseResult ast = parser.parse();


        return (ast.node, ast.error);
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

public class UnaryOpNode : Node
{
    Node node;

    public UnaryOpNode(Token op_tok, Node node) : base(op_tok)
    {
        this.node = node;
    }

    public override string ToString() => $"({tok}, {node})";
}


//parse result----------------------------------------------------

class ParseResult
{
    public CustomError error;
    public Node node;

    public ParseResult(){
    }

    public Node register(object res) {
        if(res is ParseResult)
        {
            ParseResult parRes = (ParseResult)res;
            if(parRes.error != null)
                error = parRes.error;
            return parRes.node;
        }
        if(res is Node)
        {
            Node node = (Node)res;
            return node;//might be a problem
        }
        return null;
    }
    public ParseResult success(Node node) {
        this.node = node;
        return this;
    }
    public ParseResult failure(CustomError error) {
        this.error = error;
        return this;
    }

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
    public ParseResult parse()
    {
        ParseResult res = expr();
        if(res.error == null && this.current_tok.type != MainScript.TT_EOF)
        {
            return res.failure(new InvalidSyntaxError(
                this.current_tok.posStart,
                this.current_tok.posEnd,
                "Expected '+', '-', '*' or '/'"));
        }
        return res;
    }


    ParseResult factor()
    {
        ParseResult res = new ParseResult();
        Token tok = current_tok;

        if( tok.type == MainScript.TT_MINUS || tok.type == MainScript.TT_PLUS)
        {
            res.register(advance());
            Node factorTemp = res.register(factor());
            if (res.error != null)
                return res;
            return res.success(new UnaryOpNode(tok, factorTemp));

        }
        else if (MainScript.TT_INT == tok.type || MainScript.TT_FLOAT == tok.type)
        {
            res.register(advance());//might be a problem...
            return res.success(new NumberNode(tok));
        } else if( tok.type == MainScript.TT_LPAREN)
        {
            res.register(advance());
            Node exprTemp = res.register(expr());
            if (res.error != null)
                return res;
            if(current_tok.type == MainScript.TT_RPAREN)
            {
                res.register(advance());
                return res.success(exprTemp);
            }
            else
            {
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected ')'"));
            }
        }


        return res.failure(
            new InvalidSyntaxError(
                tok.posStart,
                tok.posEnd,
                "Expected int or float"));
    }

    ParseResult term()
    {
        ParseResult res = new ParseResult();
        Node left = res.register(factor());
        if (res.error != null)
            return res;

        while (current_tok.type == MainScript.TT_MUL || current_tok.type == MainScript.TT_DIV)
        {
            Token op_tok = current_tok;
            res.register(advance());
            Node right = res.register(factor());
            if (res.error != null)
                return res;
            left = new BinOpNode(left, op_tok, right);
        }
        return res.success(left);
    }

    ParseResult expr()
    {
        ParseResult res = new ParseResult();
        Node left = res.register(term());
        if (res.error != null)
            return res;

        while (current_tok.type == MainScript.TT_PLUS || current_tok.type == MainScript.TT_MINUS)
        {
            Token op_tok = current_tok;
            res.register(advance());
            Node right = res.register(term());
            if (res.error != null)
                return res;
            left = new BinOpNode(left, op_tok, right);
        }
        return res.success(left);
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

    public Position advance(char? current_char = null)
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
        Position posStart = pos.copy();

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
            return new Token(MainScript.TT_INT, Int32.Parse(numb_str), posStart, this.pos);
        else
        {
            return new Token(MainScript.TT_FLOAT, float.Parse(numb_str), posStart, this.pos);
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
                        tokens.Add(new Token(MainScript.TT_PLUS, posStart: this.pos));
                        advance();
                        break;
                    case '-':
                        tokens.Add(new Token(MainScript.TT_MINUS, posStart: this.pos));
                        advance();
                        break;
                    case '*':
                        tokens.Add(new Token(MainScript.TT_MUL, posStart: this.pos));
                        advance();
                        break;
                    case '/':
                        tokens.Add(new Token(MainScript.TT_DIV, posStart: this.pos));
                        advance();
                        break;
                    case '(':
                        tokens.Add(new Token(MainScript.TT_LPAREN, posStart: this.pos));
                        advance();
                        break;
                    case ')':
                        tokens.Add(new Token(MainScript.TT_RPAREN, posStart: this.pos));
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
                            return  (new List<Token>(), new IllegalCharError(pos_start, pos, "'" + tempChar + "'"));
                        }
                        break;
                }
            }
            else
                advance();
        }

        tokens.Add(new Token(MainScript.TT_EOF, posStart: this.pos));
        return (tokens, null);
    }



}



//TOKENS--------------------------------------------------------------

public class Token
{
    public string type;
    object value;
    public Position posStart;
    public Position posEnd;

    public Token(string type, object value = null, Position posStart = null, Position posEnd = null)
    {
        this.type = type;
        this.value = value;

        if (posStart != null)
        {
            this.posStart = posStart.copy();
            this.posEnd = posStart.copy();
            this.posEnd.advance();
        }
        if (posEnd != null)
            this.posEnd = posEnd.copy();
    }

    public override string ToString()
    {
        if (value != null)
            return type + ":" + value.ToString();
        return type;
    }

}
