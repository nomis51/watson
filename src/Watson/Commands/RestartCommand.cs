using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
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
            var lastFrame = await FrameRepository.GetPreviousFrameAsync(DateTime.Now);
            if (lastFrame is null) return 1;
            if (string.IsNullOrEmpty(lastFrame.ProjectId)) return 1;

            var frame = Frame.CreateEmpty(lastFrame.Time);

            frame = await FrameRepository.InsertAsync(frame);
            if (frame is null) return 1;

            lastFrame.Time = DateTime.Now.Ticks;
            return await FrameRepository.UpdateAsync(lastFrame) ? 0 : 1;
        }

        var existingFrame = await FrameRepository.GetByIdAsync(options.FrameId);
        if (existingFrame is null) return 1;

        var newFrame = new Frame
        {
            Time = DateTime.Now.Ticks,
            ProjectId = existingFrame.ProjectId
        };

        var frame1 = await FrameRepository.InsertAsync(newFrame);
        if (frame1 is null) return 1;

        await FrameRepository.AssociateTagsAsync(newFrame.Id,
            existingFrame.Tags.Select(e => e.Name));

        return 0;
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion
}