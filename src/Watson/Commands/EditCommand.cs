using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class EditCommand : Command<EditOptions>
{
    #region Constructors

    public EditCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(EditOptions options)
    {
        if (string.IsNullOrEmpty(options.Project) &&
            string.IsNullOrEmpty(options.FromTime)) return 1;

        var frame = string.IsNullOrEmpty(options.FrameId)
            ? await DependencyResolver.FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now)
            : await DependencyResolver.FrameRepository.GetByIdAsync(options.FrameId);
        if (frame is null) return 1;

        if (!string.IsNullOrEmpty(options.Project))
        {
            var project = await DependencyResolver.ProjectRepository.EnsureNameExistsAsync(options.Project);
            if (project is null) return 1;

            frame.ProjectId = project.Id;
        }

        if (!string.IsNullOrEmpty(options.FromTime))
        {
            if (!DependencyResolver.TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;
            frame.Timestamp = fromTime!.Value.ToUnixTimeSeconds();
        }

        return await DependencyResolver.FrameRepository.UpdateAsync(frame) ? 0 : 1;
    }

    #endregion
}