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
        foreach (var frame in frames)
        {
            Console.WriteLine(frame);
        }
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