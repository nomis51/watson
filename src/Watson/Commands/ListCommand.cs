using Watson.Commands.Abstractions;
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
            "project" => (await ProjectRepository.GetAsync())
                .Select(e => (e.Id, e.Name)),
            "tag" => (await TagRepository.GetAsync())
                .Select(e => (e.Id, e.Name)),
            _ => null
        };

        if (resources is null) return 1;

        foreach (var (id, name) in resources)
        {
            Console.WriteLine("{0}: {1}", id, name);
        }

        return 0;
    }

    #endregion
}