using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Watson.Core.Models.Settings;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class SettingsRepository : ISettingsRepository
{
    #region Constants

    private const string SettingsFileName = "settings.json";

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    #endregion

    #region Members

    private readonly IFileSystem _fileSystem;
    private readonly ILogger<SettingsRepository> _logger;

    #endregion

    #region Props

    private static string SettingsFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        $".{nameof(Watson).ToLower()}",
        SettingsFileName
    );

    #endregion

    #region Constructors

    public SettingsRepository(IFileSystem fileSystem, ILogger<SettingsRepository> logger)
    {
        _fileSystem = fileSystem;
        _logger = logger;

        EnsureFoldersExist();
    }

    #endregion

    #region Public methods

    public async Task<Settings> GetSettings()
    {
        try
        {
            if (!_fileSystem.File.Exists(SettingsFilePath))
            {
                var settings = new Settings();
                await SaveSettings(settings);
                return settings;
            }

            var json = await _fileSystem.File.ReadAllTextAsync(SettingsFilePath);

            try
            {
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
            catch (Exception e)
            {
                await SaveSettings(new Settings());
                _logger.LogError(e, "Failed to parse settings file");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read settings file");
        }

        var settings2 = new Settings();
        await SaveSettings(settings2);
        return settings2;
    }

    public async Task SaveSettings(Settings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings, _jsonSerializerOptions);
            await _fileSystem.File.WriteAllTextAsync(SettingsFilePath, json);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save settings file");
        }
    }

    #endregion

    #region Private methods

    private void EnsureFoldersExist()
    {
        var folder = Path.GetDirectoryName(SettingsFilePath)!;
        if (_fileSystem.Directory.Exists(folder)) return;

        _fileSystem.Directory.CreateDirectory(folder);
        _fileSystem.File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(new Settings()));
    }

    #endregion
}