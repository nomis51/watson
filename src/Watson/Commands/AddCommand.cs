﻿using Watson.Commands.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Models.Settings;
using Watson.Models.Abstractions;
using Watson.Models.CommandLine;

namespace Watson.Commands;

public class AddCommand : Command<AddOptions>
{
    #region Constructors

    public AddCommand(IDependencyResolver dependencyResolver) : base(dependencyResolver)
    {
    }

    #endregion

    #region Public methods

    public override async Task<int> Run(AddOptions options)
    {
        if (string.IsNullOrEmpty(options.Project)) return 1;
        if (!TimeHelper.ParseDateTime(options.FromTime, out var fromTime)) return 1;

        var settings = await SettingsRepository.GetSettings();
        if (fromTime.HasValue && (fromTime.Value.TimeOfDay < settings.WorkTime.StartTime ||
                                  fromTime.Value.TimeOfDay > settings.WorkTime.EndTime))
        {
            var customWorkHours = settings.CustomWorkTimes.Find(e => e.Date.Date == DateTime.Today.Date);
            if (customWorkHours is null)
            {
                customWorkHours = new SettingsCustomWorkTime
                {
                    Date = DateTime.Today,
                    WorkTime = new SettingsWorkTime
                    {
                        StartTime = settings.WorkTime.StartTime,
                        EndTime = settings.WorkTime.EndTime,
                        LunchStartTime = settings.WorkTime.LunchStartTime,
                        LunchEndTime = settings.WorkTime.LunchEndTime,
                        WeekStartDay = settings.WorkTime.WeekStartDay
                    }
                };

                settings.CustomWorkTimes.Add(customWorkHours);
            }

            customWorkHours.WorkTime.StartTime = fromTime.Value.TimeOfDay;
            await SettingsRepository.SaveSettings(settings);
        }

        if (!TimeHelper.ParseDateTime(options.ToTime, out var toTime)) return 1;
        if (toTime.HasValue && (toTime.Value.TimeOfDay < settings.WorkTime.StartTime ||
                                toTime.Value.TimeOfDay > settings.WorkTime.EndTime)) return 1;

        if (toTime is not null && fromTime is null) return 1;
        if (toTime <= fromTime) return 1;
        if (toTime >= DateTime.Now) return 1;

        return await CreateFrame(options.Project, fromTime, toTime, options.Tags);
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

    #region Private methods

    private async Task<int> CreateFrame(
        string project,
        DateTime? fromTime,
        DateTime? toTime,
        IEnumerable<string> tags
    )
    {
        var projectModel = await ProjectRepository.EnsureNameExistsAsync(project);
        if (projectModel is null) return 1;

        var tagsLst = tags.ToList();
        if (!await TagRepository.EnsureTagsExistsAsync(tagsLst)) return 1;

        fromTime ??= DateTime.Now;
        var frame = new Frame
        {
            ProjectId = projectModel.Id,
            Time = fromTime.Value.Ticks
        };

        frame = await FrameHelper.CreateFrame(frame, toTime);
        if (frame is null) return 1;

        await FrameRepository.AssociateTagsAsync(frame.Id, tagsLst);
        frame = await FrameRepository.GetByIdAsync(frame.Id);
        if (frame is null) return 1;

        var nextFrame = await FrameRepository.GetNextFrameAsync(frame.TimeAsDateTime);
        if (nextFrame is null)
        {
            return await new StatusCommand(DependencyResolver)
                .Run(new StatusOptions());
        }

        if (frame.TimeAsDateTime.Date == DateTime.Today)
        {
            DisplayFrame(frame, nextFrame);
            return 0;
        }


        DisplayFrame(frame, nextFrame, true);
        return 0;
    }

    #endregion

    #region Private methods

    private void DisplayFrame(Frame frame, Frame nextFrame, bool fullDate = false)
    {
        var fromTime = TimeHelper.FormatTime(frame.TimeAsDateTime.TimeOfDay);
        var toTime = TimeHelper.FormatTime(nextFrame.TimeAsDateTime.TimeOfDay);
        Console.MarkupLine(
            "{0}: [green]{1}[/] ([purple]{2}[/]) added from [blue]{3}[/] to [blue]{4}[/] ({5})",
            frame.Id,
            frame.Project?.Name ?? "-",
            string.Join(", ", frame.Tags.Select(e => e.Name)),
            fullDate ? $"{frame.TimeAsDateTime.Date:yyyy-MM-dd} {fromTime}" : fromTime,
            fullDate ? $"{nextFrame.TimeAsDateTime.Date:yyyy-MM-dd} {toTime}" : toTime,
            TimeHelper.FormatDuration(nextFrame.TimeAsDateTime - frame.TimeAsDateTime)
        );
    }

    #endregion
}