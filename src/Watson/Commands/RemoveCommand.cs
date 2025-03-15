using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class RemoveCommand : Command<RemoveOptions>
{
    #region Constructors

    public RemoveCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(RemoveOptions options)
    {
        if (string.IsNullOrEmpty(options.FrameId)) return 1;

        return await FrameRepository.DeleteAsync(options.FrameId) ? 0 : 1;
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion
}