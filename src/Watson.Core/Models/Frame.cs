using System.ComponentModel;
using Watson.Core.Models.Abstractions;

namespace Watson.Core.Models;

[Description("Frames")]
public class Frame : DbModel
{
    public long Time { get; set; } = DateTime.Now.Ticks;
    public string ProjectId { get; set; } = null!;
    public Project? Project { get; set; }
    public List<Tag> Tags { get; set; } = [];

    public DateTime TimeAsDateTime => new(Time);

    public static Frame CreateEmpty(long timestamp)
    {
        return new Frame
        {
            ProjectId = string.Empty,
            Time = timestamp
        };
    }
}