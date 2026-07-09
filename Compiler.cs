using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Veneer;

public static class Compiler
{
    public static string? CompileFolder(
        string sourceFolder,
        string outputDirectory,
        string? projectName = "main",
        bool selfContained = true,
        bool singleFile = true,
        string? runtimeIdentifier = null)
    {
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
            string pythonManger = new StreamReader(
                Assembly.
                    GetExecutingAssembly().
                    GetManifestResourceStream("Veneer.PythonManager.cs")
                ).ReadToEnd();
            File.WriteAllText(Path.Join(tempRoot, "PythonManager.cs"), pythonManger);

            // Target whatever TFM this program itself is running on, so this doesn't
            // need to be bumped by hand every time you move to a new SDK.
            string tfm = $"net{Environment.Version.Major}.0";

            // A self-contained single file still leaves the native runtime libs (coreclr.dll etc.)
            // sitting next to it unless this is set too.
            string extraProps = selfContained && singleFile
                ? "\n    <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>"
                : "";

            string csprojPath = Path.Combine(tempRoot, projectName + ".csproj");
            File.WriteAllText(csprojPath, $@"
                <Project Sdk=""Microsoft.NET.Sdk"">
                    <PropertyGroup>
                        <OutputType>Exe</OutputType>
                        <TargetFramework>{tfm}</TargetFramework>
                        <AssemblyName>{projectName}</AssemblyName>
                        <ImplicitUsings>enable</ImplicitUsings>
                        <InvariantGlobalization>true</InvariantGlobalization>{extraProps}
                    </PropertyGroup>
                    <ItemGroup>
                        <PackageReference Include=""Jint"" Version=""4.10.1""/>
                        <PackageReference Include=""pythonnet"" Version=""3.1.0""/>
                    </ItemGroup>
                </Project>
            ");

            Directory.CreateDirectory(outputDirectory);

            var psi = new ProcessStartInfo("dotnet")
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
            psi.ArgumentList.Add(outputDirectory);

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
            string exePath = Path.Combine(outputDirectory, exeName);
            return File.Exists(exePath) ? exePath : null;
        }
        finally
        {
            try { Directory.Delete(tempRoot, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }
}