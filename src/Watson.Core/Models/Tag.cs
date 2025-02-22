using System.ComponentModel;
using Watson.Core.Models.Abstractions;

namespace Watson.Core.Models;

[Description("Tags")]
public class Tag : DbModel
{
    public string Name { get; set; } = null!;
}