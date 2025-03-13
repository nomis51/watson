using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("remove", HelpText = "Remove a frame")]
public class RemoveOptions
{
    [Value(1, MetaName = "frameId", Required = true, HelpText = "Frame ID")]
    public string FrameId { get; set; } = null!;
}