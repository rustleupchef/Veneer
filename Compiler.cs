using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Veneer;

public static class Compiler
{
    private static string GetAbsoluteMsBuildIncludeString(string absoluteTargetFolder)
    {
        // 1. Get the full, clean absolute path (resolves any redundant slashes or relative dots)
        string cleanedPath = Path.GetFullPath(absoluteTargetFolder);

        // 2. Convert all Windows backslashes (\) to universal forward slashes (/) 
        // This ensures MSBuild doesn't break if the path is used on Linux/macOS
        cleanedPath = cleanedPath.Replace('\\', '/');

        // 3. Append the MSBuild recursive wildcard pattern safely
        string separator = cleanedPath.EndsWith('/') ? "" : "/";
        
        return $"{cleanedPath}{separator}**/*";
    }
    
    public static string? CompileFolder(
        string sourceFolder,
        string buildDirectory,
        string dllDirectory,
        string? projectName = null,
        bool selfContained = true,
        bool singleFile = true,
        string? runtimeIdentifier = null,
        bool justInTime = false,
        string? config = null,
        string[]? libraries = null,
        bool implementPython = true,
        bool implementJavaScript = true)
    {

        if (!Directory.Exists(dllDirectory))
        {
            Console.WriteLine($"Dll directory doesn't exist: {dllDirectory}");
            return null;
        }

        if (!Directory.Exists(sourceFolder))
        {
            Console.WriteLine($"Source folder not found: {sourceFolder}");
            return null;
        }

        string[] csFiles = Directory.GetFiles(sourceFolder, "*.cs", SearchOption.AllDirectories);
        if (csFiles.Length == 0)
        {
            Console.WriteLine($"No .cs files found under: {sourceFolder}");
            return null;
        }

        runtimeIdentifier ??= RuntimeInformation.RuntimeIdentifier;

        string tempRoot = Path.Combine(Path.GetTempPath(), "veneer-build-" + Guid.NewGuid());
        Directory.CreateDirectory(tempRoot);

        try
        {
            // Copy the source files into the throwaway project, preserving folder structure
            foreach (string file in csFiles)
            {
                string relative = Path.GetRelativePath(sourceFolder, file);
                string destination = Path.Combine(tempRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(file, destination);
            }
            // Add Python manager in the temp directory
            if (implementPython || justInTime)
            {
                string pythonManger = new StreamReader(
                    Assembly.
                        GetExecutingAssembly().
                        GetManifestResourceStream("Veneer.PythonManager.cs") ?? throw new InvalidOperationException()
                    ).ReadToEnd();
                File.WriteAllText(Path.Join(tempRoot, "PythonManager.cs"), pythonManger);
            }
            
            // Add Javascript manager in the temp directory
            if (implementJavaScript || justInTime)
            {
                string javascriptManager = new StreamReader(
                    Assembly.
                        GetExecutingAssembly().
                        GetManifestResourceStream("Veneer.JavascriptManager.cs") ?? throw new InvalidOperationException()
                    ).ReadToEnd();
                File.WriteAllText(Path.Join(tempRoot, "JavascriptManager.cs"), javascriptManager);
            }

            if (justInTime)
            {
                void InsertInternalScript(string[] names)
                {
                    foreach (string name in names)
                    {
                        string code = new StreamReader(
                                Assembly.
                                    GetExecutingAssembly().
                                    GetManifestResourceStream($"Veneer.{name}.cs") ?? throw new InvalidOperationException()
                            ).ReadToEnd();
                        
                        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);
                        SyntaxNode root = tree.GetRoot();

                        NameSpaceRewriter rewriter = new NameSpaceRewriter("Veneer", "VeneerRuntime");
                        SyntaxNode newRoot = rewriter.Visit(root);

                        code = newRoot.ToFullString();
                        File.WriteAllText(Path.Join(tempRoot, $"{name}.cs"), code);
                    }
                }

                InsertInternalScript(["Transpiler", "LanguageConfig", "Tokens", "Lexer", "NameSpaceRewriter", "Compiler"]);
            }
            
            // Target whatever TFM this program itself is running on, so this doesn't
            // need to be bumped by hand every time you move to a new SDK.
            string tfm = $"net{Environment.Version.Major}.0";

            // A self-contained single file still leaves the native runtime libs (coreclr.dll etc.)
            // sitting next to it unless this is set too.
            string extraProps = selfContained && singleFile
                ? "\n    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>"
                : "";

            projectName ??= "main";
            string pythonNet = implementPython || justInTime
                ? "<PackageReference Include=\"pythonnet\" Version=\"3.1.0\"/>" 
                : "";
            string codeAnalysis = justInTime
                ? "<PackageReference Include=\"Microsoft.CodeAnalysis.CSharp\" Version=\"5.3.0\" />"
                : "";

            string embeddedConfig = !string.IsNullOrEmpty(config) 
                ? $"<EmbeddedResource Include=\"{config}\" LogicalName=\"Config\" />" 
                : "";
            
            string justInTimeConstant = justInTime
                ? "JUST_IN_TIME"
                : "";
            string configConstant = justInTime && !string.IsNullOrEmpty(config)
                ? "CONFIG"
                : $"";
            string compilerConstants = 
                $"<DefineConstants>$(DefineConstants);" +
                $"{string.Join(';', justInTimeConstant, configConstant)}" +
                $"</DefineConstants>";
            
            libraries ??= [];
            string csprojPath = Path.Combine(tempRoot, projectName + ".csproj");
            File.WriteAllText(csprojPath, $@"
                <Project Sdk=""Microsoft.NET.Sdk"">
                    <PropertyGroup>
                        <OutputType>Exe</OutputType>
                        <TargetFramework>{tfm}</TargetFramework>
                        <AssemblyName>{projectName}</AssemblyName>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <InvariantGlobalization>true</InvariantGlobalization>{extraProps}
                        {compilerConstants}
                    </PropertyGroup>
                    <ItemGroup>
                        {pythonNet}
                        {codeAnalysis}
                        {string.Join("\n\t\t\t", libraries)}
                    </ItemGroup>
                    <ItemGroup>
                      <Content Include=""{GetAbsoluteMsBuildIncludeString(dllDirectory)}"">
                        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
                        <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
                        
                        <Link>%(RecursiveDir)%(Filename)%(Extension)</Link>
                      </Content>
                      {embeddedConfig}
                    </ItemGroup>
                </Project>
            ");

            Directory.CreateDirectory(dllDirectory);

            ProcessStartInfo psi = new("dotnet")
            {
                WorkingDirectory = tempRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            psi.ArgumentList.Add("publish");
            psi.ArgumentList.Add(csprojPath);
            psi.ArgumentList.Add("-c");
            psi.ArgumentList.Add("Release");
            psi.ArgumentList.Add("-r");
            psi.ArgumentList.Add(runtimeIdentifier);
            psi.ArgumentList.Add("--self-contained");
            psi.ArgumentList.Add(selfContained ? "true" : "false");
            psi.ArgumentList.Add($"-p:PublishSingleFile={(singleFile ? "true" : "false")}");
            psi.ArgumentList.Add("-o");
            psi.ArgumentList.Add(buildDirectory);

            Process process;
            try
            {
                process = Process.Start(psi)!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not start 'dotnet': {ex.Message}");
                Console.WriteLine("Make sure the .NET SDK (not just the runtime) is installed and on PATH.");
                return null;
            }

            // Drain both streams concurrently - blocking on ReadToEnd() one at a time can
            // deadlock if a noisy build fills both pipe buffers at once.
            Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stderrTask = process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("Publish failed:");
                Console.WriteLine(stdoutTask.Result);
                Console.WriteLine(stderrTask.Result);
                return null;
            }

            string exeName = projectName + (OperatingSystem.IsWindows() ? ".exe" : "");
            string exePath = Path.Combine(buildDirectory, exeName);
            return File.Exists(exePath) ? exePath : null;
        }
        finally
        {
            try { Directory.Delete(tempRoot, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }
}