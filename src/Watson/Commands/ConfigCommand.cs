using System.Reflection;
using System.Text.Json.Serialization;
using Spectre.Console;
using Watson.Commands.Abstractions;
using Watson.Core.Extensions;
using Watson.Core.Models.Settings;
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
        return 1;
    }

    private async Task<int> GetConfig(string key)
    {
        var settings = await SettingsRepository.GetSettings();
        if (!settings.GetJsonPathValue(key, out var value)) return 1;

        AnsiConsole.MarkupLine("{0}: {1}", key, value ?? "[grey](null)[/]");
        return 0;
    }

    #endregion
}