using Microsoft.Extensions.Options;
using Watson.Core.Models;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class EditCommand : Command<EditOptions>
{
    #region Constructors

    public EditCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(EditOptions options)
    {
        var frame = string.IsNullOrEmpty(options.FrameId)
            ? await DependencyResolver.FrameRepository.GetPreviousFrameAsync(DateTimeOffset.Now)
            : await DependencyResolver.FrameRepository.GetByIdAsync(options.FrameId);
        if (frame is null) return 1;

        var project = await DependencyResolver.ProjectRepository.EnsureNameExistsAsync(options.Project);
        if (project is null) return 1;

        frame.ProjectId = project.Id;

        if (!string.IsNullOrEmpty(options.FromTime))
        {
            if (!DependencyResolver.TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;
            frame.Timestamp = fromTime!.Value.ToUnixTimeSeconds();
        }

        return await DependencyResolver.FrameRepository.UpdateAsync(frame) ? 0 : 1;
    }

    #endregion

    #region Private methods

    private async Task<string> EnsureProjectExists(string name)
    {
        var existingProject = await DependencyResolver.ProjectRepository.GetByNameAsync(name);
        if (existingProject is not null) return existingProject.Id;

        var project = new Project
        {
            Name = name
        };

        if (!await DependencyResolver.ProjectRepository.InsertAsync(project)) return string.Empty;

        return project.Id;
    }

    #endregion
}