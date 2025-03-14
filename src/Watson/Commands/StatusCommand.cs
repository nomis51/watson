using Spectre.Console;
using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class StatusCommand : Command<StatusOptions>
{
    #region Constructors

    public StatusCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(StatusOptions options)
    {
        var frame = await FrameRepository.GetPreviousFrameAsync(DateTime.Now);
        if (frame is null) return 1;

        var settings = await SettingsRepository.GetSettings();
        var endTime = DateTime.Now;

        if (DateTime.Now.TimeOfDay >= settings.WorkTime.LunchStartTime &&
            DateTime.Now.TimeOfDay <= settings.WorkTime.LunchEndTime)
        {
            endTime = new DateTime(
                DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                settings.WorkTime.LunchStartTime.Hours,
                settings.WorkTime.LunchStartTime.Minutes,
                settings.WorkTime.LunchStartTime.Seconds
            );
        }

        var needToSubtractLunchTime = frame.TimeAsDateTime.TimeOfDay < settings.WorkTime.LunchStartTime &&
                                      DateTime.Now.TimeOfDay > settings.WorkTime.LunchEndTime;

        Console.MarkupLine(
            "{0}: {1}{2} started at {3} ({4}){5}",
            frame.Id,
            $"[green]{frame.Project?.Name ?? "-"}[/]",
            frame.Tags.Count == 0
                ? string.Empty
                : $" ([purple]{string.Join("[/], [purple]", frame.Tags.Select(e => e.Name))}[/])",
            $"[blue]{TimeHelper.FormatTime(frame.TimeAsDateTime.TimeOfDay)}[/]",
            TimeHelper.FormatDuration(
                endTime - frame.TimeAsDateTime -
                (needToSubtractLunchTime
                    ? (settings.WorkTime.LunchEndTime - settings.WorkTime.LunchStartTime)
                    : TimeSpan.Zero)
            ),
            !needToSubtractLunchTime
                ? string.Empty
                : $" [yellow](+{TimeHelper.FormatDuration(settings.WorkTime.LunchEndTime - settings.WorkTime.LunchStartTime)} lunch)[/]"
        );

        return 0;
    }

    #endregion
}