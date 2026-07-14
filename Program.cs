using CommandLine;

namespace Veneer;

internal abstract class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(RunOptions)
            .WithNotParsed(HandleParserError);
    }

    static void RunOptions(Options opts)
    {
        if (Directory.Exists(opts.BuildDirectory))
            Directory.Delete(opts.BuildDirectory, true);
        Directory.CreateDirectory(opts.BuildDirectory);
        
        string[] files = Directory.GetFiles(opts.SourceDirectory, "*.v");
        string tempSourceDir = Path.Combine(Path.GetTempPath(), "veneer-code-" + Guid.NewGuid());
        string tempDllBuildDir = Path.Combine(Path.GetTempPath(), "veneer-build-foreign-code-" + Guid.NewGuid());
        Directory.CreateDirectory(tempSourceDir);
        Directory.CreateDirectory(tempDllBuildDir);

        foreach (string file in files)
        {
            List<Tokens.Token> tokens = Lexer.LexText(File.ReadAllText(file));
            Transpiler transpiler = new Transpiler(tokens, tempDllBuildDir);
            string result = transpiler.Transpile();
            string name = Path.GetFileNameWithoutExtension(file);
            File.WriteAllText(Path.Combine(tempSourceDir, $"{name}.cs"), result);
        }

        string? executablePath = Compiler.CompileFolder(
            sourceFolder: tempSourceDir, 
            buildDirectory: Path.GetFullPath(opts.BuildDirectory), 
            dllDirectory: Path.GetFullPath(tempDllBuildDir),
            projectName: opts.ExecutableName);
        
        Directory.Delete(tempSourceDir, true);
        Directory.Delete(tempDllBuildDir, true);
        
        Console.WriteLine($"Executable path: {executablePath}");
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