using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("start", HelpText = "Add a frame at the current time")]
public class StartOptions
{
    [Value(0, MetaName = "project", Required = true, HelpText = "Project name")]
    public string Project { get; set; } = null!;

    [Value(1, MetaName = "tags", HelpText = "Optional tags", Required = false)]
    public IEnumerable<string> Tags { get; set; } = [];

    private string? _fromTime = null;

    [Option('f', "from", HelpText = "From time")]
    public string? FromTime
    {
        get => _fromTime;
        set => _fromTime = value;
    }

    [Option('a', "at", HelpText = "At time (alias for --from)")]
    public string? AtTime
    {
        get => _fromTime;
        set => _fromTime = value;
    }
}