using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("edit", HelpText = "Edit an existing frame")]
public class EditOptions
{
    [Value(0, MetaName = "project", Required = true, HelpText = "Project name")]
    public string Project { get; set; } = null!;

    [Value(1, MetaName = "tags", HelpText = "Optional tags", Required = false)]
    public IEnumerable<string> Tags { get; set; } = [];

    [Option('i', "id", HelpText = "Frame ID")]
    public string? FrameId { get; set; }

    [Option('f', "from", HelpText = "From time")]
    public string? FromTime { get; set; }
}