using System.Text.Json.Serialization;

namespace Watson.Core.Models.Settings;

public class Settings
{
    [JsonPropertyName("workTime")]
    public SettingsWorkTime WorkTime { get; set; } = new();

    [JsonPropertyName("customWorkTimes")]
    public List<SettingsCustomWorkTime> CustomWorkTimes { get; set; } = [];
}