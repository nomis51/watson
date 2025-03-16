using System.ComponentModel;
using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Models.Database;

[Description("Aliases")]
public class Alias : DbModel
{
    public string Name { get; set; } = null!;
    public string Command { get; set; } = null!;

    public string[] Arguments =>
        Command.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}