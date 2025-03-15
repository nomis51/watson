using System.Reflection;
using CommandLine;
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

    public override async Task ProvideCompletions(string[] inputs)
    {
        if (inputs.Length == 1)
        {
            var projects = await DependencyResolver.ProjectRepository.GetAsync();
            var project =
                projects.FirstOrDefault(e => e.Name.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase));
            if (project is not null)
            {
                Console.WriteLine(project.Name);
            }

            return;
        }

        if (inputs.Length >= 2)
        {
            var lastArg = inputs.Last();
            var tags = await DependencyResolver.TagRepository.GetAsync();
            var tag = tags.FirstOrDefault(e => e.Name.StartsWith(lastArg, StringComparison.OrdinalIgnoreCase));
            if (tag is not null)
            {
                Console.WriteLine(tag.Name);
            }
        }
    }

    #endregion
}