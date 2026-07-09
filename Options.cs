using CommandLine;

namespace Veneer;

public class Options
{
    [Option('b', "build", Required = true, HelpText = "Build Location For Outputted Binaries and DLLs")]
    public string BuildDirectory { get; set; }
    
    [Option('s', "source", Required = true, HelpText = "Source Directory For Veneer Source Code")]
    public string SourceDirectory { get; set; }
}