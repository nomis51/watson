﻿using Watson.Commands.Abstractions;
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
        if (string.IsNullOrEmpty(options.Project) &&
            string.IsNullOrEmpty(options.FromTime)) return 1;

        var frame = string.IsNullOrEmpty(options.FrameId)
            ? await FrameRepository.GetPreviousFrameAsync(DateTime.Now)
            : await FrameRepository.GetByIdAsync(options.FrameId);
        if (frame is null) return 1;

        if (!string.IsNullOrEmpty(options.Project))
        {
            var project = await ProjectRepository.EnsureNameExistsAsync(options.Project);
            if (project is null) return 1;

            frame.ProjectId = project.Id;
        }

        if (!string.IsNullOrEmpty(options.FromTime))
        {
            if (!TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;
            frame.Time = fromTime!.Value.Ticks;
        }

        var tagList = options.Tags.ToList();
        if (tagList.Count > 0)
        {
            if (!await TagRepository.EnsureTagsExistsAsync(tagList)) return 1;

            await FrameRepository.AssociateTagsAsync(frame.Id, tagList);
        }

        return await FrameRepository.UpdateAsync(frame) ? 0 : 1;
    }

    public override async Task ProvideCompletions(string[] inputs)
    {
        switch (inputs.Length)
        {
            case 1:
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
            case >= 2:
            {
                var lastArg = inputs.Last();
                var tags = await DependencyResolver.TagRepository.GetAsync();
                var tag = tags.FirstOrDefault(e => e.Name.StartsWith(lastArg, StringComparison.OrdinalIgnoreCase));
                if (tag is not null)
                {
                    Console.WriteLine(tag.Name);
                }

                break;
            }
        }
    }

    #endregion
}