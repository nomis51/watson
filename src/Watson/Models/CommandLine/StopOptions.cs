using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Watson.Models.CommandLine;

[ExcludeFromCodeCoverage]
[Verb("stop", HelpText = "Stop the currently running frame")]
public class StopOptions
{
    [Option('a', "at", HelpText = "At time")]
    public string? AtTime { get; set; }
}