using CommandLine;

namespace Watson.Models.CommandLine;
[Verb("remove", HelpText = "Remove a project, tag or frame")]
public class RemoveOptions
{
    [Value(0, MetaName = "resource", Required = true, HelpText = "project, tag or frame")]
    public string Resource { get; set; } = null!;

    [Value(1, MetaName = "resourceId", Required = true, HelpText = "Resource ID")]
    public string ResourceId { get; set; } = null!;
}
