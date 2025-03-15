using Spectre.Console;
using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class StatsCommand : Command<StatsOptions>
{
    #region Constants

    private readonly Color[] _colors =
    [
        Color.Red,
        Color.Blue,
        Color.Orange1,
        Color.Green,
        Color.Yellow,
        Color.Pink1,
        Color.Gold1,
        Color.Aqua,
        Color.Grey,
        Color.Chartreuse1,
        Color.Magenta1,
        Color.Purple,
        Color.Salmon1,
        Color.SpringGreen1
    ];

    #endregion

    #region Constructors

    public StatsCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(StatsOptions options)
    {
        if (string.IsNullOrEmpty(options.Type)) return 1;

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
            var settings = await SettingsRepository.GetSettings();

            var date = DateTime.Now;
            while (date.DayOfWeek != settings.WorkTime.WeekStartDay)
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

        DisplayStats(options.Type, frames.ToList());
        return 0;
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private methods

    private void DisplayStats(string type, List<Frame> frames)
    {
        switch (type)
        {
            case "projects":
                DisplayProjectsStats(frames);
                break;
        }
    }

    private void DisplayProjectsStats(List<Frame> frames)
    {
        Dictionary<string, int> projects = [];
        foreach (var frame in frames)
        {
            var name = frame.Project?.Name ?? "Unknown";
            if (!projects.TryAdd(name, 1))
            {
                ++projects[name];
            }
        }

        Console.Write(
            new BarChart()
                .Label("[bold underline]Projects statistics[/]")
                .Width(60)
                .LeftAlignLabel()
                .WithMaxValue(100)
                .AddItems(
                    projects.Select(e =>
                        new BarChartItem(
                            e.Key,
                            Math.Round(e.Value / (double)frames.Count * 100, 0),
                            _colors[Random.Shared.Next(0, _colors.Length)]
                        )
                    )
                )
        );
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