using System.Text.RegularExpressions;
using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public partial class AddCommand : Command<AddOptions>
{
    #region Constants

    private static readonly Regex RegMonthAndDay = RegMonthAndDayRegex();

    [GeneratedRegex("(1[0-2]|0[1-9]|[1-9])\\-(3[0-1]|2[0-9]|1[0-9]|0[1-9]|[1-9])",
        RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-CA")]
    private static partial Regex RegMonthAndDayRegex();

    #endregion

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
        if (!await DependencyResolver.TagRepository.EnsureTagsExists(tagslst)) return 1;

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