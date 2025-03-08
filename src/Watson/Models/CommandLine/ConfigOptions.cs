using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("config", HelpText = "Get or set a setting key")]
public class ConfigOptions
{
    [Value(0, MetaName = "action", Required = true, HelpText = "get or set")]
    public string Action { get; set; } = "get";

    [Value(1, MetaName = "key", Required = true, HelpText = "Setting's key name")]
    public string Key { get; set; } = null!;

    [Value(2, MetaName = "value", Required = false, HelpText = "Setting's value")]
    public string? Value { get; set; } = null!;
}