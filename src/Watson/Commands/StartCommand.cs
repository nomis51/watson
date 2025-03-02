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
        return new AddCommand(DependencyResolver)
            .Run(new AddOptions
            {
                FromTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                Project = options.Project,
                Tags = options.Tags
            });
    }

    #endregion
}