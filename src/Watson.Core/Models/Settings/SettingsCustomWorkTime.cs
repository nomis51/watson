using System.Text.Json.Serialization;

namespace Watson.Core.Models.Settings;

public class SettingsCustomWorkTime
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("workTime")]
    public SettingsWorkTime WorkTime { get; set; } = null!;
}