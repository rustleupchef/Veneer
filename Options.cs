using CommandLine;

namespace Veneer;

public class Options
{
    [Option('b', "build", Required = true, HelpText = "Build Location For Outputted Binaries and DLLs")]
    public string BuildDirectory { get; set; }
    
    [Option('s', "source", Required = true, HelpText = "Source Directory For Veneer Source Code")]
    public string SourceDirectory { get; set; }
    
    [Option('n', "name", Required = false, HelpText = "Name of the Veneer Executable")]
    public string ExecutableName { get; set; }
    
    [Option('c', "config", Required = false, HelpText = "Configuration Json File for Veneer Build")]
    public string ConfigFile { get; set; }
}