using MacroBotV0._1Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;

public static class MainScript
{
    //digit constants
    public static string DIGITS = "0123456789";
    public static string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
    public static string LETTERS_DIGITS = LETTERS + DIGITS;

    //tokens constants
    public static string TT_INT = "TT_INT";
    public static string TT_FLOAT = "TT_FLOAT";
    public static string TT_IDENTIFIER = "TT_IDENTIFIER";
    public static string TT_KEYWORD = "TT_KEYWORD";
    public static string TT_PLUS = "TT_PLUS";
    public static string TT_MINUS = "TT_MINUS";
    public static string TT_MUL = "TT_MUL";
    public static string TT_DIV = "TT_DIV";
    public static string TT_POW = "TT_POW";
    public static string TT_EQ = "TT_EQ";
    public static string TT_LPAREN = "TT_LPAREN";
    public static string TT_RPAREN = "TT_RPAREN";
    public static string TT_EOF = "TT_EOF";


    public static string[] KEYWORDS = { "VAR" };



    //Run-------------------------------------------------------------
    public static (Number, CustomError) Run(string fn, string text)
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
        if (ast.error != null)
            return (null, ast.error);

        // Run program
        Interpreter interpreter = new Interpreter();
        Context context = new Context("<Program>");
        RTResult result = interpreter.Visit(ast.node, context);


        return (result.value, result.error);
    }


}



//RunTime Result--------------------------------------------------
class RTResult
{
    public Number value;
    public CustomError error;

    public Number register(RTResult res)
    {
        if (res.error != null)
            error = res.error;
        return res.value;
    }

    public RTResult success(Number value)
    {
        this.value = value;
        return this;
    }

    public RTResult failure(CustomError error)
    {
        this.error = error;
        return this;
    }

}



//Nodes------------------------------------------------------------

public class Node
{
    public Token tok;
    public Position PosStart;
    public Position PosEnd;

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
        this.PosStart = tok.posStart;
        this.PosEnd = tok.posEnd;
    }

    public override string ToString() => tok.ToString();

}

public class BinOpNode : Node
{
    public Node left_node;
    public Node right_node;

    public BinOpNode(Node left_node, Token op_tok, Node right_node) : base(op_tok)
    {
        this.left_node = left_node;
        this.right_node = right_node;
        this.PosStart = left_node.PosStart;
        this.PosEnd = right_node.PosEnd;
    }

    public override string ToString() => $"({left_node}, {tok}, {right_node})";
}

public class UnaryOpNode : Node
{
    public Node node;

    public UnaryOpNode(Token op_tok, Node node) : base(op_tok)
    {
        this.node = node;

        this.PosStart = op_tok.posStart;
        this.PosEnd = node.PosEnd;
    }

    public override string ToString() => $"({tok}, {node})";
}


public class VarAssignNode : Node//TODO: rest
{
    public Node node;

    public VarAssignNode(Token op_tok, Node node) : base(op_tok)
    {
        this.node = node;

        this.PosStart = op_tok.posStart;
        this.PosEnd = node.PosEnd;
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
        if (res.error == null && this.current_tok.type != MainScript.TT_EOF)
        {
            return res.failure(new InvalidSyntaxError(
                this.current_tok.posStart,
                this.current_tok.posEnd,
                "Expected '+', '-', '*' or '/'"));
        }
        return res;
    }

    ParseResult atom(){
        ParseResult res = new ParseResult();
        Token tok = current_tok;

        if (MainScript.TT_INT == tok.type || MainScript.TT_FLOAT == tok.type)
        {
            res.register(advance());//might be a problem...
            return res.success(new NumberNode(tok));
        }
        else if (tok.type == MainScript.TT_LPAREN)
        {
            res.register(advance());
            Node exprTemp = res.register(expr());
            if (res.error != null)
                return res;
            if (current_tok.type == MainScript.TT_RPAREN)
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
                "Expected int, float, \"+\", \"-\" or \"(\""));


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

