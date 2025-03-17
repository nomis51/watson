using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("log", HelpText = "Display frames within given timespan")]
public class LogOptions
{
    [Option('f', "from", HelpText = "From time")]
    public string FromTime { get; set; } = string.Empty;

    [Option('t', "to", HelpText = "To time")]
    public string ToTime { get; set; } = string.Empty;

    [Option('r', "reverse", HelpText = "Display in reverse order")]
    public bool Reverse { get; set; }

    [Option('q', "year", HelpText = "Display frames of the current year")]
    public bool Year { get; set; }

    [Option('m', "month", HelpText = "Display frames of the current month")]
    public bool Month { get; set; }

    [Option('w', "week", HelpText = "Display frames of the current week")]
    public bool Week { get; set; }

    [Option('l', "last-week", HelpText = "Display frames of last week")]
    public bool LastWeek { get; set; }

    [Option('d', "day", HelpText = "Display frames of the current day")]
    public int Day { get; set; }

    [Option('y', "yesterday", HelpText = "Display frame of yesterday")]
    public bool Yesterday { get; set; }

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