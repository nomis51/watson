using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class StopCommand : Command<StopOptions>
{
    #region Constructors

    public StopCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(StopOptions options)
    {
        if (string.IsNullOrEmpty(options.AtTime))
        {
            var frame = Frame.CreateEmpty(DateTime.Now.Ticks);
            return await FrameRepository.InsertAsync(frame) is not null ? 0 : 1;
        }

        if (!TimeHelper.ParseDateTime(options.AtTime, out var atTime)) return 1;
        var emptyFrame = Frame.CreateEmpty(atTime!.Value.Ticks);
        return await FrameRepository.InsertAsync(emptyFrame) is not null ? 0 : 1;
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion
}