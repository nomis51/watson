using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class ListCommand : Command<ListOptions>
{
    #region Cosntructors

    public ListCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(ListOptions options)
    {
        if (string.IsNullOrEmpty(options.Resource)) return 1;

        var resources = options.Resource switch
        {
            "project" => (await DependencyResolver.ProjectRepository.GetAsync())
                .Select(e => e.Name),
            "tag" => (await DependencyResolver.TagRepository.GetAsync())
                .Select(e => e.Name),
            _ => null
        };

        if (resources is null) return 1;

        foreach (var resource in resources)
        {
            Console.WriteLine(resource);
        }

        return 0;
    }

    #endregion
}