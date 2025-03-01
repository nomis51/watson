using System.ComponentModel;
using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Models.Database;

[Description("Projects")]
public class Project : DbModel
{
    public string Name { get; set; } = null!;
}