using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class AddCommand : Command<AddOptions>
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
        if (!TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;
        if (!TimeHelper.ParseDateTime(options.ToTime, out var toTime)) return 1;
        if (toTime is not null && fromTime is null) return 1;
        if (toTime <= fromTime) return 1;
        if (toTime >= DateTimeOffset.Now) return 1;

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
        var projectModel = await ProjectRepository.EnsureNameExistsAsync(project);
        if (projectModel is null) return 1;

        var tagslst = tags.ToList();
        if (!await TagRepository.EnsureTagsExistsAsync(tagslst)) return 1;

        fromTime ??= DateTimeOffset.Now;
        var frame = new Frame
        {
            ProjectId = projectModel.Id,
            Timestamp = fromTime.Value.ToUnixTimeSeconds()
        };

        var ok = await FrameHelper.CreateFrame(frame, toTime);
        if (!ok) return 1;

        await FrameRepository.AssociateTagsAsync(projectModel.Id, tagslst);
        return 0;
    }

    #endregion
}