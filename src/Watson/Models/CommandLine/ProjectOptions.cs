using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("project", HelpText = "Manage projects")]
public class ProjectOptions
{
    [Value(0, MetaName = "action", Required = true, HelpText = "Action to perform")]
    public string Action { get; set; } = null!;

    [Value(1, MetaName = "arguments", Required = false, HelpText = "Arguments")]
    public IEnumerable<string> Arguments { get; set; } = [];
}