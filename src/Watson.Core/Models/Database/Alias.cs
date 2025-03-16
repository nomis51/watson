using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Models.Database;

public class Alias : DbModel
{
    public string Name { get; set; } = null!;
    public string Command { get; set; } = null!;
}