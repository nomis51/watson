using CommandLine;

namespace Watson.Models.CommandLine;

[Verb("todo", HelpText = "Manage your todo list")]
public class TodoOptions
{
    [Value(0, MetaName = "action", Required = true, HelpText = "list, add, done, remove, edit")]
    public string Action { get; set; } = null!;

    [Value(1, MetaName = "arguments", Required = false, HelpText = "Arguments")]
    public IEnumerable<string> Arguments { get; set; } = [];

    [Option('d', "due", HelpText = "Due time")]
    public string? DueTime { get; set; }

    [Option('p', "priority", HelpText = "Priority")]
    public int? Priority { get; set; }
}