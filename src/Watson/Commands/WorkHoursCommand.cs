using Watson.Commands.Abstractions;
using Watson.Core.Models.Settings;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class WorkHoursCommand : Command<WorkHoursOptions>
{
    #region Constructors

    public WorkHoursCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(WorkHoursOptions options)
    {
        var settings = await SettingsRepository.GetSettings();
        var index = settings.CustomWorkTimes.FindIndex(e => e.Date.Date == DateTime.Today);

        if (options.Type.Equals("reset", StringComparison.OrdinalIgnoreCase) && index != -1)
        {
            settings.CustomWorkTimes.RemoveAt(index);
            await SettingsRepository.SaveSettings(settings);
            return 0;
        }

        if (options.Type.Equals("start", StringComparison.OrdinalIgnoreCase))
        {
            var time = TimeHelper.ParseTime(options.Time);
            if (time is null) return 1;

            if (index == -1)
            {
                settings.CustomWorkTimes.Add(new SettingsCustomWorkTime
                {
                    Date = DateTime.Today,
                    WorkTime = new SettingsWorkTime
                    {
                        StartTime = time.Value
                    }
                });
            }
            else
            {
                settings.CustomWorkTimes[index].WorkTime.StartTime = time.Value;
            }

            await SettingsRepository.SaveSettings(settings);
            return 0;
        }

        if (options.Type.Equals("end", StringComparison.OrdinalIgnoreCase))
        {
            var time = TimeHelper.ParseTime(options.Time);
            if (time is null) return 1;

            if (index == -1)
            {
                settings.CustomWorkTimes.Add(new SettingsCustomWorkTime
                {
                    Date = DateTime.Today,
                    WorkTime = new SettingsWorkTime
                    {
                        EndTime = time.Value
                    }
                });
            }
            else
            {
                settings.CustomWorkTimes[index].WorkTime.EndTime = time.Value;
            }

            await SettingsRepository.SaveSettings(settings);
            return 0;
        }

        return 1;
    }

    #endregion
}