using System.Runtime.InteropServices.JavaScript;
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
            if (!TimeHelper.ParseDateTime(options.FromTime, out var fromTime))
            {
                return 1;
            }

            if (!TimeHelper.ParseDateTime(options.ToTime, out var toTime))
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

        var groupedFrames = frames.GroupBy(e => e.TimeAsDateTime.Date);
        foreach (var group in groupedFrames)
        {
            var groupFrames = group.OrderByDescending(e => e.Time)
                .ToList();
            var totalTime = TimeHelper.GetDuration(groupFrames, dayEndHour);
            AnsiConsole.WriteLine(
                "{0} ({1})",
                TimeHelper.FormatDate(group.Key.ToLocalTime()),
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
                var fromTime = new DateTime(frame.Time).TimeOfDay;
                var toTime = i + 1 < groupFrames.Count
                    ? new DateTime(groupFrames[i + 1].Time).TimeOfDay
                    : dayEndHour;
                var duration = toTime - fromTime;

                grid.AddRow(
                    frame.Id,
                    $"{TimeHelper.FormatTime(fromTime)} to {TimeHelper.FormatTime(toTime)}",
                    TimeHelper.FormatDuration(duration),
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

    private async Task<IEnumerable<Frame>> RetrieveFrames(
        DateTime fromTime,
        DateTime toTime,
        IEnumerable<string> projects,
        IEnumerable<string> tags
    )
    {
        List<string> projectIds = [];

        foreach (var name in projects)
        {
            var project = await ProjectRepository.GetByNameAsync(name);
            if (project is null) continue;

            projectIds.Add(project.Id);
        }

        List<string> tagIds = [];

        foreach (var name in tags)
        {
            var tag = await TagRepository.GetByNameAsync(name);
            if (tag is null) continue;

            tagIds.Add(tag.Id);
        }

        return await FrameRepository.GetAsync(fromTime, toTime, projectIds, tagIds);
    }

    #endregion
}