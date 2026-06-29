namespace Veneer;

internal class Program
{
    static void Main(string[] args)
    {
        List<Tokens.Token> tokens = Lexer.LexText(File.ReadAllText(Console.ReadLine()));

        try
        {
            Transpiler transpiler = new Transpiler(tokens, Console.ReadLine());
            string result = transpiler.Transpile();
            File.WriteAllText(Console.ReadLine(), result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }
}