using System.ComponentModel;
using Watson.Core.Models.Abstractions;

namespace Watson.Core.Models;

[Description("Frames")]
public class Frame : DbModel
{
    public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
    public string ProjectId { get; set; } = null!;
    public Project? Project { get; set; } = null!;
    public List<Tag> Tags { get; set; } = [];
}