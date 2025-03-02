using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("create", HelpText = "Create a new resource")]
public class CreateOptions
{
    [Value(0, MetaName = "resource", Required = true, HelpText = "Resource name")]
    public string Resource { get; set; } = null!;

    [Value(1, MetaName = "name", Required = true, HelpText = "Name")]
    public string Name { get; set; } = null!;
}