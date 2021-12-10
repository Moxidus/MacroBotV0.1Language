using MacroBotV0._1Language;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;

public static class MainScript
{
    //Global symbol table
    public static SymbolTable GlobalSymbolTable = new SymbolTable(); //not setting null to 0

    //digit constants
    public static string DIGITS = "0123456789";
    public static string LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz_";
    public static string LETTERS_DIGITS = LETTERS + DIGITS;

    //tokens constants
    public const string TT_INT = "TT_INT";
    public const string TT_FLOAT = "TT_FLOAT";
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
    public const string TT_EE = "TT_EE";
    public const string TT_NE = "TT_NE";
    public const string TT_LT = "TT_LT";
    public const string TT_GT = "TT_GT";
    public const string TT_LTE = "TT_LTE";
    public const string TT_GTE = "TT_GTE";
    public const string TT_EOF = "TT_EOF";


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
        "WHILE"
    };



    //Run-------------------------------------------------------------
    public static (Number, CustomError) Run(string fn, string text)
    {
        //setting global VARIABLES
        GlobalSymbolTable.Set("NULL", new Number(0));
        GlobalSymbolTable.Set("TRUE", new Number(1));
        GlobalSymbolTable.Set("FALSE", new Number(0));



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
        Interpreter interpreter = new Interpreter();
        Context context = new Context("<Program>");
        context.symbolTable = GlobalSymbolTable;
        RTResult result = interpreter.Visit(ast.node, context);


        return (result.value, result.error);
    }


}


//#################################################################
// RunTime Result
//#################################################################
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



//#################################################################
// Nodes
//#################################################################
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
public class VarIfThenNode : Node
{
    public List<(Node, Node)> cases;
    public Node elseCase;

    public VarIfThenNode(List<(Node, Node)> cases, Node elseCase): base(null)
    {
        this.cases = cases;
        this.elseCase = elseCase;

        PosStart = cases[0].Item1.PosStart;
        PosEnd = elseCase != null ? elseCase.PosEnd : cases[cases.Count - 1].Item2.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class ForNode : Node
{
    public Node StartValue;
    public Node EndValue;
    public Node BodyVal;
    public Node StepVal;
    public ForNode(Token varName, Node startValue, Node endValue, Node bodyVal, Node stepVal) : base(varName)
    {
        this.StartValue = startValue;
        this.EndValue = endValue;
        this.BodyVal = bodyVal;
        this.StepVal = stepVal;

        this.PosStart = varName.posStart;
        this.PosEnd = BodyVal.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}
public class WhileNode : Node
{
    public Node Condition;
    public Node BodyVal;
    public WhileNode(Node condition, Node bodyVal) : base(null)
    {
        this.Condition = condition;
        this.BodyVal = bodyVal;

        this.PosStart = condition.PosStart;
        this.PosEnd = BodyVal.PosEnd;
    }

    //public override string ToString() => $"({tok}, {node})";
}

//#################################################################
// Parse result
//#################################################################
class ParseResult
{
    public CustomError error;
    public Node node;
    public int advanceCount = 0;


    public void registerAdvancement()
    {
        advanceCount++;
    }

    public Node register(ParseResult res) {
        advanceCount += res.advanceCount;
        if (res.error != null)
            this.error = res.error;
        return res.node;
    }


    public ParseResult success(Node node) {
        this.node = node;
        return this;
    }
    public ParseResult failure(CustomError error) {
        if(this.error == null || this.advanceCount == 0) 
            this.error = error;
        return this;
    }

}


//#################################################################
// Parser
//#################################################################
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
            res.registerAdvancement();
            advance();
            return res.success(new NumberNode(tok));
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
            if (res.error != null)
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
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "IF"))//checks for if expresions
        {
            Node ifExprtemp = res.register(ifExpr());
            if (res.error != null)// ERROR check
                return res;
            return res.success(ifExprtemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "FOR"))
        {
            Node forExprtemp = res.register(forExpr());
            if (res.error != null)// ERROR check
                return res;
            return res.success(forExprtemp);
        }
        else if (current_tok.Matches(MainScript.TT_KEYWORD, "WHILE"))
        {
            Node whileExprtemp = res.register(whileExpr());
            if (res.error != null)// ERROR check
                return res;
            return res.success(whileExprtemp);
        }


