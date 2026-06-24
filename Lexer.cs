namespace Veneer;

public class Lexer
{
    // 1. Keyword Dictionary: For lowercase words found in the source code
    public static readonly Dictionary<string, Tokens.TokenType> Keywords = new()
    {
        { "for", Tokens.TokenType.For },
        { "while", Tokens.TokenType.While },
        { "if", Tokens.TokenType.If },
        { "else", Tokens.TokenType.Else },
        { "return", Tokens.TokenType.Return },
        { "int", Tokens.TokenType.Int },
        { "float", Tokens.TokenType.Float },
        { "bool", Tokens.TokenType.Bool },
        { "char", Tokens.TokenType.Char },
        { "void", Tokens.TokenType.Void },
        { "string", Tokens.TokenType.String },
        { "func", Tokens.TokenType.Function },
        { "tooth", Tokens.TokenType.Tooth },
        { "language", Tokens.TokenType.Language },
        { "chip", Tokens.TokenType.Chip },
        { "class", Tokens.TokenType.Class },
        { "new", Tokens.TokenType.New },
        { "this", Tokens.TokenType.This },
        { "base", Tokens.TokenType.Base },
        { "public", Tokens.TokenType.Public },
        { "private", Tokens.TokenType.Private },
        { "protected", Tokens.TokenType.Protected },
        { "internal", Tokens.TokenType.Internal },
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
        { ";",  Tokens.TokenType.SemiColon },
        { ".",  Tokens.TokenType.Dot },
        { ":",  Tokens.TokenType.Colon }
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
    
    public static List<Tokens.Token> LexText(string source)
    {
        var tokens = new List<Tokens.Token>();
        int i = 0;

        while (i < source.Length)
        {
            char current = source[i];

            // 1. Skip Whitespace
            if (char.IsWhiteSpace(current))
            {
                i++;
                continue;
            }

            // 2. Handle String Literals (Everything inside " ")
            if (current == '"')
            {
                int start = i;
                i++; // Move past opening quote
                while (i < source.Length && source[i] != '"')
                {
                    i++;
                }
                i++; // Move past closing quote
                
                string lexeme = source.Substring(start, i - start);
                tokens.Add(new Tokens.Token(Tokens.TokenType.StringLiteral, lexeme));
                continue;
            }
            
            // 3. Character Literals (Everything inside ' ')
            if (current == '\'')
            {
                int start = i;
                i++; // Move past opening single quote
    
                // Read until the closing single quote or end of file
                while (i < source.Length && source[i] != '\'') 
                {
                    // Handle escape characters like '\'' so it doesn't break early
                    if (source[i] == '\\' && i + 1 < source.Length) i++; 
                    i++;
                }
                i++; // Move past closing single quote
    
                string lexeme = source.Substring(start, i - start);
                tokens.Add(new Tokens.Token(Tokens.TokenType.CharLiteral, lexeme));
                continue;
            }

            // 4. Handle Operators and Punctuation (Checking 2-character symbols first)
            if (i + 1 < source.Length)
            {
                string twoCharOp = source.Substring(i, 2);
                if (OperatorsAndPunctuation.ContainsKey(twoCharOp))
                {
                    tokens.Add(new Tokens.Token(OperatorsAndPunctuation[twoCharOp], twoCharOp));
                    i += 2;
                    continue;
                }
            }

            if (OperatorsAndPunctuation.ContainsKey(current.ToString()))
            {
                tokens.Add(new Tokens.Token(OperatorsAndPunctuation[current.ToString()], current.ToString()));
                i++;
                continue;
            }

            // 5. Handle Words (Keywords & Identifiers) and Numbers
            if (char.IsLetterOrDigit(current) || current == '_')
            {
                int start = i;
                // Keep reading as long as it's a valid variable/number character
                while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_' || source[i] == '.'))
                {
                    i++;
                }

                string lexeme = source.Substring(start, i - start);
                Tokens.TokenType type = ResolveLiteralOrIdentifier(lexeme);
                tokens.Add(new Tokens.Token(type, lexeme));
                continue;
            }

            // 6. Catch-all for unexpected characters
            tokens.Add(new Tokens.Token(Tokens.TokenType.Invalid, current.ToString()));
            i++;
        }

        // Always append EndOfFile so the Parser knows it hit the end safely
        tokens.Add(new Tokens.Token(Tokens.TokenType.EndOfFile, ""));
        return tokens;
    }
}