using System.Text.Json.Serialization;

namespace Watson.Core.Models.Settings;

public class Settings
{
    [JsonPropertyName("workTime")]
    public SettingsWorkTime WorkTime { get; set; } = new();

    [JsonPropertyName("customWorkTimes")]
    public List<SettingsCustomWorkTime> CustomWorkTimes { get; set; } = [];

    public SettingsWorkTime GetTodaysWorkTime()
    {
        var customWorkTime = CustomWorkTimes.FirstOrDefault(e => e.Date.Date == DateTime.Now.Date);
        return customWorkTime?.WorkTime ?? WorkTime;
    }
}