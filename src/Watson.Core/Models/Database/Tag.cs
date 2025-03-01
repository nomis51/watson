using System.ComponentModel;
using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Models.Database;

[Description("Tags")]
public class Tag : DbModel
{
    public string Name { get; set; } = null!;
}