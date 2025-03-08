using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("stats", HelpText = "Display statistics")]
public class StatsOptions
{
    [Value(0, MetaName = "type", Required = true, HelpText = "Type of statistics")]
    public string Type { get; set; } = null!;

    [Option('f', "from", HelpText = "From time")]
    public string FromTime { get; set; } = string.Empty;

    [Option('t', "to", HelpText = "To time")]
    public string ToTime { get; set; } = string.Empty;

    [Option('y', "year", HelpText = "Display frames of the current year")]
    public bool Year { get; set; }

    [Option('m', "month", HelpText = "Display frames of the current month")]
    public bool Month { get; set; }

    [Option('w', "week", HelpText = "Display frames of the current week")]
    public bool Week { get; set; }

    [Option('d', "day", HelpText = "Display frames of the current day")]
    public bool Day { get; set; } = true;

    [Option('a', "all", HelpText = "Display all frames")]
    public bool All { get; set; }

    [Option('p', "project", HelpText = "Display frames of the project(s) only")]
    public string? Projects { get; set; }

    [Option('t', "tag", HelpText = "Display frames of the tag(s) only")]
    public string? Tags { get; set; }

    [Option('i', "ignore-project", HelpText = "Ignore frames of the project(s)")]
    public string? IgnoredProjects { get; set; }

    [Option('g', "ignore-tag", HelpText = "Ignore frames of the tag(s)")]
    public string? IgnoredTags { get; set; }
}