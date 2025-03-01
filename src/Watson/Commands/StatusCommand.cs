using Spectre.Console;
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
        var frame = await FrameRepository.GetPreviousFrameAsync(DateTime.Now);
        if (frame is null) return 1;

        AnsiConsole.MarkupLine(
            "{0}: {1} ({2}) started at {3} ({4})",
            frame.Id,
            $"[green]{frame.Project?.Name ?? "-"}[/]",
            $"[purple]{string.Join("[/], [purple]", frame.Tags.Select(e => e.Name))}[/]",
            $"[blue]{TimeHelper.FormatTime(frame.TimeAsDateTime.TimeOfDay)}[/]",
            TimeHelper.FormatDuration(DateTime.Now - frame.TimeAsDateTime)
        );

        return 0;
    }

    #endregion
}