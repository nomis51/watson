﻿using Watson.Core.Models.Database;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class CreateCommand : Command<CreateOptions>
{
    #region Constructors

    public CreateCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(CreateOptions options)
    {
        if (string.IsNullOrEmpty(options.Resource)) return 1;
        if (string.IsNullOrEmpty(options.Name)) return 1;

        return options.Resource switch
        {
            "project" => await CreateProject(options.Name),
            "tag" => await CreateTag(options.Name),
            _ => 1
        };
    }

    #endregion

    #region Private methods

    private async Task<int> CreateTag(string name)
    {
        var existingTag = await TagRepository.DoesNameExistAsync(name);
        if (existingTag) return 1;

        var tag = new Tag
        {
            Name = name
        };

        return await TagRepository.InsertAsync(tag) is not null ? 0 : 1;
    }

    private async Task<int> CreateProject(string name)
    {
        var existingProject = await ProjectRepository.DoesNameExistAsync(name);
        if (existingProject) return 1;

        var project = new Project
        {
            Name = name
        };

        return await ProjectRepository.InsertAsync(project) is not null ? 0 : 1;
    }

    #endregion
}