        return res.failure(
            new InvalidSyntaxError(
                tok.posStart,
                tok.posEnd,
                "Expected int, float, identifier, \"+\", \"-\" or \"(\""));
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
        if (res.error != null)// ERROR check
            return res;


        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'THEN'"));


        res.registerAdvancement(); // Steps over THEN
        advance();



        // Registers the expresion right next to the "THEN"
        body = res.register(this.expr());
        if (res.error != null)// ERROR check
            return res;

        return res.success(new WhileNode(condition, body));

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
        if (res.error != null)// ERROR check
            return res;


        if (!current_tok.Matches(MainScript.TT_KEYWORD, "TO"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'TO'"));


        res.registerAdvancement();// Steps over TO
        advance();



        endVal = res.register(expr());
        if (res.error != null)// ERROR check
            return res;


        if (current_tok.Matches(MainScript.TT_KEYWORD, "STEP"))
        {
            res.registerAdvancement();// Steps over TO
            advance();

            stepNode = res.register(expr());
            if (res.error != null)// ERROR check
                return res;
        }

        if (!current_tok.Matches(MainScript.TT_KEYWORD, "THEN"))
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart,
                current_tok.posEnd,
                "Expected 'THEN'"));

        res.registerAdvancement();// Steps over THEN
        advance();

        bodyNode = res.register(expr());
        if (res.error != null)// ERROR check
            return res;

        return res.success(new ForNode(varName, startVal, endVal, bodyNode, stepNode));
    }

    private ParseResult ifExpr()
    {
        ParseResult res = new ParseResult();
        List<(Node, Node)> cases = new List<(Node, Node)>();
        Node elseCase = null;

        //Position posStart = current_tok.posStart.copy();
        res.registerAdvancement();
        advance();

        // Registers the expresion right next to the "IF"
        Node condition = res.register(this.expr());
        if (res.error != null)// ERROR check
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
        if (res.error != null)// ERROR check
            return res;
        cases.Add((condition, exprTemp));

        while(current_tok.Matches(MainScript.TT_KEYWORD, "ELIF"))
        {
            res.registerAdvancement();
            advance();

            condition = res.register(expr());
            if (res.error != null)// ERROR check
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
            if (res.error != null)// ERROR check
                return res;

            cases.Add((condition, exprTemp));
        }

        if(current_tok.Matches(MainScript.TT_KEYWORD, "ELSE"))
        {

            res.registerAdvancement();
            advance();

            exprTemp = res.register(expr());
            if (res.error != null)// ERROR check
                return res;
            elseCase = exprTemp;
        }

        return res.success(new VarIfThenNode(cases, elseCase));
    }

    ParseResult factor()
    {
        ParseResult res = new ParseResult();
        Token tok = current_tok;

        if( tok.type == MainScript.TT_MINUS || tok.type == MainScript.TT_PLUS)
        {
            res.registerAdvancement();
            advance();
            Node factorTemp = res.register(factor());
            if (res.error != null)
                return res;
            return res.success(new UnaryOpNode(tok, factorTemp));

        }

        return Power();
    }

    ParseResult Power() => binOp(atom, new(string, string)[] { (MainScript.TT_POW, null) },factor);

    ParseResult term() => binOp(factor, new (string, string)[] { (MainScript.TT_MUL, null), (MainScript.TT_DIV, null) });

    ParseResult expr()
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
            if (res.error != null)// ERROR check
                return res;

            return res.success(new VarAssignNode(varName, exTemp));
        }

        Node node = res.register(binOp(compExpr, new (string, string)[] { (MainScript.TT_KEYWORD, "AND"), (MainScript.TT_KEYWORD, "OR") }));
        if (res.error != null)
            return res.failure(new InvalidSyntaxError(
                current_tok.posStart, current_tok.posEnd,
                "Expected 'VAR', int, float, identifier, '+', '-', or '('"));

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
            if (res.error != null)
                return res;
            return res.success(new UnaryOpNode(opTok, nodeNot));
        }

        Node node = res.register(binOp(arithExpr, new (string, string)[] {(MainScript.TT_EE, null), (MainScript.TT_NE, null), (MainScript.TT_LT, null), (MainScript.TT_GT, null), (MainScript.TT_GTE, null), (MainScript.TT_LTE, null) }));

        if (res.error != null)
            return res.failure(
                new InvalidSyntaxError(
                    node.PosStart,
                    node.PosEnd,
                    "Expected int, float, identifier, '+', '-', '(' or ''"));

        return res.success(node);

    }

    private ParseResult arithExpr()
    {
        return binOp(term, new (string, string)[] { (MainScript.TT_PLUS, null), (MainScript.TT_MINUS, null) });
    }

    ParseResult binOp(Func<ParseResult> funcA, (string, string)[] ops, Func<ParseResult> funcB = null)
    {
        if (funcB == null)
            funcB = funcA;

        ParseResult res = new ParseResult();
        Node left = res.register(funcA());// Find left node
        if (res.error != null)// ERROR check
            return res;

        while (ops.Any(x => x.Item1 == current_tok.type && current_tok.value == null) ||
               ops.Any(x => x.Item1 == current_tok.type && x.Item2 == current_tok.value.ToString()))
        {
            Token op_tok = current_tok;
            res.registerAdvancement();
            advance();
            Node right = res.register(funcB());// Finds right node
            if (res.error != null)// ERROR check
                return res;
            left = new BinOpNode(left, op_tok, right);
        }
        return res.success(left);
    }


}

