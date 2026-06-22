namespace Veneer;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(Lexer.ResolveLiteralOrIdentifier(Console.ReadLine()));
    }
}