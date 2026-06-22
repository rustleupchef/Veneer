namespace Veneer;

public class Tokens
{
    public enum TokenType 
    {
        // Special
        EndOfFile, Invalid,

        // Identifiers & Literals
        Identifier, IntLiteral, FloatLiteral, CharLiteral, StringLiteral, BoolLiteral,

        // Keywords
        For, While, If, Else, Return,
        Int, Float, Bool, Char, Void, String,
        Tooth, Language,

        // Operators
        Plus, Minus, Star, Slash, Modulo,
        Assign, EqualEqual, NotEqual,
        LessThan, LessEqual, GreaterThan, GreaterEqual,
        AmpAmp, PipePipe, Bang,

        // Punctuation
        LeftParen, RightParen, LeftBrace, RightBrace, LeftBracket, RightBracket,
        Comma, SemiColon
    }
    
    public record Token(TokenType Type, string Value);
}