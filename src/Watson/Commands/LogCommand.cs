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
        DateTime? fromTime = null;
        DateTime? toTime = null;

        if (!string.IsNullOrEmpty(options.FromTime) || !string.IsNullOrEmpty(options.ToTime))
        {
            if (!TimeHelper.ParseDateTime(options.FromTime, out fromTime))
            {
                return 1;
            }

            if (!TimeHelper.ParseDateTime(options.ToTime, out toTime))
            {
                return 1;
            }
        }
        else if (options.Year)
        {
            fromTime = new DateTime(DateTime.Now.Year, 1, 1);
            toTime = new DateTime(DateTime.Now.Year, 12, 31, 23, 59, 59);
        }
        else if (options.Month)
        {
            fromTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            toTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 23, 59, 59);
        }
        else if (options.Week)
        {
            // TODO: get week start day from settings
            var date = DateTime.Now;
            while (date.DayOfWeek != DayOfWeek.Sunday)
            {
                date = date.AddDays(-1);
            }

            fromTime = date;
            toTime = date.AddDays(6);
        }
        else if (options.Day)
        {
            fromTime = DateTime.Now.Date;
            toTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        }
        else if (options.All)
        {
            fromTime = DateTime.MinValue;
            toTime = DateTime.MaxValue;
        }

        if (!fromTime.HasValue || !toTime.HasValue) return 1;

        var frames = await RetrieveFrames(
            fromTime.Value,
            toTime.Value,
            options.Projects?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ??
            [],
            options.Tags?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [],
            options.IgnoredProjects?.Split(',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ??
            [],
            options.IgnoredTags?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ??
            []
        );

        DisplayFrames(frames, options.Reverse);
        return 0;
    }

    #endregion

    #region Private methods

    private void DisplayFrames(IEnumerable<Frame> frames, bool reversed)
    {
        // TODO: get from settings
        var dayEndHour = new TimeSpan(21, 0, 0);

        var groupedFrames = frames.GroupBy(e => e.TimeAsDateTime.Date);

        if (reversed)
        {
            groupedFrames = groupedFrames.Reverse();
        }

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
                var toTime = new DateTime(frame.Time).TimeOfDay;
                var fromTime = i + 1 < groupFrames.Count
                    ? new DateTime(groupFrames[i + 1].Time).TimeOfDay
                    : dayEndHour;
                var duration = toTime - fromTime;

                grid.AddRow(
                    new Text(frame.Id),
                    new Text($"{TimeHelper.FormatTime(fromTime)} to {TimeHelper.FormatTime(toTime)}"),
                    new Markup($"[blue]{TimeHelper.FormatDuration(duration)}[/]"),
                    new Markup($"[green]{frame.Project?.Name ?? "-"}[/]"),
                    new Markup(
                        frame.Tags.Count == 0
                            ? string.Empty
                            : $"([purple]{string.Join("[/], [purple]", frame.Tags.Select(e => e.Name))}[/])"
                    )
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
        IEnumerable<string> tags,
        IEnumerable<string> ignoredProjects,
        IEnumerable<string> ignoredTags
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

        List<string> ignoredProjectIds = [];

        foreach (var name in ignoredProjects)
        {
            var project = await ProjectRepository.GetByNameAsync(name);
            if (project is null) continue;

            ignoredProjectIds.Add(project.Id);
        }

        List<string> ignoredTagIds = [];

        foreach (var name in ignoredTags)
        {
            var tag = await TagRepository.GetByNameAsync(name);
            if (tag is null) continue;

            ignoredTagIds.Add(tag.Id);
        }

        return await FrameRepository.GetAsync(
            fromTime,
            toTime,
            projectIds,
            tagIds,
            ignoredProjectIds,
            ignoredTagIds
        );
    }

    #endregion
}