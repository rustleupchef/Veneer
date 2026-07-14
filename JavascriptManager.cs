using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace VeneerRuntime;

/// <summary>
/// Runs JavaScript through Node.js from C#.
/// </summary>
public static class JavascriptManager
{
    /// <summary>Path or name of the Node.js executable. Defaults to "node" (resolved via PATH).</summary>
    public static string NodeExecutablePath { get; set; } = "node";

    /// <summary>Maximum time a single Run call is allowed to take before the process is killed.</summary>
    public static TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Event/Action triggered whenever console.log (or stderr) output is received from Node.js.
    /// Defaults to printing directly to the C# Console.
    /// </summary>
    public static Action<string>? OnLog { get; set; } = Console.WriteLine;

    private static readonly string BootstrapScriptPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    static JavascriptManager()
    {
        BootstrapScriptPath = Path.Combine(Path.GetTempPath(), "JavascriptManager_bootstrap.js");
        File.WriteAllText(BootstrapScriptPath, BootstrapScript, Encoding.UTF8);
    }

    /// <summary>
    /// Synchronously runs <paramref name="code"/> and returns the JSON-serialized result.
    /// </summary>
    public static string Run(string code, params object[] args)
        => RunAsync(code, args).GetAwaiter().GetResult();

    /// <summary>
    /// Same as <see cref="Run"/> but deserializes the result directly into <typeparamref name="T"/>.
    /// </summary>
    public static T Run<T>(string code, params object[] args)
    {
        var json = Run(code, args);
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>Async version of <see cref="Run"/>.</summary>
    public static async Task<string> RunAsync(string code, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("code must not be null or empty.", nameof(code));

        var payload = JsonSerializer.Serialize(new NodeRequest
        {
            Code = code,
            Args = args ?? Array.Empty<object>()
        }, JsonOptions);

        var psi = new ProcessStartInfo
        {
            FileName = NodeExecutablePath,
            Arguments = $"\"{BootstrapScriptPath}\"",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.Append(e.Data); };
        
        // FIX: Capture stderr in real-time and pass it to the OnLog handler
        process.ErrorDataReceived += (_, e) => 
        { 
            if (e.Data != null) 
            {
                stderr.AppendLine(e.Data);
                OnLog?.Invoke(e.Data); 
            }
        };

        try
        {
            process.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Could not start Node.js process ('{NodeExecutablePath}'). " +
                "Make sure Node.js is installed and on PATH, or set JavascriptManager.NodeExecutablePath.",
                ex);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.StandardInput.WriteAsync(payload);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        using var cts = new CancellationTokenSource(Timeout);
        try
        {
            await process.WaitForExitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw new TimeoutException(
                $"Node execution exceeded {Timeout.TotalSeconds:0.#}s and was terminated.");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Node process exited with code {process.ExitCode}. Stderr: {stderr}");
        }

        var raw = stdout.ToString().Trim();

        NodeResponse response;
        try
        {
            response = JsonSerializer.Deserialize<NodeResponse>(raw, JsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse Node output: {raw}", ex);
        }

        if (response is null)
            throw new InvalidOperationException("Node process produced no output.");

        if (!response.Success)
            throw new JavaScriptExecutionException(response.Error, response.Stack);

        return response.Result?.GetRawText() ?? "null";
    }

    private static void TryKill(Process process)
    {
        try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
        catch { /* best effort cleanup */ }
    }

    private sealed class NodeRequest
    {
        public string Code { get; set; }
        public object[] Args { get; set; }
    }

    private sealed class NodeResponse
    {
        public bool Success { get; set; }
        public JsonElement? Result { get; set; }
        public string Error { get; set; }
        public string Stack { get; set; }
    }

    private const string BootstrapScript = @"
const vm = require('vm');

let inputData = '';
process.stdin.setEncoding('utf8');
process.stdin.on('data', (chunk) => { inputData += chunk; });
process.stdin.on('end', async () => {
try {
const parsed = JSON.parse(inputData);
const code = parsed.code;
const args = parsed.args || [];

const util = require('util');
const safeConsole = {
  log: (...a) => process.stderr.write(util.format(...a) + '\n'),
  info: (...a) => process.stderr.write(util.format(...a) + '\n'),
  warn: (...a) => process.stderr.write(util.format(...a) + '\n'),
  error: (...a) => process.stderr.write(util.format(...a) + '\n'),
  debug: (...a) => process.stderr.write(util.format(...a) + '\n')
};

const sandbox = {
  console: safeConsole,
  require: require,
  Buffer: Buffer,
  setTimeout: setTimeout,
  clearTimeout: clearTimeout,
  setInterval: setInterval,
  clearInterval: clearInterval
};
const context = vm.createContext(sandbox);

const script = new vm.Script('(' + code + ')');
const fn = script.runInContext(context, { timeout: 5000 });

if (typeof fn !== 'function') {
  throw new Error('Provided code must evaluate to a function.');
}

let result = fn.apply(null, args);
if (result && typeof result.then === 'function') {
  result = await result;
}

process.stdout.write(JSON.stringify({
  success: true,
  result: result === undefined ? null : result
}));
} catch (err) {
process.stdout.write(JSON.stringify({
  success: false,
  error: err && err.message ? err.message : String(err),
  stack: err && err.stack ? err.stack : null
}));
}
});
";
}

public sealed class JavaScriptExecutionException : Exception
{
    public string JavaScriptStack { get; }

    public JavaScriptExecutionException(string message, string javaScriptStack)
        : base(message)
    {
        JavaScriptStack = javaScriptStack;
    }
}