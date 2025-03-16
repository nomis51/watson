using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("alias", HelpText = "Create or remove a command alias")]
public class AliasOptions
{
    [Value(0, MetaName = "arguments", HelpText = "Command alias", Required = true)]
    public IEnumerable<string> Arguments { get; set; } = [];
}