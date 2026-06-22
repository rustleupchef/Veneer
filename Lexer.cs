namespace Veneer;

public class Lexer
{
    public static readonly Dictionary<string, Tokens.TokenType> Keywords = new()
    {
        { "for",      Tokens.TokenType.For },
        { "while",    Tokens.TokenType.While },
        { "if",       Tokens.TokenType.If },
        { "else",     Tokens.TokenType.Else },
        { "return",   Tokens.TokenType.Return },
        { "int",      Tokens.TokenType.Int },
        { "float",    Tokens.TokenType.Float },
        { "bool",     Tokens.TokenType.Bool },
        { "char",     Tokens.TokenType.Char },
        { "void",     Tokens.TokenType.Void },
        { "string",   Tokens.TokenType.String },
        { "tooth",    Tokens.TokenType.Tooth },
        { "language", Tokens.TokenType.Language }
    };

    // 2. Operators & Punctuation Dictionary: For literal code symbols
    public static readonly Dictionary<string, Tokens.TokenType> OperatorsAndPunctuation = new()
    {
        // Arithmetic & Assignment
        { "+",  Tokens.TokenType.Plus },
        { "-",  Tokens.TokenType.Minus },
        { "*",  Tokens.TokenType.Star },
        { "/",  Tokens.TokenType.Slash },
        { "%",  Tokens.TokenType.Modulo },
        { "=",  Tokens.TokenType.Assign },

        // Comparison
        { "==", Tokens.TokenType.EqualEqual },
        { "!=", Tokens.TokenType.NotEqual },
        { "<",  Tokens.TokenType.LessThan },
        { "<=", Tokens.TokenType.LessEqual },
        { ">",  Tokens.TokenType.GreaterThan },
        { ">=", Tokens.TokenType.GreaterEqual },

        // Logical
        { "&&", Tokens.TokenType.AmpAmp },
        { "||", Tokens.TokenType.PipePipe },
        { "!",  Tokens.TokenType.Bang },

        // Punctuation & Delimiters
        { "(",  Tokens.TokenType.LeftParen },
        { ")",  Tokens.TokenType.RightParen },
        { "{",  Tokens.TokenType.LeftBrace },
        { "}",  Tokens.TokenType.RightBrace },
        { "[",  Tokens.TokenType.LeftBracket },
        { "]",  Tokens.TokenType.RightBracket },
        { ",",  Tokens.TokenType.Comma },
        { ";",  Tokens.TokenType.SemiColon }
    };
    
    public static Tokens.TokenType ResolveLiteralOrIdentifier(string text)
    {
        // 1. Check if it's a known keyword
        if (Keywords.TryGetValue(text, out var type))
        {
            return type;
        }

        // 2. Check for Boolean Literals
        if (text == "true" || text == "false")
        {
            return Tokens.TokenType.BoolLiteral;
        }

        // 3. Check for String Literals (Wrapped in double quotes)
        if (text.StartsWith("\"") && text.EndsWith("\"") && text.Length >= 2)
        {
            return Tokens.TokenType.StringLiteral;
        }

        // 4. Check for Character Literals (Wrapped in single quotes, e.g., 'a')
        if (text.StartsWith("'") && text.EndsWith("'") && text.Length == 3)
        {
            return Tokens.TokenType.CharLiteral; // Assuming CharLiteral is added to your enum
        }

        // 5. Check for Numeric Literals (Int vs Float)
        if (char.IsDigit(text[0]) || (text.StartsWith("-") && text.Length > 1 && char.IsDigit(text[1])))
        {
            // If it contains a decimal point, it's a float literal
            if (text.Contains('.'))
            {
                if (float.TryParse(text, out _)) return Tokens.TokenType.FloatLiteral;
                return Tokens.TokenType.Invalid;
            }
            
            // Otherwise, it's an integer literal
            if (int.TryParse(text, out _)) return Tokens.TokenType.IntLiteral;
            return Tokens.TokenType.Invalid;
        }

        // 6. Default: If it's valid text but not a keyword, it's a user-defined name
        if (IsValidIdentifier(text))
        {
            return Tokens.TokenType.Identifier;
        }

        return Tokens.TokenType.Invalid;
    }

    private static bool IsValidIdentifier(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        // C-like identifiers must start with a letter or underscore
        if (!char.IsLetter(text[0]) && text[0] != '_') return false;

        // Remaining characters can be letters, digits, or underscores
        return text.All(c => char.IsLetterOrDigit(c) || c == '_');
    }
}