//#################################################################
// Position
//#################################################################
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



//#################################################################
// LEXER
//#################################################################
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
                    case '(':
                        tokens.Add(new Token(MainScript.TT_LPAREN, posStart: this.pos));
                        advance();
                        break;
                    case ')':
                        tokens.Add(new Token(MainScript.TT_RPAREN, posStart: this.pos));
                        advance();
                        break;
                    case '!':
                        (Token, CustomError) tokNError = makeNotEquals();
                        if (tokNError.Item2 != null)
                            return (null, tokNError.Item2);
                        tokens.Add(tokNError.Item1);
                        break;
                    case '=':
                        tokens.Add(makeEquals());
                        break;
                    case '<':
                        tokens.Add(makeLessThen());
                        break;
                    case '>':
                        tokens.Add(makeGreaterThen());
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



//#################################################################
// TOKENS
//#################################################################
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



//#################################################################
// Values
//#################################################################
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

    public Number(float? value, Position posStart, Position posEnd, Context context)
    {

        this.Value = value;
        this.PosStart = posStart;
        this.PosEnd = posEnd;
        this.Context = context;
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

    public Number Copy()
    {
        return new Number(this.Value, PosStart, PosEnd, this.Context);
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

    internal (Number, CustomError) GetComparisonEE(Number other)
    {
        return (new Number(Value == other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) GetComparisonNE(Number other)
    {
        return (new Number(Value != other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) GetComparisonLT(Number other)
    {
        return (new Number(Value < other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) GetComparisonGT(Number other)
    {
        return (new Number(Value > other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) GetComparisonLTE(Number other)
    {
        return (new Number(Value <= other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) GetComparisonGTE(Number other)
    {
        return (new Number(Value >= other.Value ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) AndedBy(Number other)
    {
        return (new Number(Value == 1 && other.Value == 1 ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) OredBy(Number other)
    {
        return (new Number(Value == 1 || other.Value == 1 ? 1 : 0).setContext(Context), null);
    }

    internal (Number, CustomError) Notted()
    {
        return (new Number(Value == 0? 1 : 0), null);
    }

    internal bool isTrue()
    {
        return Value != 0;
    }
}


//#################################################################
// Context
//#################################################################
public class Context
{
    public string displayName;
    public Context parent;
    public Position parentEtrPos;
    public SymbolTable symbolTable;

    public Context(string displayName, Context parent = null, Position parentEtrPos = null)
    {
        this.displayName = displayName;
        this.parent = parent;
        this.parentEtrPos = parentEtrPos;
    }
}

//#################################################################
// SYMBOL Table
//#################################################################
public class SymbolTable {
    Dictionary<string, Number> Symbols = new Dictionary<string, Number>();
    SymbolTable Parent;

    //Finds the variable in Symbols tree
    public Number get(string name)
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

    public void Set(string name, Number value)
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


//#################################################################
// Interpreter
//#################################################################
class Interpreter
{
    public RTResult Visit(Node node, Context context) {
        string method_name = $"Visit_{node.GetType().Name}";

        Type thisType = this.GetType();
        MethodInfo theMethod = thisType.GetMethod(method_name);
        object[] para = { node, context };



        if(theMethod == null)
        {
            NoVisitMethod(node, context);
            return null;
        }

        return (RTResult)theMethod.Invoke(this, para);

    }
    public void NoVisitMethod(Node node, Context context)
    {
        throw new Exception($"No Visit_{node.GetType().Name} method defined");
    }
    public RTResult Visit_VarAccessNode(Node node, Context context)
    {
        RTResult res = new RTResult();

        string varName = node.tok.value.ToString();
        Number value = context.symbolTable.get(varName);

        if (value == null)
        {
           return res.failure(new RTError(node.PosStart, node.PosEnd, $"{varName} is not defined", context));
        }

        value = value.Copy().SetPos(node.PosStart, node.PosEnd);

        return res.success(value);
    }
    public RTResult Visit_VarAssignNode(Node node, Context context)
    {
        RTResult res = new RTResult();
        VarAssignNode varAssignNode = (VarAssignNode)node;

        Console.WriteLine(varAssignNode.VarNameTok.ToString());

        string varName = varAssignNode.VarNameTok.value.ToString();
        Number value = res.register(Visit(varAssignNode.ValueNode, context));

        if (res.error != null)
            return res;
        context.symbolTable.Set(varName, value);
        return res.success(value);
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
    public RTResult Visit_UnaryOpNode(UnaryOpNode node, Context context)
    {
        RTResult res = new RTResult();


        Number numb = res.register(Visit(node.node, context));
        if (res.error != null)
            return res;


        (Number, CustomError) result = (numb, null);


        if (node.tok.type == MainScript.TT_MINUS)
            result = numb.MultedBy(new Number(-1));
        else if (node.tok.Matches(MainScript.TT_KEYWORD, "NOT"))
            result = numb.Notted();


        if (res.error != null)
            return res.failure(result.Item2);
        else
            return res.success(result.Item1.SetPos(node.PosStart, node.PosEnd));
    }
    public RTResult Visit_VarIfThenNode(VarIfThenNode node, Context context)
    {
        RTResult res = new RTResult();

        foreach((Node, Node) conNExpr in node.cases)
        {
            Number conditionValue = res.register(Visit(conNExpr.Item1, context));
            if (res.error != null)// ERROR check
                return res;

            if (conditionValue.isTrue())
            {
                Number exprVal = res.register(Visit(conNExpr.Item2, context));
                if (res.error != null)// ERROR check
                    return res;
                return res.success(exprVal);

            }
        }
        if (node.elseCase != null)
        {
            Number elseVal = res.register(Visit(node.elseCase, context));
            if (res.error != null)// ERROR check
                return res;
            return res.success(elseVal);
        }
        return res.success(null);

    }


    public RTResult Visit_ForNode(ForNode node, Context context)
    {
        RTResult res = new RTResult();

        Number stepVal;

        Number startVal = res.register(Visit(node.StartValue, context));
        if (res.error != null)
            return res;


        Number endVal = res.register(Visit(node.EndValue, context));
        if (res.error != null)
            return res;


        if(node.StepVal != null)
        {
            stepVal = res.register(Visit(node.StepVal, context));
            if (res.error != null)
                return res;
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
            if (res.error != null)
                return res;
        }
        return res.success(null);

    }

    public RTResult Visit_WhileNode(WhileNode node, Context context)
    {
        RTResult res = new RTResult(); 

        while (true)
        {
            Number condition = res.register(Visit(node.Condition, context));
            if (res.error != null)
                return res;

            if (!condition.isTrue())
                break;

            res.register(Visit(node.BodyVal, context));
            if (res.error != null)
                return res;
        }
        return res.success(null);
    }


}
