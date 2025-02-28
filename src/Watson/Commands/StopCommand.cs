using Watson.Core.Models;
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
            var frame = Frame.CreateEmpty(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return await FrameRepository.InsertAsync(frame) ? 0 : 1;
        }

        if (!TimeHelper.ParseDateTime(options.AtTime, out var atTime)) return 1;
        var emptyFrame = Frame.CreateEmpty(atTime!.Value.ToUnixTimeSeconds());
        return await FrameRepository.InsertAsync(emptyFrame) ? 0 : 1;
    }

    #endregion
}