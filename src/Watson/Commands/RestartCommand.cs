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
            var lastFrame = await FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now);
            if (lastFrame is null) return 1;
            if (string.IsNullOrEmpty(lastFrame.ProjectId)) return 1;

            var frame = Frame.CreateEmpty(lastFrame.Timestamp);

            var ok = await FrameRepository.InsertAsync(frame);
            if (!ok) return 1;

            lastFrame.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            ok = await FrameRepository.UpdateAsync(lastFrame);
            return !ok ? 1 : 0;
        }

        var existingFrame = await FrameRepository.GetByIdAsync(options.FrameId);
        if (existingFrame is null) return 1;

        var newFrame = new Frame
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ProjectId = existingFrame.ProjectId
        };

        var ok2 = await FrameRepository.InsertAsync(newFrame);
        if (!ok2) return 1;

        await FrameRepository.AssociateTagsAsync(newFrame.Id,
            existingFrame.Tags.Select(e => e.Name));

        return 0;
    }

    #endregion
}