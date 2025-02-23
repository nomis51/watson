using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class CancelCommand : Command<CancelOptions>
{
    #region Constructors

    public CancelCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(CancelOptions options)
    {
        var lastFrame = await DependencyResolver.FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now);
        if (lastFrame is null) return 1;

        var emptyFrame = Frame.CreateEmpty(lastFrame.Timestamp);
        emptyFrame.Id = lastFrame.Id;

        return await DependencyResolver.FrameRepository.UpdateAsync(emptyFrame) ? 0 : 1;
    }

    #endregion
}