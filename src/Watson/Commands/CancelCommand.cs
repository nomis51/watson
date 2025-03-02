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
        var lastFrame = await FrameRepository.GetPreviousFrameAsync(DateTime.Now);
        if (lastFrame is null) return 1;
        if (string.IsNullOrEmpty(lastFrame.ProjectId)) return 1;

        return await FrameRepository.DeleteAsync(lastFrame.Id) ? 0 : 1;
    }

    #endregion
}