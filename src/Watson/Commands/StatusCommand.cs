using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class StatusCommand : Command<StatusOptions>
{
    #region Constructors

    public StatusCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(StatusOptions options)
    {
        var frame = await DependencyResolver.FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now);
        if (frame is null) return 1;

        Console.WriteLine(
            "{0}: {1} [{2}] started at {3:HH:mm} ({4:HH:mm})",
            frame.Id,
            frame.Project?.Name,
            frame.Tags.Count,
            frame.TimestampAsDateTime,
            (DateTimeOffset.UtcNow - frame.TimestampAsDateTime).Duration()
        );

        return 0;
    }

    #endregion
}