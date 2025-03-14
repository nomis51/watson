using Spectre.Console;
using Watson.Commands.Abstractions;
using Watson.Core.Extensions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class ConfigCommand : Command<ConfigOptions>
{
    #region Constructors

    public ConfigCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(ConfigOptions options)
    {
        if (string.IsNullOrEmpty(options.Key)) return 1;
        if (options.Action.Equals("get", StringComparison.OrdinalIgnoreCase))
        {
            return await GetConfig(options.Key);
        }

        if (options.Action.Equals("set", StringComparison.OrdinalIgnoreCase))
        {
            return await SetConfig(options.Key, options.Value);
        }

        return 1;
    }

    #endregion

    #region Private methods

    private async Task<int> SetConfig(string key, string? value)
    {
        var settings = await SettingsRepository.GetSettings();
        if (!settings.SetJsonPathValue(key, ConvertToValueType(value, key))) return 1;

        await SettingsRepository.SaveSettings(settings);
        return 0;
    }

    private async Task<int> GetConfig(string key)
    {
        var settings = await SettingsRepository.GetSettings();
        if (!settings.GetJsonPathValue(key, out var value)) return 1;

        Console.MarkupLine("{0}: {1}", key, value ?? "[grey](null)[/]");
        return 0;
    }

    private static object? ConvertToValueType(string? input, string key)
    {
        if (string.IsNullOrEmpty(input)) return null;

        return key switch
        {
            "workTime.startTime" => TimeSpan.Parse(input),
            "workTime.endTime" => TimeSpan.Parse(input),
            "workTime.lunchStartTime" => TimeSpan.Parse(input),
            "workTime.lunchEndTime" => TimeSpan.Parse(input),
            "workTime.weekStartDay" => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), input, true),
            _ => null
        };
    }

    #endregion
}