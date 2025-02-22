using System.ComponentModel;
using Watson.Core.Models.Abstractions;

namespace Watson.Core.Models;

[Description("Projects")]
public class Project : DbModel
{
    public string Name { get; set; } = null!;
}