        return Power();
    }

    private ParseResult Power()
    {
        return binOp(atom, new string[] {MainScript.TT_POW },factor);
    }

    ParseResult term()
    {
        return binOp(factor, new string[] { MainScript.TT_MUL, MainScript.TT_DIV });
    }

    ParseResult expr()
    {
        // Holds result
        ParseResult res = new ParseResult();

        // finds keyword values like VAR
        if (current_tok.Matches(MainScript.TT_KEYWORD, "VAR")) {
            res.register(advance());

            // If token next to KEYWORD is not identifier throw an error
            if (current_tok.type != MainScript.TT_IDENTIFIER)
                return res.failure(
                    new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected Identifier"));

            Token varName = current_tok;
            res.register(advance());

            // If token next to IDENTIFIER is not "=" throw an error
            if (current_tok.type != MainScript.TT_EQ)
                return res.failure(
                    new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected \"=\""));

            res.register(advance());

            // Registers the expresion right next to the "="
            Node exTemp = res.register(this.expr());
            if (res.error != null)// ERROR check
                return res;
            return res.success(new VarAssignNode(varName, exTemp));
        }


        return binOp(term, new string[]{ MainScript.TT_PLUS, MainScript.TT_MINUS });
    }


    ParseResult binOp(Func<ParseResult> funcA, string[] ops, Func<ParseResult> funcB = null)
    {
        if (funcB == null)
            funcB = funcA;

        ParseResult res = new ParseResult();
        Node left = res.register(funcA());// Find left node
        if (res.error != null)// ERROR check
            return res;

        while (ops.Any(x => x == current_tok.type))
        {
            Token op_tok = current_tok;
            res.register(advance());
            Node right = res.register(funcB());// Finds right node
            if (res.error != null)// ERROR check
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
                    case '^':
                        tokens.Add(new Token(MainScript.TT_POW, posStart: this.pos));
                        advance();
                        break;
                    case '=':
                        tokens.Add(new Token(MainScript.TT_EQ, posStart: this.pos));
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
                        if (MainScript.DIGITS.Contains((char)current_char)){
                            tokens.Add(make_number());
                        } else if (MainScript.LETTERS.Contains((char)current_char)){
                            tokens.Add(MakeIdentifier());
                        } else
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

    private Token MakeIdentifier()
    {
        string idStr = "";
        Position posStart = pos.copy();

        while (current_char != null && MainScript.LETTERS_DIGITS.Contains((char)current_char)) {
            idStr += current_char;
            advance();
        }
        //TODO: keywords. cointain idStr
        string tokType;
        if (MainScript.KEYWORDS.Any(x => x == idStr))
            tokType = MainScript.TT_KEYWORD;
        else
            tokType = MainScript.TT_IDENTIFIER;

        return new Token(tokType, idStr, posStart, pos);

    }
}



//TOKENS--------------------------------------------------------------

public class Token
{
    public string type;
    public object value;
    public Position posStart;
    public Position posEnd;

    public Token(string type, object value = null, Position posStart = null, Position posEnd = null)
    {
        this.type = type;
        this.value = value;//temporary fix

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

    internal bool Matches(string type, string value)
    {
        return (this.type == type && this.value.ToString() == value);
    }
}


//Values
//###################################################
public class Number
{
    public float? Value;
    Position PosStart;
    Position PosEnd;
    public Context Context;

    public Number(object value)
    {
        this.Value = float.Parse(value.ToString());
        SetPos();
    }

    public Number SetPos(Position posStart = null, Position posEnd = null){
        this.PosStart = posStart;
        this.PosEnd = posEnd;
        return this;
    }
    public (Number, CustomError) AddedTo(Number other){
        return (new Number(Value + other.Value).setContext(this.Context), null);
    }
    public (Number, CustomError) SubbedBy(Number other)
    {
        return (new Number(Value - other.Value).setContext(this.Context), null);
    }
    public (Number, CustomError) MultedBy(Number other)
    {
        return (new Number(Value * other.Value).setContext(this.Context), null);
    }
    public (Number, CustomError) DivBy(Number other)
    {
        if (other.Value == 0)
            return (null, new RTError(other.PosStart, other.PosEnd, "Division by zero", this.Context));
        return (new Number(Value / other.Value).setContext(this.Context), null);
    }

    internal (Number, CustomError) PoweredBy(Number right)
    {
        return (new Number(MathF.Pow((float)Value, (float)right.Value)).setContext(this.Context), null);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public Number setContext(Context context = null)
    {
        this.Context = context;
        return this;
    }

}



//Context
//###################################################


public class Context
{
    public string displayName;
    public Context parent;
    public Position parentEtrPos;

    public Context(string displayName, Context parent = null, Position parentEtrPos = null)
    {
        this.displayName = displayName;
        this.parent = parent;
        this.parentEtrPos = parentEtrPos;
    }
}




//Interpreter
//###################################################

class Interpreter
{
    public RTResult Visit(Node node, Context context) {
        string method_name = $"Visit_{node.GetType().Name}";

        Type thisType = this.GetType();
        MethodInfo theMethod = thisType.GetMethod(method_name);
        object[] para = { node, context };
        return (RTResult)theMethod.Invoke(this, para);
    }
    public void NoVisitMethod(Node node, Context context)
    {
        throw new Exception($"No Visit_{node.GetType().Name} method defined");
    }

    public RTResult Visit_NumberNode(NumberNode node, Context context)
    {
        return new RTResult().success(
            new Number(node.tok.value).setContext(context).SetPos(node.PosStart, node.PosEnd));
    }
    public RTResult Visit_BinOpNode(BinOpNode node, Context context)
    {
        RTResult res = new RTResult();


        Number left = res.register(Visit(node.left_node, context));
        if (res.error != null)
            return res;
        Number right = res.register(Visit(node.right_node, context));
        if (res.error != null)
            return res;

        (Number, CustomError) result = (null, null);

        if (node.tok.type == MainScript.TT_PLUS)
            result = left.AddedTo(right);
        if (node.tok.type == MainScript.TT_MINUS)
            result = left.SubbedBy(right);
        if (node.tok.type == MainScript.TT_MUL)
            result = left.MultedBy(right);
        if (node.tok.type == MainScript.TT_DIV)
            result = left.DivBy(right);
        if (node.tok.type == MainScript.TT_POW)
            result = left.PoweredBy(right);

        if (result.Item2 != null)
            return res.failure(result.Item2);
        else
            return res.success(result.Item1.SetPos(node.PosStart, node.PosEnd));

    }
    public RTResult Visit_UnaryOpNode(UnaryOpNode node, Context context)
    {
        RTResult res = new RTResult();


        Number numb = res.register(Visit(node.node, context));
        if (res.error != null)
            return res;


        (Number, CustomError) result = (numb, null);


        if (node.tok.type == MainScript.TT_MINUS)
            result = numb.MultedBy(new Number(-1));


        if (res.error != null)
            return res.failure(result.Item2);
        else
            return res.success(result.Item1.SetPos(node.PosStart, node.PosEnd));
    }


}
