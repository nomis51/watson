using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("add", HelpText = "Add a new frame")]
public class AddOptions
{
    [Value(0, MetaName = "project", Required = true, HelpText = "Project name")]
    public string Project { get; set; } = null!;

    [Option('f', "from", HelpText = "From time")]
    public string? FromTime { get; set; }

    [Option('t', "to", HelpText = "To time")]
    public string? ToTime { get; set; }

    [Value(1, MetaName = "tags", HelpText = "Optional tags", Required = false)]
    public IEnumerable<string> Tags { get; set; } = [];
}