using Spectre.Console;
using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class LogCommand : Command<LogOptions>
{
    #region Constructors

    public LogCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(LogOptions options)
    {
        if (!string.IsNullOrEmpty(options.FromTime) || !string.IsNullOrEmpty(options.ToTime))
        {
            if (!DependencyResolver.TimeHelper.ParseDateTime(options.FromTime, out var fromTime))
            {
                return 1;
            }

            if (!DependencyResolver.TimeHelper.ParseDateTime(options.ToTime, out var toTime))
            {
                return 1;
            }

            var frames = await RetrieveFrames(
                fromTime!.Value,
                toTime!.Value,
                options.Projects?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ??
                [],
                options.Tags?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? []
            );

            DisplayFrames(frames);
        }

        return 0;
    }

    #endregion

    #region Private methods

    private void DisplayFrames(IEnumerable<Frame> frames)
    {
        // TODO: get from settings
        var dayEndHour = new TimeSpan(21, 0, 0);

        var groupedFrames = frames.GroupBy(e => e.TimestampAsDateTime.Date);
        foreach (var group in groupedFrames)
        {
            var groupFrames = group.OrderByDescending(e => e.Timestamp)
                .ToList();
            var totalTime = GetTotalTime(groupFrames);
            AnsiConsole.WriteLine(
                "{0} ({1})",
                group.Key.ToLocalTime().Date.ToString("dddd dd MMMM yyyy"),
                $"{totalTime.Hours}h {totalTime.Minutes}m"
            );

            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddColumn();
            grid.AddColumn();

            for (var i = 0; i < groupFrames.Count; ++i)
            {
                var frame = groupFrames[i];
                var fromTime = DateTimeOffset.FromUnixTimeSeconds(frame.Timestamp).TimeOfDay;
                var toTime = i + 1 < groupFrames.Count
                    ? DateTimeOffset.FromUnixTimeSeconds(groupFrames[i + 1].Timestamp).TimeOfDay
                    : dayEndHour;
                var duration = toTime - fromTime;

                grid.AddRow(
                    frame.Id,
                    $"{fromTime.Hours.ToString().PadLeft(2, '0')}:{fromTime.Minutes.ToString().PadLeft(2, '0')} to {toTime.Hours.ToString().PadLeft(2, '0')}:{toTime.Minutes.ToString().PadLeft(2, '0')}",
                    $"{duration.Hours.ToString().PadLeft(2, '0')}h {duration.Minutes.ToString().PadLeft(2, '0')}m",
                    frame.Project?.Name ?? "-",
                    frame.Tags.Count == 0 ? string.Empty : $"[[{string.Join(", ", frame.Tags.Select(e => e.Name))}]]"
                );
            }

            var panel = new Panel(grid);
            panel.NoBorder();
            panel.PadLeft(5);
            AnsiConsole.Write(panel);
            AnsiConsole.WriteLine();
        }
    }

    private static TimeSpan GetTotalTime(List<Frame> frames)
    {
        // TODO: get from settings
        var dayEndHour = new TimeSpan(21, 0, 0);

        var totalSeconds = 0L;

        for (var i = frames.Count - 1; i >= 0; --i)
        {
            if (i == frames.Count - 1)
            {
                totalSeconds += Convert.ToInt64(dayEndHour.TotalSeconds) -
                                Convert.ToInt64(DateTimeOffset.FromUnixTimeSeconds(frames[i].Timestamp).TimeOfDay
                                    .TotalSeconds);
            }
            else
            {
                totalSeconds += frames[i + 1].Timestamp - frames[i].Timestamp;
            }
        }

        return TimeSpan.FromSeconds(totalSeconds);
    }

    private async Task<IEnumerable<Frame>> RetrieveFrames(
        DateTimeOffset fromTime,
        DateTimeOffset toTime,
        IEnumerable<string> projects,
        IEnumerable<string> tags
    )
    {
        List<string> projectIds = [];

        foreach (var name in projects)
        {
            var project = await DependencyResolver.ProjectRepository.GetByNameAsync(name);
            if (project is null) continue;

            projectIds.Add(project.Id);
        }

        List<string> tagIds = [];

        foreach (var name in tags)
        {
            var tag = await DependencyResolver.TagRepository.GetByNameAsync(name);
            if (tag is null) continue;

            tagIds.Add(tag.Id);
        }

        return await DependencyResolver.FrameRepository.GetAsync(fromTime, toTime, projectIds, tagIds);
    }

    #endregion
}