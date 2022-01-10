using MacroBotV0._1Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;

using Emgu.CV;
using Emgu.CV.Structure;


//List of broken things:
/*
Context doesnt know about perent contexts and it just looks at it self
Some errors override old errors even tho they are correct
fun parameters should have its own parent
using VAR keyword everytime to create variable is dumb
you can create global variable when adding it to arguments of FOR cycle
for some reason its legal to create unary expresion with array
ITS OK ITS NOT A BUG BUT A FEATURE!
 */

public static class MainScript
{
    //Global symbol table
    public static SymbolTable GlobalSymbolTable = new SymbolTable(); //not setting null to 0

    //digit constants
    public static string DIGITS = "0123456789";
    public static string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
    public static string LETTERS_DIGITS = LETTERS + DIGITS;

    #region tokens constants
    public const string TT_INT = "TT_INT";
    public const string TT_FLOAT = "TT_FLOAT";
    public const string TT_STRING = "TT_STRING";
    public const string TT_IDENTIFIER = "TT_IDENTIFIER";
    public const string TT_KEYWORD = "TT_KEYWORD";
    public const string TT_PLUS = "TT_PLUS";
    public const string TT_MINUS = "TT_MINUS";
    public const string TT_MUL = "TT_MUL";
    public const string TT_DIV = "TT_DIV";
    public const string TT_POW = "TT_POW";
    public const string TT_EQ = "TT_EQ";
    public const string TT_LPAREN = "TT_LPAREN";
    public const string TT_RPAREN = "TT_RPAREN";
    public const string TT_LSQUARE = "TT_LSQUARE";
    public const string TT_RSQUARE = "TT_RSQUARE";
    public const string TT_EE = "TT_EE";
    public const string TT_NE = "TT_NE";
    public const string TT_LT = "TT_LT";
    public const string TT_GT = "TT_GT";
    public const string TT_LTE = "TT_LTE";
    public const string TT_GTE = "TT_GTE";
    public const string TT_COMMA = "TT_COMMA";
    public const string TT_ARROW = "TT_ARROW";
    public const string TT_NEWLINE = "TT_NEWLINE";
    public const string TT_EOF = "TT_EOF";
    #endregion

    public static string[] KEYWORDS =
    {
        "VAR",
        "AND",
        "OR",
        "NOT",
        "IF",
        "THEN",
        "ELIF",
        "ELSE",
        "FOR",
        "TO",
        "STEP",
        "FUN",
        "END",
        "WHILE"
    };

    public static Dictionary<char, char> EscapeChars;

