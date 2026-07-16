namespace VeneerRuntime;

using System;
using System.Text.RegularExpressions;
using Python.Runtime;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

public sealed class PythonManager : IDisposable
{
    private static readonly Lazy<PythonManager> _instance = new Lazy<PythonManager>(() => new PythonManager());
    private bool _isDisposed = false;
    private IntPtr _mainThreadState; // 👈 Tracks state to safely lock/unlock the GIL during lifecycle changes

    public static PythonManager Instance => _instance.Value;

    public static string FindPythonDll()
    {
        // 1. Try resolving via environment PATH (supports virtual envs, global, conda, etc.)
        string[] commands = { "python", "python3" };

        // Cross-platform Python script to discover its own internal runtime library location
        string locatorScript = 
            "import sys, os, sysconfig; " +
            "dll = (sysconfig.get_config_var('DLLLIBRARY') or f'python{sys.version_info.major}{sys.version_info.minor}.dll') if os.name == 'nt' else (sysconfig.get_config_var('LDLIBRARY') or f'libpython{sys.version_info.major}.{sys.version_info.minor}.so'); " +
            "libdir = sys.base_prefix if os.name == 'nt' else (sysconfig.get_config_var('LIBDIR') or os.path.join(sys.base_prefix, 'lib')); " +
            "print(os.path.abspath(os.path.join(libdir, dll)))";

        foreach (var cmd in commands)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = cmd,
                    Arguments = $"-c \"{locatorScript}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd().Trim();
                        process.WaitForExit();

                        if (process.ExitCode == 0 && !string.IsNullOrEmpty(output) && File.Exists(output))
                        {
                            return output;
                        }
                    }
                }
            }
            catch
            {
                // Fall through to try the next command if this execution fails
            }
        }

        // 2. Windows Registry Fallback (if Python is installed but missing from PATH)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                using (var baseKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Python\PythonCore"))
                {
                    if (baseKey != null)
                    {
                        var versions = baseKey.GetSubKeyNames();
                        if (versions.Length > 0)
                        {
                            // Sort to isolate the latest registered version
                            Array.Sort(versions);
                            string latestVersion = versions[versions.Length - 1];

                            using (var installKey = baseKey.OpenSubKey($@"{latestVersion}\InstallPath"))
                            {
                                string exePath = installKey?.GetValue("ExecutablePath")?.ToString();
                                if (!string.IsNullOrEmpty(exePath))
                                {
                                    string dir = Path.GetDirectoryName(exePath);
                                    string versionToken = latestVersion.Replace(".", "");
                                    string dllPath = Path.Combine(dir, $"python{versionToken}.dll");

                                    if (File.Exists(dllPath))
                                    {
                                        return dllPath;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // Suppress registry exceptions for platform safety
            }
        }

        throw new FileNotFoundException("Could not automatically locate the Python shared library. Ensure Python is installed and accessible.");
    }
    
    private PythonManager()
    {
        // Automatically handle engine shutdown when C# application exits
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => Dispose();
        Runtime.PythonDLL = FindPythonDll();
        PythonEngine.Initialize();

        // 👈 CRITICAL: Yields primary thread control back to C#.
        // This stops the main thread from choking Python's shutdown hook later.
        _mainThreadState = PythonEngine.BeginAllowThreads();
    }

    /// <summary>
    /// Compiles raw Python code on the fly, auto-detects the function name, and executes it.
    /// </summary>
    /// <typeparam name="T">Expected C# return type.</typeparam>
    /// <param name="rawPythonCode">The standalone Python function as a raw string.</param>
    /// <param name="args">Parameters to pass into the Python function.</param>
    public T Execute<T>(string rawPythonCode, string functionName, params object[] args)
    {

        // 1. Safely acquire the Global Interpreter Lock (GIL)
        using (Py.GIL())
        {
            try
            {
                // 2. Create an isolated scope/context for this execution
                using (PyModule scope = Py.CreateScope())
                {
                    // Compile and load the raw string into our local scope
                    scope.Exec(rawPythonCode);

                    // Fetch the function object out of the scope
                    using (PyObject pyFunction = scope.GetAttr(functionName))
                    {
                        // Convert C# arguments to Python types
                        PyObject[] pyArgs = new PyObject[args.Length];
                        for (int i = 0; i < args.Length; i++)
                        {
                            pyArgs[i] = args[i].ToPython();
                        }

                        // Run the function
                        using (PyObject pyResult = pyFunction.Invoke(pyArgs))
                        {
                            // Clean up parameter allocations
                            foreach (var arg in pyArgs) arg.Dispose();

                            // Cast back to C# type
                            return pyResult.As<T>();
                        }
                    }
                }
            }
            catch (PythonException ex)
            {
                throw new Exception($"Python Runtime Error: {ex.Message}", ex);
            }
        }
    }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            PythonEngine.Shutdown();
            _isDisposed = true;
        }
    }
}