using System.ComponentModel;
using Watson.Core.Models.Database.Abstractions;

namespace Watson.Core.Models.Database;

[Description("Todos")]
public class Todo : DbModel
{
    public string Description { get; set; } = null!;
    public string ProjectId { get; set; } = null!;
    public long? DueTime { get; set; }
    public int? Priority { get; set; }
    public bool IsCompleted { get; set; }

    public Project? Project { get; set; }
    public List<Tag> Tags { get; set; } = [];

    public DateTime? DueTimeAsDateTime => DueTime is null ? null : new DateTime(DueTime.Value);
}