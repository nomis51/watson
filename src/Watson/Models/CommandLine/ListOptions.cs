using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("list", HelpText = "List projects or tags")]
public class ListOptions
{
    [Value(0, MetaName = "resource", Required = true, HelpText = "project or tag")]
    public string Resource { get; set; } = null!;
}
