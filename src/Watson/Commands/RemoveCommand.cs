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
        if (string.IsNullOrEmpty(options.Resource)) return 1;
        if (string.IsNullOrEmpty(options.ResourceId)) return 1;
        
        return options.Resource switch
        {
            "project" => await DependencyResolver.ProjectRepository.DeleteAsync(options.ResourceId),
            "tag" => await DependencyResolver.TagRepository.DeleteAsync(options.ResourceId),
            "frame" => await DependencyResolver.FrameRepository.DeleteAsync(options.ResourceId),
            _ => false
        } ? 0 : 1;
    }

    #endregion
}