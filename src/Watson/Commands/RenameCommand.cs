using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class RenameCommand : Command<RenameOptions>
{
    #region Constructors

    public RenameCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(RenameOptions options)
    {
        if (string.IsNullOrEmpty(options.Resource)) return 1;
        if (string.IsNullOrEmpty(options.ResourceId)) return 1;
        if (string.IsNullOrWhiteSpace(options.Name)) return 1;

        return options.Resource switch
        {
            "project" => await ProjectRepository.RenameAsync(options.ResourceId, options.Name),
            "tag" => await TagRepository.RenameAsync(options.ResourceId, options.Name),
            _ => false
        }
            ? 0
            : 1;
    }

    #endregion
}