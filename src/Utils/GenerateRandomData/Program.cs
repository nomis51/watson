using Dapper;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;

var projects = new[]
{
    "cooking",
    "eating"
};

var tags = new[]
{
    "pizza",
    "pasta",
    "vegetables",
    "fruits",
    "meat",
    "tofu"
};

var startTime = DateTime.Now.AddDays(-14);
var endTime = DateTime.Now.AddDays(1);
var startDayHour = new TimeSpan(8, 0, 0);
var endDayHour = new TimeSpan(16, 0, 0);

var dbContext = new AppDbContext();
var idHelper = new IdHelper();

dbContext.Connection.Execute("DROP TABLE IF EXISTS Frames;");
dbContext.Connection.Execute("DROP TABLE IF EXISTS Tags;");
dbContext.Connection.Execute("DROP TABLE IF EXISTS Projects;");
dbContext.Connection.Execute("DROP TABLE IF EXISTS Frames_Tags;");

var frameRepository = new FrameRepository(dbContext, idHelper);
var projectRepository = new ProjectRepository(dbContext, idHelper);
var tagRepository = new TagRepository(dbContext, idHelper);

var currentDate = startTime.Date;
while (currentDate < endTime.Date)
{
    var currentTime = startDayHour;

    while (true)
    {
        currentTime = currentTime.Add(TimeSpan.FromMinutes(Random.Shared.Next(15, 80)));
        if (currentTime > endDayHour) break;

        var projectName = projects[Random.Shared.Next(0, projects.Length)];
        var selectedTags = Enumerable.Range(0, Random.Shared.Next(0, 3))
            .Select(_ => tags[Random.Shared.Next(0, tags.Length)])
            .ToList();
        var project = await projectRepository.EnsureNameExistsAsync(projectName);
        var frame = new Frame
        {
            Time = currentDate.Add(currentTime).Ticks,
            ProjectId = project!.Id,
        };
        await frameRepository.InsertAsync(frame);
        await tagRepository.EnsureTagsExistsAsync(selectedTags);
        await frameRepository.AssociateTagsAsync(frame.Id, selectedTags);
    }

    currentDate = currentDate.AddDays(1);
}