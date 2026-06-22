namespace Veneer;

internal class Program
{
    static void Main(string[] args)
    {
        List<Tokens.Token> tokens = Lexer.LexText(File.ReadAllText(Console.ReadLine()));

        foreach (Tokens.Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }
}