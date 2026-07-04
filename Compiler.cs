using System.Reflection;
using Microsoft.CodeAnalysis.Emit;

namespace Veneer;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class Compiler
{
    public static void CompileAndRun(string code)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

        var references = new MetadataReference[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(FileSystemInfo).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Jint.Engine).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Acornima")).Location),
            MetadataReference.CreateFromFile(typeof(Python.Runtime.PythonEngine).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("Microsoft.CSharp")).Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("netstandard")).Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Linq.Expressions")).Location),
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            "Veneer",
            new [] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

        using (var ms = new MemoryStream())
        {
            EmitResult result = compilation.Emit(ms);
            if (!result.Success)
            {
                Console.WriteLine("Compilation failed");
                foreach (var diagnostic in result.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }
                return;
            }
            
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            MethodInfo entryPoint = assembly.EntryPoint;
            if (entryPoint != null)
            {
                // Check if the method expects arguments
                object[] parameters = entryPoint.GetParameters().Length == 0 
                    ? null 
                    : new object[] { new string[0] };

                entryPoint.Invoke(null, parameters);
            }
        }
    }
}