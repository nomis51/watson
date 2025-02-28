using System.ComponentModel;
using Watson.Core.Models.Abstractions;

namespace Watson.Core.Models;

[Description("Frames")]
public class Frame : DbModel
{
    public long Timestamp { get; set; } = DateTimeOffset.Now.ToUnixTimeSeconds();
    public string ProjectId { get; set; } = null!;
    public Project? Project { get; set; } = null!;
    public List<Tag> Tags { get; set; } = [];

    public DateTimeOffset TimestampAsDateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp);

    public static Frame CreateEmpty(long timestamp)
    {
        return new Frame
        {
            ProjectId = string.Empty,
            Timestamp = timestamp
        };
    }
}