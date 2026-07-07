namespace Veneer;

public class Tokens
{
    public enum TokenType 
    {
        // Special
        EndOfFile, Invalid,
        Arrow, ForeignCodeBlock,

        // Identifiers & Literals
        Identifier, IntLiteral, FloatLiteral, CharLiteral, StringLiteral, BoolLiteral,

        // Keywords
        For, While, If, Else, Return,
        Int, Float, Bool, Char, Void, String,
        Function, Class,
        New, This, Base,
        Public, Private, Protected, Internal,
        Static, ReadOnly, Const,
        Async, Virtual, Override, Sealed,
        Tooth, Language,

        // Operators
        Plus, Minus, Star, Slash, Modulo,
        Assign, EqualEqual, NotEqual,
        LessThan, LessEqual, GreaterThan, GreaterEqual,
        AmpAmp, PipePipe, Bang,

        // Punctuation
        LeftParen, RightParen, LeftBrace, RightBrace, LeftBracket, RightBracket,
        Comma, SemiColon, Dot, Colon
    }
    
    public record Token(TokenType Type, string Value);
}