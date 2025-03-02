using System.Text.Json.Serialization;

namespace Watson.Core.Models.Settings;

public class SettingsWorkTime
{
    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; set; } = new(8, 0, 0);

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; set; } = new(16, 0, 0);

    [JsonPropertyName("lunchStartTime")]
    public TimeSpan LunchStartTime { get; set; } = new(12, 0, 0);

    [JsonPropertyName("lunchEndTime")]
    public TimeSpan LunchEndTime { get; set; } = new(13, 0, 0);

    [JsonPropertyName("weekStartDay")]
    public DayOfWeek WeekStartDay { get; set; } = DayOfWeek.Sunday;
}