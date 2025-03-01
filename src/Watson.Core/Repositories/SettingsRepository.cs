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
    }

    #endregion

    #region Public methods

    public async Task<Settings> GetSettings()
    {
        try
        {
            if (!_fileSystem.File.Exists(SettingsFilePath)) return new Settings();

            var json = await _fileSystem.File.ReadAllTextAsync(SettingsFilePath);
            return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read settings file");
        }

        return new Settings();
    }

    public async Task SaveSettings(Settings settings)
    {
        try
        {
            var json = JsonSerializer.Serialize(settings);
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
        if (_fileSystem.Directory.Exists(SettingsFilePath)) return;

        _fileSystem.Directory.CreateDirectory(SettingsFilePath);
    }

    #endregion
}