    public static (ValueF, CustomError) Run(string fn, string text, List<AssetItem> assetItems)
    {
        #region setting global VARIABLES
        GlobalSymbolTable.Set("NULL", new SpecialValue(SpecialValue.SpecialType.NullVal));
        GlobalSymbolTable.Set("TRUE", new SpecialValue(SpecialValue.SpecialType.TrueVal));
        GlobalSymbolTable.Set("FALSE", new SpecialValue(SpecialValue.SpecialType.FalseVal));
        GlobalSymbolTable.Set("PI", new Number(MathF.PI));
        #endregion
        #region built-in FUNCTIONS
        GlobalSymbolTable.Set("PRINT", new BuiltInFun("print"));
        GlobalSymbolTable.Set("TO_STRING", new BuiltInFun("toString"));
        GlobalSymbolTable.Set("DELAY", new BuiltInFun("delay"));
        GlobalSymbolTable.Set("MOVE", new BuiltInFun("moveMouse"));
        GlobalSymbolTable.Set("SIN", new BuiltInFun("sin"));
        GlobalSymbolTable.Set("PRESS", new BuiltInFun("press"));
        GlobalSymbolTable.Set("CLICK", new BuiltInFun("cursorClick"));
        GlobalSymbolTable.Set("START", new BuiltInFun("startApp"));
        #endregion
        #region KeyCodes
        GlobalSymbolTable.Set("VK_0", new Number(0x30));
        GlobalSymbolTable.Set("VK_1", new Number(0x31));
        GlobalSymbolTable.Set("VK_2", new Number(0x32));
        GlobalSymbolTable.Set("VK_3", new Number(0x33));
        GlobalSymbolTable.Set("VK_4", new Number(0x34));
        GlobalSymbolTable.Set("VK_5", new Number(0x35));
        GlobalSymbolTable.Set("VK_6", new Number(0x36));
        GlobalSymbolTable.Set("VK_7", new Number(0x37));
        GlobalSymbolTable.Set("VK_8", new Number(0x38));
        GlobalSymbolTable.Set("VK_9", new Number(0x39));
        GlobalSymbolTable.Set("VK_A", new Number(0x41));
        GlobalSymbolTable.Set("VK_B", new Number(0x42));
        GlobalSymbolTable.Set("VK_C", new Number(0x43));
        GlobalSymbolTable.Set("VK_D", new Number(0x44));
        GlobalSymbolTable.Set("VK_E", new Number(0x45));
        GlobalSymbolTable.Set("VK_F", new Number(0x46));
        GlobalSymbolTable.Set("VK_G", new Number(0x47));
        GlobalSymbolTable.Set("VK_H", new Number(0x48));
        GlobalSymbolTable.Set("VK_I", new Number(0x49));
        GlobalSymbolTable.Set("VK_J", new Number(0x4A));
        GlobalSymbolTable.Set("VK_K", new Number(0x4B));
        GlobalSymbolTable.Set("VK_L", new Number(0x4C));
        GlobalSymbolTable.Set("VK_M", new Number(0x4D));
        GlobalSymbolTable.Set("VK_N", new Number(0x4E));
        GlobalSymbolTable.Set("VK_O", new Number(0x4F));
        GlobalSymbolTable.Set("VK_P", new Number(0x50));
        GlobalSymbolTable.Set("VK_Q", new Number(0x51));
        GlobalSymbolTable.Set("VK_R", new Number(0x52));
        GlobalSymbolTable.Set("VK_S", new Number(0x53));
        GlobalSymbolTable.Set("VK_T", new Number(0x54));
        GlobalSymbolTable.Set("VK_U", new Number(0x55));
        GlobalSymbolTable.Set("VK_V", new Number(0x56));
        GlobalSymbolTable.Set("VK_W", new Number(0x57));
        GlobalSymbolTable.Set("VK_X", new Number(0x58));
        GlobalSymbolTable.Set("VK_Y", new Number(0x59));
        GlobalSymbolTable.Set("VK_Z", new Number(0x5A));
        GlobalSymbolTable.Set("VK_NUMPAD0", new Number(0x60));
        GlobalSymbolTable.Set("VK_NUMPAD1", new Number(0x61));
        GlobalSymbolTable.Set("VK_NUMPAD2", new Number(0x62));
        GlobalSymbolTable.Set("VK_NUMPAD3", new Number(0x63));
        GlobalSymbolTable.Set("VK_NUMPAD4", new Number(0x64));
        GlobalSymbolTable.Set("VK_NUMPAD5", new Number(0x65));
        GlobalSymbolTable.Set("VK_NUMPAD6", new Number(0x66));
        GlobalSymbolTable.Set("VK_NUMPAD7", new Number(0x67));
        GlobalSymbolTable.Set("VK_NUMPAD8", new Number(0x68));
        GlobalSymbolTable.Set("VK_NUMPAD9", new Number(0x69));
        GlobalSymbolTable.Set("VK_MULTIPLY", new Number(0x6A));
        GlobalSymbolTable.Set("VK_ADD", new Number(0x6B));
        GlobalSymbolTable.Set("VK_SUBTRACT", new Number(0x6D));
        GlobalSymbolTable.Set("VK_DECIMAL", new Number(0x6E));
        GlobalSymbolTable.Set("VK_DIVIDE", new Number(0x6F));
        GlobalSymbolTable.Set("VK_F1", new Number(0x70));
        GlobalSymbolTable.Set("VK_F2", new Number(0x71));
        GlobalSymbolTable.Set("VK_F3", new Number(0x72));
        GlobalSymbolTable.Set("VK_F4", new Number(0x73));
        GlobalSymbolTable.Set("VK_F5", new Number(0x74));
        GlobalSymbolTable.Set("VK_F6", new Number(0x75));
        GlobalSymbolTable.Set("VK_F7", new Number(0x76));
        GlobalSymbolTable.Set("VK_F8", new Number(0x77));
        GlobalSymbolTable.Set("VK_F9", new Number(0x78));
        GlobalSymbolTable.Set("VK_F10", new Number(0x79));
        GlobalSymbolTable.Set("VK_F11", new Number(0x7A));
        GlobalSymbolTable.Set("VK_F12", new Number(0x7B));
        #endregion
        #region Creating Image variables
        foreach(AssetItem im in assetItems)
            GlobalSymbolTable.Set(im.name, new ImageValue(im.asset));
        #endregion
        EscapeChars = new Dictionary<char, char>();
        EscapeChars.Add('n','\n');
        EscapeChars.Add('t','\t');

        //generate tokens
        Lexer lexer = new Lexer(fn, text);
        (List<Token>, CustomError) tokensAndError = lexer.make_tokens();
        List<Token> tokens = tokensAndError.Item1;
        //Console.WriteLine(tokens.ToDelimitedString()); //debug draw tokens
        CustomError error = tokensAndError.Item2;
        if (error != null)
            return (null, error);

        //generate AST
        Parser parser = new Parser(tokens);
        ParseResult ast = parser.parse();
        if (ast.error != null)
            return (null, ast.error);


        // Run program
        ContextHolder context = new ContextHolder("<Program>");
        context.symbolTable = GlobalSymbolTable;
        RTResult result = Interpreter.Visit(ast.node, context);

        return (result.value, result.error);
    }


}

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
        current_char = pos.idx < text.Length ? text[pos.idx] : (char?)null;
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

            if (" \t".Contains((char)current_char))
            {
                advance();
            }
            else if (current_char == '"')
            {
                tokens.Add(makeString());
            }
            else if (current_char == '+')
            {
                tokens.Add(new Token(MainScript.TT_PLUS, posStart: this.pos));
                advance();
            }
            else if (current_char == '-')
            {
                tokens.Add(makeMinusOrArrow());
            }
            else if (current_char == '*')
            {
                tokens.Add(new Token(MainScript.TT_MUL, posStart: this.pos));
                advance();
            }
            else if (current_char == '/')
            {
                tokens.Add(new Token(MainScript.TT_DIV, posStart: this.pos));
                advance();
            }
            else if (current_char == '^')
            {
                tokens.Add(new Token(MainScript.TT_POW, posStart: this.pos));
                advance();
            }
            else if (current_char == '(') {
                tokens.Add(new Token(MainScript.TT_LPAREN, posStart: this.pos));
                advance();
            }
            else if (current_char == ')') {
                tokens.Add(new Token(MainScript.TT_RPAREN, posStart: this.pos));
                advance();
            }
            else if (current_char == '[') {
                tokens.Add(new Token(MainScript.TT_LSQUARE, posStart: this.pos));
                advance();
            }
            else if (current_char == ']') {
                tokens.Add(new Token(MainScript.TT_RSQUARE, posStart: this.pos));
                advance();
            }
            else if (current_char == '!') {
                (Token, CustomError) tokNError = makeNotEquals();
                if (tokNError.Item2 != null) return (null, tokNError.Item2);
                tokens.Add(tokNError.Item1);
            }
            else if (current_char == '=')
            {
                tokens.Add(makeEquals());
            }
            else if (current_char == '<')
            {
                tokens.Add(makeLessThen());
            }
            else if (current_char == '>')
            {
                tokens.Add(makeGreaterThen());
            }
            else if (current_char == ',')
            {
                tokens.Add(new Token(MainScript.TT_COMMA, posStart: this.pos));
                advance();
            }
            else if (";\n".Contains((char)current_char))
            {
                tokens.Add(new Token(MainScript.TT_NEWLINE, posStart: this.pos));
                advance();
            }
            else if (MainScript.DIGITS.Contains((char)current_char))
            { 
                tokens.Add(make_number());
            }
            else if (MainScript.LETTERS.Contains((char)current_char))
            {
                tokens.Add(MakeIdentifier());
            }
            else
            {
                Position pos_start = pos.copy();
                char tempChar = (char)current_char;
                advance();
                return (new List<Token>(), new IllegalCharError(pos_start, pos, "'" + tempChar + "'"));
            }
        }

        tokens.Add(new Token(MainScript.TT_EOF, posStart: this.pos));
        return (tokens, null);
    }


    private Token makeString()
    {
        string str = "";
        Position posStart = pos.copy();
        bool escapeChar = false;

        advance();// Steps over starting double Quot

        while(current_char != null && (current_char != '"' || escapeChar))
        {
            if (escapeChar)
                str += MainScript.EscapeChars
                    .GetValueOrDefault<char, char>
                    ((char)current_char,
                    (char)current_char);// if it doesnt find escape char it returns current char
            else if (current_char == '\\')
                escapeChar = true;
            else
                str += current_char;
            advance();
            escapeChar = false;
        }
        advance();// Steps over ending double Quot
        return new Token(MainScript.TT_STRING, str, posStart, pos);

    }

    private Token makeMinusOrArrow()
    {
        string tokenType = MainScript.TT_MINUS;
        Position posStart = this.pos.copy();
        advance();

        if (current_char == '>')
        {
            advance();
            tokenType = MainScript.TT_ARROW;
        }

        return new Token(tokenType, posStart: posStart, posEnd: pos);
    }

    private Token makeGreaterThen()
    {
        string tokenType = MainScript.TT_GT;
        Position posStart = this.pos.copy();
        advance();

        if (current_char == '=')
        {
            advance();
            tokenType = MainScript.TT_GTE;
        }

        return new Token(tokenType, posStart: posStart, posEnd: pos);
    }

    private Token makeLessThen()
    {
        string tokenType = MainScript.TT_LT;
        Position posStart = this.pos.copy();
        advance();

        if (current_char == '=')
        {
            advance();
            tokenType = MainScript.TT_LTE;
        }

        return new Token(tokenType, posStart: posStart, posEnd: pos);
    }

    private Token makeEquals()
    {
        string tokenType = MainScript.TT_EQ;
        Position posStart = this.pos.copy();
        advance();

        if (current_char == '=')
        {
            advance();
            tokenType = MainScript.TT_EE;
        }

        return new Token(tokenType, posStart: posStart, posEnd: pos);
    }


    private (Token, CustomError) makeNotEquals()
    {
        Position posStart = this.pos.copy();
        advance();

        if(current_char == '=')
        {
            advance();
            return (new Token(MainScript.TT_NE, posStart, pos), null);
        }

        advance();
        return (null, new ExpectedCharError(posStart, pos, "'=' (after '!')"));
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

#region RunTime Result
public class RTResult
{
    public ValueF value;
    public CustomError error;
    public bool HasError;

    public ValueF register(RTResult res)
    {
        if (res.HasError)
        {
            error = res.error;
            this.HasError = true;
        }
        return res.value;
    }

    public RTResult success(ValueF value)
    {
        this.value = value;
        return this;
    }

    public RTResult failure(CustomError error)
    {
        this.error = error;
        HasError = true;
        return this;
    }

}
#endregion

#region Nodes
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
public class StringNode : Node
{
    public StringNode(Token tok) : base(tok)
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
public class VarAssignNode : Node
{
    public Node ValueNode;
    public Token VarNameTok;

    public VarAssignNode(Token varNameTok, Node valueNode) : base(varNameTok)
    {
        this.VarNameTok = varNameTok;
        this.ValueNode = valueNode;

        this.PosStart = varNameTok.posStart;
        this.PosEnd = valueNode.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class VarAccessNode : Node//TODO: rest
{
    public Token VarNameTok;

    public VarAccessNode(Token varNameTok) : base(varNameTok)
    {
        this.PosStart = varNameTok.posStart;
        this.PosEnd = varNameTok.posEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class IfThenNode : Node
{
    public List<(Node, Node, bool)> cases;
    public (Node, bool) elseCase;

    public IfThenNode(List<(Node, Node, bool)> cases, (Node, bool) elseCase): base(null)
    {
        this.cases = cases;
        this.elseCase = elseCase;

        PosStart = cases[0].Item1.PosStart;
        PosEnd = elseCase.Item1 != null ? elseCase.Item1.PosEnd : cases[cases.Count - 1].Item2.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class ForNode : Node
{
    public Node StartValue;
    public Node EndValue;
    public Node BodyVal;
    public Node StepVal;
    public bool ShouldReturnNull;
    public ForNode(Token varName, Node startValue, Node endValue, Node bodyVal, Node stepVal, bool shouldReturnNull) : base(varName)
    {
        this.StartValue = startValue;
        this.EndValue = endValue;
        this.BodyVal = bodyVal;
        this.StepVal = stepVal;
        this.ShouldReturnNull = shouldReturnNull;

        this.PosStart = varName.posStart;
        this.PosEnd = BodyVal.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class WhileNode : Node
{
    public Node Condition;
    public Node BodyVal;
    public bool ShouldReturnNull;

    public WhileNode(Node condition, Node bodyVal, bool shouldReturnNull) : base(null)
    {
        this.Condition = condition;
        this.BodyVal = bodyVal;
        this.ShouldReturnNull = shouldReturnNull;

        this.PosStart = condition.PosStart;
        this.PosEnd = BodyVal.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class FunDefNode : Node
{
    public List<Token> argNameToks;
    public Node bodyNode;
    public bool ShouldReturnNull;

    public FunDefNode(List<Token> argNameToks, Node bodyNode, bool shouldReturnNull, Token varNameTok = null) :base(varNameTok)
    {
        this.argNameToks = argNameToks;
        this.bodyNode = bodyNode;
        this.ShouldReturnNull = shouldReturnNull;

        if (varNameTok != null)
            PosStart = varNameTok.posStart;
        else if (argNameToks.Count > 0)
            PosStart = argNameToks[0].posStart;
        else
            PosStart = bodyNode.PosStart;

        PosEnd = bodyNode.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class CallNode : Node
{
    public Node NodeToCall;
    public List<Node> argNodes;

    public CallNode(Node nodeToCall, List<Node> argNodes):base(null)
    {
        NodeToCall = nodeToCall;
        this.argNodes = argNodes;

        PosStart = NodeToCall.PosStart;

        if (argNodes.Count > 0)
            PosEnd = argNodes.Last().PosEnd;
        else
            PosEnd = NodeToCall.PosEnd;

    }
    //public override string ToString() => $"({tok}, {node})";
}
public class ListNode:Node
{
    public List<Node> ElementNodes;

    public ListNode(List<Node> elementNodes, Position posStart, Position posEnd):base(null)
    {
        ElementNodes = elementNodes;
        PosStart = posStart;
        PosEnd = posEnd;
    }
}
#endregion

class ParseResult
{
    public int ReverseCount = 0;
    public int lastRegAdvanceCount = 0;
    public int advanceCount = 0;

    public CustomError error;
    public Node node;
    public bool HasError;

    public (List<(Node, Node, bool)>, (Node, bool)) IfCases;//I hate this solution, this is not a solution this is crime!


    public void registerAdvancement()
    {
        lastRegAdvanceCount = 0;
        advanceCount++;
    }

    public Node register(ParseResult res) {
        lastRegAdvanceCount = res.advanceCount;
        advanceCount += res.advanceCount;
        if (res.HasError) { 
            this.error = res.error;
            this.HasError = true;
        }
        return res.node;
    }


    public Node register(out (List<(Node, Node, bool)>, (Node, bool))  ifCases , ParseResult res) {
        lastRegAdvanceCount = res.advanceCount;
        advanceCount += res.advanceCount;
        if (res.HasError) { 
            this.error = res.error;
            this.HasError = true;
        }
        ifCases = res.IfCases;
        return res.node;
    }


    public Node tryRegister(ParseResult res)
    {
        if (res.HasError)
        {
            ReverseCount = res.advanceCount;
            return null;
        }
        return res.register(res);
    }


    public ParseResult success(Node node) {
        this.node = node;
        return this;
    }

    public ParseResult success((List<(Node, Node, bool)>, (Node, bool)) ifCases)
    {
        this.IfCases = ifCases;
        return this;
    }

    public ParseResult failure(CustomError error) {
        if(this.error == null || this.advanceCount == 0) 
            this.error = error;
        HasError = true;
        return this;
    }

    internal ParseResult success((Node, bool) elseCase)
    {
        this.IfCases.Item2 = elseCase;
        return this;
    }
}

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
        updateCurrentTok();
        return current_tok;
    }

    Token reverse(int reverseCount = 1)
    {
        tok_idx -= reverseCount;
        updateCurrentTok();
        return current_tok;
    }

    private void updateCurrentTok()
    {
        if (tok_idx >= 0 && tok_idx < tokens.Count)
            current_tok = tokens[tok_idx];
    }

    //#################################################################
    public ParseResult parse()
    {
        ParseResult res = statements();
        if (!res.HasError && this.current_tok.type != MainScript.TT_EOF)
        {
            return res.failure(new InvalidSyntaxError(
                this.current_tok.posStart,
                this.current_tok.posEnd,
                "Expected '+', '-', '*', '/', '^', '==', '!=', '<', '>', '<=', '>=', 'AND' or 'OR'"));
        }
        return res;
    }


    #region Language grammar methods  
    private ParseResult statements()
    {
        // Holds result
        ParseResult res = new ParseResult();

        List<Node> statementsTemp = new List<Node>();
        Position posStart = current_tok.posStart.copy();

        while(current_tok.type == MainScript.TT_NEWLINE)
        {
            res.registerAdvancement();
            advance();
        }

        Node statementTemp = res.register(expr());
        if (res.HasError) return res;

        statementsTemp.Add(statementTemp);

        bool moreStatements = true;

        while (true)
        {
            int newLineCount = 0;
            while (current_tok.type == MainScript.TT_NEWLINE)
            {
                res.registerAdvancement();
                advance();
                newLineCount++;
            }
            if (newLineCount == 0)
                moreStatements = false;

            if (!moreStatements)
                break;

            statementTemp = res.tryRegister(expr());
            if (statementTemp == null)
            {
                reverse(res.ReverseCount);
                moreStatements = false;
                continue;
            }
            statementsTemp.Add(statementTemp);
        }

        return res.success(new ListNode(
            statementsTemp,
            posStart,
            current_tok.posEnd.copy()));

    }
    private ParseResult expr()
    {
        // Holds result
        ParseResult res = new ParseResult();

        // finds keyword values like VAR
        if (current_tok.Matches(MainScript.TT_KEYWORD, "VAR"))
        {
            res.registerAdvancement();
            advance();

            // If token next to KEYWORD is not identifier throw an error
            if (current_tok.type != MainScript.TT_IDENTIFIER)
                return res.failure(
                    new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected Identifier"));

            Token varName = current_tok;
            res.registerAdvancement();
            advance();

            // If token next to IDENTIFIER is not "=" throw an error
            if (current_tok.type != MainScript.TT_EQ)
                return res.failure(
                    new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected \"=\""));

            res.registerAdvancement();
            advance();

            // Registers the expresion right next to the "="
            Node exTemp = res.register(this.expr());
            if (res.HasError)// ERROR check
                return res;

            return res.success(new VarAssignNode(varName, exTemp));
        }

        Node node = res.register(binOp(compExpr, new (string, string)[] { (MainScript.TT_KEYWORD, "AND"), (MainScript.TT_KEYWORD, "OR") }));
        if (res.HasError)
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart, current_tok.posEnd,
                "Expected 'VAR', int, float, identifier, '+', '-', '[' or '('"));

        return res.success(node);
    }
    private ParseResult compExpr()
    {
        ParseResult res = new ParseResult();

        if(current_tok.Matches(MainScript.TT_KEYWORD, "NOT"))
        {
            Token opTok = current_tok;
            res.registerAdvancement();
            advance();

            Node nodeNot = res.register(compExpr());
            if (res.HasError)
                return res;
            return res.success(new UnaryOpNode(opTok, nodeNot));
        }

        Node node = res.register(binOp(arithExpr, new (string, string)[] {(MainScript.TT_EE, null), (MainScript.TT_NE, null), (MainScript.TT_LT, null), (MainScript.TT_GT, null), (MainScript.TT_GTE, null), (MainScript.TT_LTE, null) }));
        if (res.HasError) return res;

        //this causes problesm
        /*if (res.HasError)
            return res.failure(
                new InvalidSyntaxError(
                    node.PosStart,
                    node.PosEnd,
                    "Expected int, float, identifier, '+', '-', '(', '[' or ''"));*/

        return res.success(node);

    }
    private ParseResult arithExpr()
    {
        return binOp(term, new (string, string)[] { (MainScript.TT_PLUS, null), (MainScript.TT_MINUS, null) });
    }
    private ParseResult term() => binOp(factor, new (string, string)[] { (MainScript.TT_MUL, null), (MainScript.TT_DIV, null) });
    private ParseResult factor()
    {
        ParseResult res = new ParseResult();
        Token tok = current_tok;

        if (tok.type == MainScript.TT_MINUS || tok.type == MainScript.TT_PLUS)
        {
            res.registerAdvancement();
            advance();
            Node factorTemp = res.register(factor());
            if (res.HasError)
                return res;
            return res.success(new UnaryOpNode(tok, factorTemp));

        }

        return Power();
    }
    private ParseResult Power() => binOp(Call, new (string, string)[] { (MainScript.TT_POW, null) }, factor);
    private ParseResult Call()
    {
        ParseResult res = new ParseResult();
        List<Node> argsNodes = new List<Node>();

        Node atomTemp = res.register(atom());
        if (res.HasError) return res;



        if (current_tok.type == MainScript.TT_LPAREN)
        {
            res.registerAdvancement();// Steps over LPAREN
            advance();

            if (current_tok.type == MainScript.TT_RPAREN)
            {
                res.registerAdvancement();// Steps over RPAREN
                advance();
            }
            else
            {
                argsNodes.Add(res.register(expr()));// Adds the first parameter if it exists
                if (res.HasError)
                    return res.failure(new InvalidSyntaxError(
                        current_tok.posStart, current_tok.posEnd,
                        "Expected 'VAR', int, float, identifier, '+', '-', ')', '[' or '('"));

                while (current_tok.type == MainScript.TT_COMMA)
                {
                    res.registerAdvancement();// Steps over COMMA
                    advance();

                    argsNodes.Add(res.register(expr()));
                    if (res.HasError) return res; // Ends if Error was founds
                }

                if (current_tok.type != MainScript.TT_RPAREN)
                    return res.failure(new InvalidSyntaxError(
                        current_tok.posStart, current_tok.posEnd,
                        "Expected ',' or ')'"));

                res.registerAdvancement();// Steps over RPAREN
                advance();

            }


            return res.success(new CallNode(atomTemp, argsNodes));
        }
        return res.success(atomTemp);

    }
    private ParseResult atom()
    {
        ParseResult res = new ParseResult();
        Token tok = current_tok;

        if (MainScript.TT_INT == tok.type || MainScript.TT_FLOAT == tok.type)
        {
            res.registerAdvancement();
            advance();
            return res.success(new NumberNode(tok));
        }
        else if (MainScript.TT_STRING == tok.type)
        {
            res.registerAdvancement();
            advance();
            return res.success(new StringNode(tok));
        }
        else if (tok.type == MainScript.TT_IDENTIFIER)
        {
            res.registerAdvancement();
            advance();
            return res.success(new VarAccessNode(tok));
        }
        else if (tok.type == MainScript.TT_LPAREN)
        {
            res.registerAdvancement();
            advance();
            Node exprTemp = res.register(expr());
            if (res.HasError)
                return res;
            if (current_tok.type == MainScript.TT_RPAREN)
            {
                res.registerAdvancement();
                advance();
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
        else if (tok.type == MainScript.TT_LSQUARE)
        {
            Node listExprTemp = res.register(listExpr());
            if (res.HasError) return res;
            return res.success(listExprTemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "IF"))//checks for if expresions
        {
            Node ifExprtemp = res.register(ifExpr());
            if (res.HasError)// ERROR check
                return res;
            return res.success(ifExprtemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "FOR"))
        {
            Node forExprtemp = res.register(forExpr());
            if (res.HasError) return res;
            return res.success(forExprtemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "WHILE"))
        {
            Node whileExprtemp = res.register(whileExpr());
            if (res.HasError)// ERROR check
                return res;
            return res.success(whileExprtemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "FUN"))
        {
            Node funDeftemp = res.register(Fundef());
            if (res.HasError)// ERROR check
                return res;
            return res.success(funDeftemp);
        }


        return res.failure(
            new InvalidSyntaxError(
                tok.posStart,
                tok.posEnd,
                "Expected int, float, identifier, \"[\", \"+\", \"-\" or \"(\""));
    }
    private ParseResult listExpr()
    {
        ParseResult res = new ParseResult();

        List<Node> elementNode = new List<Node>();
        Position posStart = current_tok.posStart.copy();

        res.registerAdvancement();// steps over '['
        advance();


        if (current_tok.type == MainScript.TT_RSQUARE)
        {
            res.registerAdvancement();
            advance();
        }
        else
        {
            elementNode.Add(res.register(expr()));// Adds the first parameter if it exists
            if (res.HasError)
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart, current_tok.posEnd,
                    "Expected 'VAR', int, float, identifier, '+', '-', ']', '[' or '('"));


            while (current_tok.type == MainScript.TT_COMMA)
            {
                res.registerAdvancement();// Steps over COMMA
                advance();

                elementNode.Add(res.register(expr()));
                if (res.HasError) return res; // Ends if Error was founds
            }

            if (current_tok.type != MainScript.TT_RSQUARE)
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart, current_tok.posEnd,
                    "Expected ',' or ']'"));

            res.registerAdvancement();// Steps over ']'
            advance();
        }
        return res.success(new ListNode(elementNode, posStart, current_tok.posStart.copy()));

    }
    private ParseResult ifExpr()
    {
        ParseResult res = new ParseResult();

        (List<(Node, Node, bool)>, (Node, bool)) allCases;
        res.register( out allCases, ifExprCases("IF"));
        if (res.HasError) return res;

        return res.success(new IfThenNode(allCases.Item1, allCases.Item2));
        
        /*
        List<(Node, Node)> cases = new List<(Node, Node)>();
        Node elseCase = null;

        //Position posStart = current_tok.posStart.copy();
        res.registerAdvancement();
        advance();

        // Registers the expresion right next to the "IF"
        Node condition = res.register(this.expr());
        if (res.HasError)// ERROR check
            return res;

        // If token next to expresion is not "THEN" throw an error
        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(
                new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected \"THEN\""));

        res.registerAdvancement();
        advance();

        // Registers the expresion right next to the "THEN"
        Node exprTemp = res.register(this.expr());
        if (res.HasError)// ERROR check
            return res;
        cases.Add((condition, exprTemp));

        while (current_tok.Matches(MainScript.TT_KEYWORD, "ELIF"))
        {
            res.registerAdvancement();
            advance();

            condition = res.register(expr());
            if (res.HasError)// ERROR check
                return res;

            if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
                return res.failure(
                    new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected \"THEN\""));

            res.registerAdvancement();
            advance();

            exprTemp = res.register(expr());
            if (res.HasError)// ERROR check
                return res;

            cases.Add((condition, exprTemp));
        }

        if (current_tok.Matches(MainScript.TT_KEYWORD, "ELSE"))
        {

            res.registerAdvancement();
            advance();

            exprTemp = res.register(expr());
            if (res.HasError)// ERROR check
                return res;
            elseCase = exprTemp;
        }

        return res.success(new VarIfThenNode(cases, elseCase));
        */
    }
    private ParseResult ifExprEif() => ifExprCases("ELIF");

    private ParseResult ifExprEifOrEl()
    {
        ParseResult res = new ParseResult();
        List<(Node, Node, bool)> cases = new List<(Node, Node, bool)>();
        (Node, bool) elseCase;

        if(current_tok.Matches(MainScript.TT_KEYWORD, "ELIF"))
        {

            (List<(Node, Node, bool)>, (Node, bool)) allCases;
            res.register(out allCases, ifExprEif());

            if (res.HasError) return res;
            cases = allCases.Item1;
            elseCase = allCases.Item2;
        }
        else
        {
            (List<(Node, Node, bool)>, (Node, bool)) allCases;
            res.register(out allCases, ifExprEl());
            elseCase = allCases.Item2;
            if (res.HasError) return res;
        }

        return res.success((cases, elseCase));
    }

    private ParseResult ifExprEl()
    {
        ParseResult res = new ParseResult();
        (Node, bool) elseCase = (null, true);

        if (current_tok.Matches(MainScript.TT_KEYWORD, "ELSE"))
        {
            res.registerAdvancement();// Steps over Else
            advance();

            if(current_tok.type == MainScript.TT_NEWLINE)
            {
                res.registerAdvancement();// Steps over NewLine
                advance();

                Node statementsTemp = res.register(statements());
                if (res.HasError) return res;
                elseCase = (statementsTemp, true);

                if(current_tok.Matches(MainScript.TT_KEYWORD, "END"))
                {
                    res.registerAdvancement();// Steps over END keyWord
                    advance();
                }
                else
                {
                    return res.failure(
                        new InvalidSyntaxError(
                            current_tok.posStart, current_tok.posEnd,
                            "Expected 'END'"));
                }
            }
            else
            {
                Node exprTemp = res.register(expr());
                if (res.HasError) return res;
                elseCase = (exprTemp, false);
            }
        }
        return res.success(elseCase);
    }

    private ParseResult ifExprCases(string caseKeyword)
    {
        ParseResult res = new ParseResult();
        List<(Node, Node, bool)> cases = new List<(Node, Node, bool)>();
        (Node, bool) elseCase = (null, true);

        if (!current_tok.Matches(MainScript.TT_KEYWORD, caseKeyword))
            return res.failure(
                new InvalidSyntaxError(
                    current_tok.posStart, current_tok.posEnd,
                    $"Expected '{caseKeyword}'"));

        res.registerAdvancement();// Steps over caseWord like if, elif and else
        advance();

        Node condition = res.register(expr());
        if (res.HasError) return res;

        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(
                new InvalidSyntaxError(
                    current_tok.posStart, current_tok.posEnd,
                    "Expected 'THEN'"));


        res.registerAdvancement();// Steps over THEN
        advance();

        if( current_tok.type == MainScript.TT_NEWLINE)
        {
            res.registerAdvancement();// Steps over newLine
            advance();

            Node statementsTemp = res.register(statements());
            if (res.HasError) return res;
            cases.Add((condition, statementsTemp, true));

            if(current_tok.Matches(MainScript.TT_KEYWORD, "END"))
            {
                res.registerAdvancement();
                advance();
            }
            else
            {
                (List<(Node, Node, bool)>, (Node, bool)) allCases;
                res.register(out allCases, ifExprEifOrEl());

                if (res.HasError) return res;
                List<(Node, Node, bool)>  newCases = allCases.Item1;
                elseCase = allCases.Item2;
                cases.AddRange(newCases);
            }
        }
        else
        {
            Node exprtemp = res.register(expr());
            if (res.HasError) return res;
            cases.Add((condition, exprtemp, false));

            (List<(Node, Node, bool)>, (Node, bool)) allCases;
            res.register(out allCases, ifExprEifOrEl());

            if (res.HasError) return res;
            List<(Node, Node, bool)> newCases = allCases.Item1;
            elseCase = allCases.Item2;
            cases.AddRange(newCases);
        }

        return res.success((cases, elseCase));

    }


    private ParseResult whileExpr()
    {
        ParseResult res = new ParseResult();

        Node condition;
        Node body;


        res.registerAdvancement(); // Steps over WHILE
        advance();


        // Registers the condition right next to the "WHILE"
        condition = res.register(this.expr());
        if (res.HasError)// ERROR check
            return res;


        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'THEN'"));


        res.registerAdvancement();
        advance();



        if (current_tok.type == MainScript.TT_NEWLINE)
        {
            res.registerAdvancement();
            advance();

            body = res.register(statements());
            if (res.HasError) return res;

            if (!current_tok.Matches(MainScript.TT_KEYWORD, "END"))
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected 'END'"));

            res.registerAdvancement();
            advance();


            return res.success(new WhileNode(condition, body, true));
        }

        res.registerAdvancement(); // Steps over THEN
        advance();



        // Registers the expresion right next to the "THEN"
        body = res.register(this.expr());
        if (res.HasError)// ERROR check
            return res;

        return res.success(new WhileNode(condition, body, false));

    }
    private ParseResult forExpr()
    {

        ParseResult res = new ParseResult();

        Token varName;
        Node startVal;
        Node bodyNode;
        Node stepNode = null;
        Node endVal;

        res.registerAdvancement(); // Steps over FOR
        advance();

        if (current_tok.type != MainScript.TT_IDENTIFIER)
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected identifier"));

        varName = current_tok;
        res.registerAdvancement();// Steps over IDENTIFIER
        advance();

        if (current_tok.type != MainScript.TT_EQ)
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected '='"));


        res.registerAdvancement();// Steps over EQ
        advance();

        startVal = res.register(expr());
        if (res.HasError)// ERROR check
            return res;


        if (!current_tok.Matches(MainScript.TT_KEYWORD, "TO"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'TO'"));


        res.registerAdvancement();// Steps over TO
        advance();



        endVal = res.register(expr());
        if (res.HasError)// ERROR check
            return res;


        if (current_tok.Matches(MainScript.TT_KEYWORD, "STEP"))
        {
            res.registerAdvancement();// Steps over TO
            advance();

            stepNode = res.register(expr());
            if (res.HasError)// ERROR check
                return res;
        }

        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'THEN'"));

        res.registerAdvancement();// Steps over THEN
        advance();


        if(current_tok.type == MainScript.TT_NEWLINE)
        {
            res.registerAdvancement();
            advance();

            bodyNode = res.register(statements());
            if (res.HasError) return res;

            if(!current_tok.Matches(MainScript.TT_KEYWORD, "END"))
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected 'END'"));

            res.registerAdvancement();
            advance();

            return res.success(new ForNode(varName, startVal, endVal, bodyNode, stepNode, true));
        }

        bodyNode = res.register(expr());
        if (res.HasError) return res;

        return res.success(new ForNode(varName, startVal, endVal, bodyNode, stepNode, false));
    }
    private ParseResult Fundef()
    {
        ParseResult res = new ParseResult();

        List<Token> argNameTokens = new List<Token>();
        Node bodyNode;
        Token varNameTok = null;

        res.registerAdvancement(); // Steps over FUN
        advance();

        string errorMsg = "Expected '('";
        if (current_tok.type == MainScript.TT_IDENTIFIER)
        {
            varNameTok = current_tok;
            res.registerAdvancement(); // Steps over IDENTIFIER
            advance();
        }
        else
            errorMsg += " or identifier";
        if (current_tok.type != MainScript.TT_LPAREN)
            return res.failure(new InvalidSyntaxError(current_tok.posStart, current_tok.posEnd, errorMsg));

        res.registerAdvancement(); // Steps over LPAREN
        advance();

        if (current_tok.type == MainScript.TT_IDENTIFIER)
        {
            argNameTokens.Add(current_tok);
            res.registerAdvancement(); // Steps over IDENTIFIER
            advance();

            while (current_tok.type == MainScript.TT_COMMA)
            {
                res.registerAdvancement(); // Steps over COMMA
                advance();

                if (current_tok.type != MainScript.TT_IDENTIFIER)
                    return res.failure(new InvalidSyntaxError(
                        current_tok.posStart,
                        current_tok.posEnd,
                        "Expected Identifier"));

                argNameTokens.Add(current_tok);
                res.registerAdvancement(); // Steps over IDENTIFIER
                advance();
            }

            if (current_tok.type != MainScript.TT_RPAREN)
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected ')' or ','"));
        }
        else
        {
            if (current_tok.type != MainScript.TT_RPAREN)
                return res.failure(new InvalidSyntaxError(
                    current_tok.posStart,
                    current_tok.posEnd,
                    "Expected 'identifier' or ')'"));
        }

        res.registerAdvancement(); // Steps over RPARAN
        advance();

        if (current_tok.type != MainScript.TT_ARROW)
        {
            res.registerAdvancement(); // Steps over ARROW
            advance();

            bodyNode = res.register(expr());
            if (res.HasError)
                return res;

            return res.success(new FunDefNode(argNameTokens, bodyNode, false, varNameTok));
        }

        if (current_tok.type != MainScript.TT_NEWLINE)
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected '->' or NEWLINE"));

        res.registerAdvancement();
        advance();

        bodyNode = res.register(statements());
        if (res.HasError) return res;

        if (!current_tok.Matches(MainScript.TT_KEYWORD, "END"))
            return res.failure(new InvalidSyntaxError(
                            current_tok.posStart, current_tok.posEnd,
                            "Expected 'END'"));

        res.registerAdvancement();
        advance();


        return res.success(new FunDefNode(argNameTokens, bodyNode, true, varNameTok));
    }
    #endregion

    ParseResult binOp(Func<ParseResult> funcA, (string, string)[] ops, Func<ParseResult> funcB = null)
    {
        if (funcB == null)
            funcB = funcA;

        ParseResult res = new ParseResult();
        Node left = res.register(funcA());// Find left node
        if (res.HasError)// ERROR check
            return res;

        while (ops.Any(x => x.Item1 == current_tok.type && current_tok.value == null) ||
               ops.Any(x => x.Item1 == current_tok.type && x.Item2 == current_tok.value.ToString()))
        {
            Token op_tok = current_tok;
            res.registerAdvancement();
            advance();
            Node right = res.register(funcB());// Finds right node
            if (res.HasError)// ERROR check
                return res;
            left = new BinOpNode(left, op_tok, right);
        }
        return res.success(left);
    }


}

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
#region Values
public class ValueF
{
    public Position PosStart;
    public Position PosEnd;
    public ContextHolder Context;

    public ValueF()
    {
        SetPos();
        setContext();
    }

    public ValueF setContext(ContextHolder context = null)
    {
        this.Context = context;
        return this;
    }
    public ValueF SetPos(Position posStart = null, Position posEnd = null)
    {
        this.PosStart = posStart;
        this.PosEnd = posEnd;
        return this;
    }
    public virtual ValueF Copy() => new ValueF();

    #region Mathematical expresion methods
    public virtual (ValueF, CustomError) AddedTo(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) SubbedBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) MultedBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) DivBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) PoweredBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonEE(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonNE(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonLT(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonGT(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonLTE(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) GetComparisonGTE(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) AndedBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) OredBy(ValueF other) => (null, IllegalOperation(other));
    public virtual (ValueF, CustomError) Notted() => (null, IllegalOperation());
    public virtual bool isTrue() => false;
    #endregion

    public virtual RTResult Execute(List<ValueF> args) => new RTResult().failure(IllegalOperation());
    public CustomError IllegalOperation(ValueF other= null)
    {
        if (other == null)
            other = this;


        return new RTError(
            PosStart, other.PosEnd,
            "Illegal operation",
            Context);
    }
}
public class SpecialValue: ValueF
{
    public enum SpecialType
    {
        NullVal, TrueVal, FalseVal
    }
    public SpecialType TypeVal;

    public SpecialValue(SpecialType value) : base()
    {
        this.TypeVal = value;
    }
    public override ValueF Copy()
    {
        SpecialValue copy = new SpecialValue(TypeVal);
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }
    public override string ToString()
    {
        switch (TypeVal)
        {
            case SpecialType.TrueVal: return "TRUE";
            case SpecialType.FalseVal: return "FALSE";
            case SpecialType.NullVal: return "NULL";
            default: return base.ToString();
        }
    }

    #region Mathematical expresion methods //TODO: make errors returning null exeptions or returning false
    public override (ValueF, CustomError) AddedTo(ValueF other)
    {
        if (other is SpecialValue && TypeVal == SpecialType.NullVal)//If im null i return the other value
            return (other.Copy().setContext(this.Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonEE(ValueF other)
    {
        if (other is SpecialValue)
            return (new SpecialValue(TypeVal == ((SpecialValue)other).TypeVal ? SpecialType.TrueVal : SpecialType.FalseVal).setContext(Context), null);
        else
            return (new SpecialValue(SpecialType.FalseVal).setContext(Context), null);
    }
    public override (ValueF, CustomError) AndedBy(ValueF other)
    {
        if (other is SpecialValue)
            return (new SpecialValue(TypeVal == SpecialType.TrueVal && ((SpecialValue)other).TypeVal == SpecialType.TrueVal ? SpecialType.TrueVal : SpecialType.FalseVal).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) OredBy(ValueF other)
    {
        if (other is SpecialValue)
            return (new SpecialValue(TypeVal == SpecialType.TrueVal || ((SpecialValue)other).TypeVal == SpecialType.TrueVal ? SpecialType.TrueVal : SpecialType.FalseVal).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) Notted()
    {
        if (TypeVal != SpecialType.NullVal)
            return (
                TypeVal == SpecialType.TrueVal?
                new SpecialValue(SpecialType.FalseVal) : 
                new SpecialValue(SpecialType.FalseVal)
                , null);
        return base.Notted();
    }
    public override bool isTrue()
    {
        if (TypeVal != SpecialType.NullVal)
            return TypeVal == SpecialType.TrueVal;
        return base.isTrue();
    }
    #endregion

}
public class ImageValue : ValueF
{
    Image<Bgr, byte> ImageData;
    public ImageValue(Image<Bgr, byte> imageData) : base()
    {
        this.ImageData = imageData;
    }
    public override ValueF Copy()
    {
        ImageValue copy = new ImageValue(ImageData.Clone());
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }
    public override string ToString() => $"Image[{ImageData.Width}, {ImageData.Height}]";
}
public class Number : ValueF
{
    public float? Value;

    public Number(object value):base()
    {
        this.Value = float.Parse(value.ToString());
    }

    public override ValueF Copy()
    {
        Number copy = new Number(Value);
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }
    public override string ToString()
    {
        return Value.ToString();
    }

    #region Mathematical expresion methods
    public override (ValueF, CustomError) AddedTo(ValueF other){
        if (other is Number)
            return (new Number(Value + ((Number)other).Value).setContext(this.Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) SubbedBy(ValueF other)
    {
        if (other is Number)
            return (new Number(Value - ((Number)other).Value).setContext(this.Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) MultedBy(ValueF other)
    {
        if (other is Number)
            return (new Number(Value * ((Number)other).Value).setContext(this.Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) DivBy(ValueF other)
    {
        if (other is Number)
        {
            if (((Number)other).Value == 0)
                return (null, new RTError(other.PosStart, other.PosEnd, "Division by zero", this.Context));
            return (new Number(Value / ((Number)other).Value).setContext(this.Context), null);
        }
        else
            return (null, IllegalOperation(other));

    }
    public override (ValueF, CustomError) PoweredBy(ValueF other)
    {
        if (other is Number)
            return (new Number(MathF.Pow((float)Value, (float)((Number)other).Value)).setContext(this.Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonEE(ValueF other)
    {
        if (other is Number)
            return (new Number(Value == ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonNE(ValueF other)
    {
        if (other is Number)
            return (new Number(Value != ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonLT(ValueF other)
    {
        if (other is Number)
            return (new Number(Value < ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonGT(ValueF other)
    {
        if (other is Number)
            return (new Number(Value > ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonLTE(ValueF other)
    {
        if (other is Number)
            return (new Number(Value <= ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) GetComparisonGTE(ValueF other)
    {
        if (other is Number)
            return (new Number(Value >= ((Number)other).Value ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) AndedBy(ValueF other)
    {
        if (other is Number)
            return (new Number(Value == 1 && ((Number)other).Value == 1 ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) OredBy(ValueF other)
    {
        if (other is Number)
            return (new Number(Value == 1 || ((Number)other).Value == 1 ? 1 : 0).setContext(Context), null);
        else
            return (null, IllegalOperation(other));
    }
    public override (ValueF, CustomError) Notted()
    {
        return (new Number(Value == 0? 1 : 0), null);
    }
    public override bool isTrue()
    {
        return Value != 0;
    }
    #endregion
}
public class StringValue : ValueF
{
    public string Value;

    public StringValue(string value): base()
    {
        Value = value;
    }

    #region Mathematical expresion methods
    public override (ValueF, CustomError) AddedTo(ValueF other)
    {
        if (other is StringValue)
            return (new StringValue(Value + ((StringValue)other).Value), null);
        return base.AddedTo(other);
    }

    public override (ValueF, CustomError) MultedBy(ValueF other)
    {
        if (other is Number)
            return (new StringValue(String.Concat(Enumerable.Repeat(Value, (int)((Number)other).Value))), null);
        return base.MultedBy(other);
    }

    public override bool isTrue() => Value.Length > 0;

    #endregion

    public override ValueF Copy()
    {
        StringValue copy = new StringValue(Value);
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }

    public override (ValueF, CustomError) GetComparisonEE(ValueF other)
    {
        return base.GetComparisonEE(other);
    }

    public override string ToString() => Value;

}
public class ListValue : ValueF
{
    public List<ValueF> Elements;

    public ListValue(List<ValueF> elements):base()
    {
        Elements = elements;
    }

    public override (ValueF, CustomError) AddedTo(ValueF other)
    {
        if(other is ListValue)
        {

            List<ValueF> newList = Elements.ToArray().ToList<ValueF>();
            newList = newList.Concat<ValueF>(((ListValue)other).Elements).ToList();
            return (new ListValue(newList), null);
        }
        else
        {
            List<ValueF> newList = Elements.ToArray().ToList<ValueF>();
            newList.Add(other);
            return (new ListValue(newList), null);
        }
    }

    public override (ValueF, CustomError) SubbedBy(ValueF other)
    {
        if (other is Number)
        {
            List<ValueF> newList = Elements.ToArray().ToList<ValueF>();
            newList.RemoveAt((int)((Number)other).Value);// TODO: error if not found
            return (new ListValue(newList), null);
        }
        return base.SubbedBy(other);
    }

    public override (ValueF, CustomError) DivBy(ValueF other)
    {
        if(other is Number)
        {
            return (Elements[(int)((Number)other).Value], null);//TODO: if not found return error
        }
        return base.DivBy(other);
    }

    public override ValueF Copy()
    {
        ListValue copy = new ListValue(Elements.ToArray().ToList());
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }

    public override string ToString()
    {
        string ret = "[";
        for(int i = 0; i < Elements.Count; i++)
        {

            ret += (Elements[i].ToString());

            ret += i == Elements.Count - 1 ? "" : ", ";
        }

        ret += "]";


        return ret;
    }   

}

public class BaseFunction : ValueF
{
    protected string Name;
    public BaseFunction(string name):base()
    {
        Name = name != null ? name : "<anonymous>";
    }

    protected ContextHolder GenerateNewContext()
    {
        ContextHolder newContext = new ContextHolder(Name, Context, PosStart);
        newContext.symbolTable = new SymbolTable(newContext.parent.symbolTable);
        return newContext;
    }
    protected RTResult CheckArgs(List<string> argNames, List<ValueF> args)
    {
        RTResult res = new RTResult();
        if (args.Count > argNames.Count)
            return res.failure(
                new RTError(
                    PosStart, PosEnd,
                    $"{args.Count - argNames.Count} too many args passed into {Name}",
                    Context));


        if (args.Count < argNames.Count)
            return res.failure(
                new RTError(
                    PosStart, PosEnd,
                    $"{argNames.Count - args.Count} too few args passed into {Name}",
                    Context));

        return res.success(null);
    }
    protected void PopulateArgs(List<string> argNames, List<ValueF> args, ContextHolder execCtx)
    {
        for (int i = 0; i < args.Count; i++)
        {
            string argName = argNames[i];
            ValueF argValue = args[i];
            argValue.setContext(execCtx);
            execCtx.symbolTable.Set(argName, argValue);
        }
    }
    protected RTResult CheckAndPopulateArgs(List<string> argNames, List<ValueF> args, ContextHolder execCtx)
    {
        RTResult res = new RTResult();
        res.register(CheckArgs(argNames, args));
        if (res.HasError) return res;
        PopulateArgs(argNames, args, execCtx);
        return res.success(null);
    }


}

public class Function : BaseFunction
{
    Node BodyNode;
    List<string> ArgNames;
    bool ShouldReturnNull;

    public Function(Node bodyNode, List<string> argNames, string name, bool shouldReturnNull):base(name)
    {
        BodyNode = bodyNode;
        this.ArgNames = argNames;
        this.ShouldReturnNull = shouldReturnNull;
    }

    public override RTResult Execute(List<ValueF> args)//POTENTIAL ERROR: not sure what type this arg is supossed to be
    {
        RTResult res = new RTResult();
        ContextHolder execCtx = GenerateNewContext();

        res.register(CheckAndPopulateArgs(ArgNames, args, execCtx));
        if (res.HasError) return res;

        ValueF value = res.register(Interpreter.Visit(BodyNode, execCtx));
        if (res.HasError) return res;

        return res.success(ShouldReturnNull? new SpecialValue(SpecialValue.SpecialType.NullVal) : value);
    }

    public override ValueF Copy()
    {
        ValueF copy = new Function(BodyNode, ArgNames, Name, ShouldReturnNull);

        copy.setContext(Context);

        copy.SetPos(PosStart, PosEnd);

        return copy;
    }

    public override string ToString()
    {
        return $"<function {Name}>";
    }

}

public class BuiltInFun : BaseFunction
{
    Dictionary<string, List<string>> argNamesDictionary = new Dictionary<string, List<string>>();
    public BuiltInFun(string name) : base(name)
    {
        //Dictionary of arguments
        argNamesDictionary.Add("print", new List<string>(new string[] { "value"}));
        argNamesDictionary.Add("toString", new List<string>(new string[] { "convertable" }));
        argNamesDictionary.Add("delay", new List<string>(new string[] { "time" }));
        argNamesDictionary.Add("moveMouse", new List<string>(new string[] { "x", "y" }));
        argNamesDictionary.Add("sin", new List<string>(new string[] { "value"}));
        argNamesDictionary.Add("press", new List<string>(new string[] { "keyCode"}));
        argNamesDictionary.Add("cursorClick", new List<string>(new string[] { }));
        argNamesDictionary.Add("startApp", new List<string>(new string[] { "appPath" }));
        //argNamesDictionary.Add("findImage", new List<string>(new string[] { "posList" }));

    }

    public override RTResult Execute(List<ValueF> args)
    {
        RTResult res = new RTResult();
        ContextHolder execCtx = GenerateNewContext();

        string methodName = $"execute_{Name}";


        Type thisType = typeof(BuiltInFun);
        MethodInfo theMethod = thisType.GetMethod(methodName);

        if (theMethod == null)
        {
            NoVisitMethod(methodName);
            return null;
        }

        res.register(CheckAndPopulateArgs(argNamesDictionary[Name], args, execCtx));
        if (res.HasError) return res;

        
        RTResult result = (RTResult)theMethod.Invoke(null, new object[] { execCtx });//executes method based on Name
        ValueF returnVal = res.register(result);
        if (res.HasError) return res;

        return res.success(returnVal);
    }
    //TODO: add usefull build-in functions
    #region built-in funcs

    public static RTResult execute_startApp(ContextHolder execCtx)
    {
        ValueF valueF = execCtx.symbolTable.get("appPath");
        string appPath = valueF.ToString();
        try
        {
            Process.Start(appPath);
        }
        catch (Win32Exception ex)
        {
            return new RTResult().failure(new RTError(valueF.PosStart, valueF.PosEnd, ex.Message, execCtx));
        }
        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));
    }


    public static RTResult execute_cursorClick(ContextHolder execCtx)
    {
        Win32.CursorClick();
        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));
    }

    public static RTResult execute_press(ContextHolder execCtx)
    {
        ValueF parameter = execCtx.symbolTable.get("keyCode");

        if (parameter is Number == false)//Checks if keyCode is number
            return new RTResult().failure(
                new RTError(parameter.PosStart, parameter.PosEnd,
                    "Epected Int", execCtx));

        int keyCode = (int)((Number)parameter).Value;

        Win32.PressKey(keyCode);

        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));
    }
    public static RTResult execute_sin(ContextHolder execCtx)
    {
        ValueF parameter = execCtx.symbolTable.get("value");

        if (parameter is Number == false)//Checks if x is number
            return new RTResult().failure(
                new RTError(parameter.PosStart, parameter.PosEnd,
                    "Epected Float or Int", execCtx));

        float floatVal = (float)((Number)parameter).Value;
        float sinOfVal = MathF.Sin(floatVal);
        return new RTResult().success(new Number(sinOfVal).setContext(execCtx));
    }
    public static RTResult execute_moveMouse(ContextHolder execCtx)
    {
        ValueF xPosVal = execCtx.symbolTable.get("x");
        ValueF yPosVal = execCtx.symbolTable.get("y");

        if(xPosVal is Number == false)//Checks if x is number
            return new RTResult().failure(
                new RTError(xPosVal.PosStart, xPosVal.PosEnd,
                    "Epected Float or Int", execCtx));

        if (yPosVal is Number == false)//Checks if y is number
            return new RTResult().failure(
                new RTError(yPosVal.PosStart, yPosVal.PosEnd,
                    "Epected Float or Int", execCtx));

        int xPos = (int)((Number)xPosVal).Value;
        int yPos = (int)((Number)yPosVal).Value;

        Win32.POINT p = new Win32.POINT(xPos, yPos);
        Win32.SetCursorPos(p.x, p.y);//moves Cursor to position

        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));

    }
    public static RTResult execute_print(ContextHolder execCtx)
    {
        Console.WriteLine(execCtx.symbolTable.get("value"));
        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));
    }

    public static RTResult execute_delay(ContextHolder execCtx)
    {
        ValueF value = execCtx.symbolTable.get("time");//Gets required parameters

        if (!(value is Number))
            return new RTResult().failure(
                new RTError(value.PosStart, value.PosEnd,
                    "Epected Float or Int", execCtx));
        int sleepTime = (int)((Number)value).Value;
        Thread.Sleep(sleepTime);
        return new RTResult().success(new SpecialValue(SpecialValue.SpecialType.NullVal));
    }

    public static RTResult execute_toString(ContextHolder execCtx) => new RTResult().success(new StringValue(execCtx.symbolTable.get("convertable").ToString()));
    #endregion
    private void NoVisitMethod(string name)
    {
        throw new Exception($"No {name} method defined");
    }
    public override ValueF Copy()
    {
        BuiltInFun copy = new BuiltInFun(Name);
        copy.SetPos(PosStart, PosEnd);
        copy.setContext(Context);
        return copy;
    }
    public override string ToString() => $"<built-in function {Name}>";

}
#endregion
public class ContextHolder
{
    public string displayName;
    public ContextHolder parent;
    public Position parentEtrPos;
    public SymbolTable symbolTable;

    public ContextHolder(string displayName, ContextHolder parent = null, Position parentEtrPos = null)
    {
        this.displayName = displayName;
        this.parent = parent;
        this.parentEtrPos = parentEtrPos;
    }
}
public class SymbolTable {
    Dictionary<string, ValueF> Symbols = new Dictionary<string, ValueF>();
    SymbolTable Parent;

    public SymbolTable(SymbolTable parent = null)
    {
        Parent = parent;
    }


    //Finds the variable in Symbols tree
    public ValueF get(string name)
    {
        if (!Symbols.ContainsKey(name) && Parent != null)
        {
            return Parent.get(name);
        }else if(!Symbols.ContainsKey(name) && Parent == null)
        {
            return null;
        }

        return Symbols[name];
    }

    public void Set(string name, ValueF value)
    {
        if (!Symbols.ContainsKey(name))
            Symbols.Add(name, value);
        else
            Symbols[name] = value;
    }

    public void Remove(string name)
    {
        if (Symbols.ContainsKey(name))
        {
            Symbols.Remove(name);
        }

    }
}
static class Interpreter
{
    public static RTResult Visit(Node node, ContextHolder context) {
        string method_name = $"Visit_{node.GetType().Name}";

        Type thisType = typeof(Interpreter);
        MethodInfo theMethod = thisType.GetMethod(method_name);
        object[] para = { node, context };



        if(theMethod == null)
        {
            NoVisitMethod(node, context);
            return null;
        }

        return (RTResult)theMethod.Invoke(null, para);

    }
    public static void NoVisitMethod(Node node, ContextHolder context)
    {
        throw new Exception($"No Visit_{node.GetType().Name} method defined");
    }
    public static RTResult Visit_VarAccessNode(Node node, ContextHolder context)
    {
        RTResult res = new RTResult();

        string varName = node.tok.value.ToString();
        ValueF value = context.symbolTable.get(varName);

        if (value == null)
        {
           return res.failure(new RTError(node.PosStart, node.PosEnd, $"{varName} is not defined", context));
        }

        value = value.Copy().SetPos(node.PosStart, node.PosEnd).setContext(context);

        return res.success(value);
    }
    public static RTResult Visit_VarAssignNode(Node node, ContextHolder context)
    {
        RTResult res = new RTResult();
        VarAssignNode varAssignNode = (VarAssignNode)node;

        //Console.WriteLine(varAssignNode.VarNameTok.ToString());

        string varName = varAssignNode.VarNameTok.value.ToString();
        ValueF value = res.register(Visit(varAssignNode.ValueNode, context));
        if (res.HasError) return res;

        context.symbolTable.Set(varName, value);
        return res.success(value);
    }
    public static RTResult Visit_NumberNode(NumberNode node, ContextHolder context)
    {
        return new RTResult().success(new Number(node.tok.value).
            setContext(context).SetPos(node.PosStart, node.PosEnd)
            );
    }
    public static RTResult Visit_StringNode(StringNode node, ContextHolder context)
    {
        return new RTResult().success(
            new StringValue(node.tok.value.ToString())
            .setContext(context)
            .SetPos(node.PosStart, node.PosEnd)
            );
    }
    public static RTResult Visit_BinOpNode(BinOpNode node, ContextHolder context)
    {
        RTResult res = new RTResult();


        ValueF left = res.register(Visit(node.left_node, context));
        if (res.HasError) return res;

        ValueF right = res.register(Visit(node.right_node, context));
        if (res.HasError) return res;

        (ValueF, CustomError) result = (null, null);

        switch (node.tok.type)
        {
            case MainScript.TT_PLUS:    result = left.AddedTo(right); break;
            case MainScript.TT_MINUS:   result = left.SubbedBy(right); break;
            case MainScript.TT_MUL:     result = left.MultedBy(right); break;
            case MainScript.TT_DIV:     result = left.DivBy(right); break;
            case MainScript.TT_POW:     result = left.PoweredBy(right); break;
            case MainScript.TT_EE:      result = left.GetComparisonEE(right); break;
            case MainScript.TT_NE:      result = left.GetComparisonNE(right); break;
            case MainScript.TT_LT:      result = left.GetComparisonLT(right); break;
            case MainScript.TT_GT:      result = left.GetComparisonGT(right); break;
            case MainScript.TT_LTE:      result = left.GetComparisonLTE(right); break;
            case MainScript.TT_GTE:      result = left.GetComparisonGTE(right); break;
            default:

                if (node.tok.Matches(MainScript.TT_KEYWORD, "AND"))
                    result = left.AndedBy(right);
                else if(node.tok.Matches(MainScript.TT_KEYWORD, "OR"))
                    result = left.OredBy(right);

                break;
        }

        if (result.Item2 != null)
            return res.failure(result.Item2);
        else
            return res.success(result.Item1.SetPos(node.PosStart, node.PosEnd));

    }
    public static RTResult Visit_UnaryOpNode(UnaryOpNode node, ContextHolder context)
    {
        RTResult res = new RTResult();


        ValueF numb = res.register(Visit(node.node, context));
        if (res.HasError) return res;


        (ValueF, CustomError) result = (numb, null);


        if (node.tok.type == MainScript.TT_MINUS)
            result = numb.MultedBy(new Number(-1));
        else if (node.tok.Matches(MainScript.TT_KEYWORD, "NOT"))
            result = numb.Notted();


        if (res.HasError) return res.failure(result.Item2);
        return res.success(result.Item1.SetPos(node.PosStart, node.PosEnd));
    }
    public static RTResult Visit_IfThenNode(IfThenNode node, ContextHolder context)
    {
        RTResult res = new RTResult();

        foreach((Node, Node, bool) conNExpr in node.cases)
        {
            ValueF conditionValue = res.register(Visit(conNExpr.Item1, context));
            if (res.HasError) return res;

            if (conditionValue.isTrue())
            {
                ValueF exprVal = res.register(Visit(conNExpr.Item2, context));
                if (res.HasError) return res;
                return res.success(conNExpr.Item3 ? new SpecialValue(SpecialValue.SpecialType.NullVal): exprVal);

            }
        }
        if (node.elseCase.Item1 != null)
        {
            ValueF elseVal = res.register(Visit(node.elseCase.Item1, context));
            if (res.HasError) return res;
            return res.success(node.elseCase.Item2 ? new SpecialValue(SpecialValue.SpecialType.NullVal) : elseVal);
        }
        return res.success(new SpecialValue(SpecialValue.SpecialType.NullVal));

    }
    public static RTResult Visit_ForNode(ForNode node, ContextHolder context)
    {
        RTResult res = new RTResult();

        Number stepVal;

        Number startVal = (Number)res.register(Visit(node.StartValue, context));
        if (res.HasError) return res;


        Number endVal = (Number)res.register(Visit(node.EndValue, context));
        if (res.HasError) return res;


        if(node.StepVal != null)
        {
            stepVal = (Number)res.register(Visit(node.StepVal, context));
            if (res.HasError) return res;
        }
        else
        {
            stepVal = new Number(1);
        }

        float? i = startVal.Value;

        while (stepVal.Value >= 0 ? i < endVal.Value: i > endVal.Value)//if steps are negative it will check for smaller then i
        {
            context.symbolTable.Set(node.tok.value.ToString(), new Number(i));
            i += stepVal.Value;

            res.register(Visit(node.BodyVal, context));
            if (res.HasError) return res;
        }
        return res.success(new SpecialValue(SpecialValue.SpecialType.NullVal));//Allways return null

    }
    public static RTResult Visit_WhileNode(WhileNode node, ContextHolder context)
    {
        RTResult res = new RTResult(); 

        while (true)
        {
            ValueF condition = res.register(Visit(node.Condition, context));
            if (res.HasError) return res;

            if (!condition.isTrue())
                break;

            res.register(Visit(node.BodyVal, context));
            if (res.HasError) return res;
        }
        return res.success(new SpecialValue(SpecialValue.SpecialType.NullVal));//allways returns null
    }
    public static RTResult Visit_FunDefNode(FunDefNode node, ContextHolder context)
    {
        RTResult res = new RTResult();

        string funName = node.tok != null ? node.tok.value.ToString() : null;
        Node bodyNode = node.bodyNode;
        List<string> argNames = new List<string>();
        node.argNameToks.ToList().ForEach(x => argNames.Add(x.value.ToString()));
        Function FuncValue = (Function)new Function(bodyNode, argNames, funName, node.ShouldReturnNull)
            .setContext(context)
            .SetPos(node.PosStart, node.PosEnd);

        if (node.tok != null)
            context.symbolTable.Set(funName, FuncValue);

        return res.success(FuncValue);
    }
    public static RTResult Visit_CallNode(CallNode node, ContextHolder context)
    {
        RTResult res = new RTResult();

        List<ValueF> args = new List<ValueF>();

        ValueF valueToCall = res.register(Visit(node.NodeToCall, context));
        if (res.HasError) return res;
        valueToCall = valueToCall.Copy().SetPos(node.PosStart, node.PosEnd);

        foreach(Node argNode in node.argNodes)
        {
            args.Add(res.register(Visit(argNode, context)));
            if (res.HasError) return res;
        }

        ValueF returnValue = res.register(valueToCall.Execute(args));
        if (res.HasError) return res;

        returnValue = returnValue.Copy().SetPos(node.PosStart, node.PosEnd).setContext(context);//TODO: Test what removing this method does

        return res.success(returnValue);
    }
    public static RTResult Visit_ListNode(ListNode node, ContextHolder context)
    {
        RTResult res = new RTResult();
        List<ValueF> elements = new List<ValueF>();

        foreach(Node elNode in node.ElementNodes)
        {
            elements.Add(res.register(Visit(elNode, context)));
            if (res.HasError) return res;
        }

        return res.success(new ListValue(elements)
            .setContext(context)
            .SetPos(node.PosStart, node.PosEnd));
    }
}
