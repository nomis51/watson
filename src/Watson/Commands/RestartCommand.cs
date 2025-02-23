using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class RestartCommand : Command<RestartOptions>
{
    #region Constructors

    public RestartCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(RestartOptions options)
    {
        if (string.IsNullOrEmpty(options.FrameId))
        {
            var lastFrame = await DependencyResolver.FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now);
            if (lastFrame is null) return 1;
            if (!string.IsNullOrEmpty(lastFrame.ProjectId)) return 1;

            var lastNonEmptyFrame =
                await DependencyResolver.FrameRepository.GetPreviousFrameAsync(lastFrame.TimestampAsDateTime);
            if (lastNonEmptyFrame is null) return 1;

            var frame = new Frame
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ProjectId = lastNonEmptyFrame.ProjectId
            };

            var ok = await DependencyResolver.FrameRepository.InsertAsync(frame);
            if (!ok) return 1;

            await DependencyResolver.FrameRepository.AssociateTagsAsync(frame.Id,
                lastNonEmptyFrame.Tags.Select(e => e.Name));

            return 0;
        }

        var existingFrame = await DependencyResolver.FrameRepository.GetByIdAsync(options.FrameId);
        if (existingFrame is null) return 1;

        var newFrame = new Frame
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ProjectId = existingFrame.ProjectId
        };

        var ok2 = await DependencyResolver.FrameRepository.InsertAsync(newFrame);
        if (!ok2) return 1;

        await DependencyResolver.FrameRepository.AssociateTagsAsync(newFrame.Id,
            existingFrame.Tags.Select(e => e.Name));

        return 0;
    }

    #endregion
}