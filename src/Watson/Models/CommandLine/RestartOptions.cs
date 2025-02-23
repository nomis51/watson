using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("restart", HelpText = "Restart a frame")]
public class RestartOptions
{
    [Value(0, MetaName = "id", Required = false, HelpText = "Frame ID")]
    public string? FrameId { get; set; }
}