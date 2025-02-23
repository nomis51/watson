using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("rename", HelpText = "Rename a project or tag")]
public class RenameOptions
{
    [Value(0, MetaName = "resource", Required = true, HelpText = "project or tag")]
    public string Resource { get; set; } = null!;

    [Value(1, MetaName = "resourceId", Required = true, HelpText = "Resource ID")]
    public string ResourceId { get; set; } = null!;

    [Value(2, MetaName = "name", Required = true, HelpText = "New name")]
    public string Name { get; set; } = null!;
}