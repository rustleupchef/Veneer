using CommandLine;

namespace Veneer;

internal class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParserError);
    }

    static void RunOptions(Options opts)
    {
        string[] files = Directory.GetFiles(opts.SourceDirectory, "*.v");
        string tempDir = Path.Combine(Path.GetTempPath(), "veneer-code-" + Guid.NewGuid());
        Directory.CreateDirectory(tempDir);

        foreach (string file in files)
        {
            List<Tokens.Token> tokens = Lexer.LexText(File.ReadAllText(file));
            Transpiler transpiler = new Transpiler(tokens, opts.BuildDirectory);
            string result = transpiler.Transpile();
            string name = Path.GetFileNameWithoutExtension(file);
            File.WriteAllText(Path.Combine(tempDir, $"{name}.cs"), result);
        }

        Compiler.CompileFolder(sourceFolder: tempDir, outputDirectory: opts.BuildDirectory);
        
        Directory.Delete(tempDir, true);
    }

    static void HandleParserError(IEnumerable<Error> errs)
    {
        Console.WriteLine("Parser Error");
        foreach (var err in errs)
        {
            Console.WriteLine(err.Tag);
        }
        Environment.Exit(1);
    }
}