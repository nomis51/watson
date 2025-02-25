using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public partial class AddCommand : Command<AddOptions>
{
    #region Constructors

    public AddCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(AddOptions options)
    {
        if (string.IsNullOrEmpty(options.Project)) return 1;
        if (!DependencyResolver.TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;
        if (!DependencyResolver.TimeHelper.ParseDateTime(options.ToTime, out var toTime)) return 1;

        return await CreateFrame(options.Project, fromTime, toTime, options.Tags);
    }

    #endregion

    #region Private methods

    private async Task<int> CreateFrame(
        string project,
        DateTimeOffset? fromTime,
        DateTimeOffset? toTime,
        IEnumerable<string> tags
    )
    {
        var projectModel = await DependencyResolver.ProjectRepository.EnsureNameExistsAsync(project);
        if (projectModel is null) return 1;

        var tagslst = tags.ToList();
        if (!await DependencyResolver.TagRepository.EnsureTagsExistsAsync(tagslst)) return 1;

        fromTime ??= DateTimeOffset.UtcNow;
        var frame = new Frame
        {
            ProjectId = project,
            Timestamp = fromTime.Value.ToUnixTimeSeconds()
        };

        var ok = await DependencyResolver.FrameHelper.CreateFrame(frame, toTime);
        if (!ok) return 1;

        await DependencyResolver.FrameRepository.AssociateTagsAsync(projectModel.Id, tagslst);
        return 0;
    }

    #endregion
}