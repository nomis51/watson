using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class StartCommand : Command<StartOptions>
{
    #region Constructors

    public StartCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override Task<int> Run(StartOptions options)
    {
        var addOptions = new AddOptions
        {
            Project = options.Project,
            Tags = options.Tags
        };

        if (!string.IsNullOrEmpty(options.FromTime))
        {
            addOptions.FromTime = options.FromTime;
        }
        else if (!string.IsNullOrEmpty(options.AtTime))
        {
            addOptions.FromTime = options.AtTime;
        }

        return new AddCommand(DependencyResolver)
            .Run(addOptions);
    }

    #endregion
}