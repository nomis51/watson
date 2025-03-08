using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("workhours", HelpText = "Set custom work hours for the day")]
public class WorkHoursOptions
{
    [Value(0, MetaName = "type", Required = true, HelpText = "'start' or 'end'")]
    public string Type { get; set; } = "end";

    [Value(1, MetaName = "time", Required = true, HelpText = "The time. Input 'reset' to reset to default value")]
    public string Time { get; set; } = "reset";
}