using Watson.Commands.Abstractions;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class ProjectCommand : Command<ProjectOptions>
{
    #region Constructors

    public ProjectCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(ProjectOptions options)
    {
        return options.Action switch
        {
            "add" or "create" => await AddProject(options),
            "remove" or "delete" => await RemoveProject(options),
            "rename" => await RenameProject(options),
            "list" => await ListProjects(options),
            _ => 1
        };
    }

    public override Task ProvideCompletions(string[] inputs)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Private methods

    private async Task<int> AddProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(arguments[0]);
        return project is null ? 1 : 0;
    }

    private async Task<int> RemoveProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 1) return 1;

        return !await ProjectRepository.DeleteAsync(arguments[0]) ? 1 : 0;
    }

    private async Task<int> RenameProject(ProjectOptions options)
    {
        var arguments = options.Arguments.ToList();
        if (arguments.Count != 2) return 1;

        var project = await ProjectRepository.EnsureNameExistsAsync(arguments[0]);
        if (project is null) return 1;

        project.Name = arguments[1];
        return !await ProjectRepository.UpdateAsync(project) ? 1 : 0;
    }

    private async Task<int> ListProjects(ProjectOptions _)
    {
        var projects = await ProjectRepository.GetAsync();
        foreach (var project in projects)
        {
            Console.WriteLine("{0}: {1}", project.Id, project.Name);
        }

        return 0;
    }

    #endregion
}