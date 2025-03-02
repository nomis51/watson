using Watson.Core.Models.Settings;

namespace Watson.Core.Repositories.Abstractions;

public interface ISettingsRepository
{
    Task<Settings> GetSettings();
    Task SaveSettings(Settings settings);
}