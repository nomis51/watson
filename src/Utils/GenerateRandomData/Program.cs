using Dapper;
using Watson.Core;
using Watson.Core.Helpers;
using Watson.Core.Models.Database;
using Watson.Core.Repositories;

var projects = new[]
{
    "cooking",
    "gaming",
    "sleeping",
    "reading",
    "tv",
    "music"
};

var tags = new[]
{
    "banana",
    "apple",
    "spiderman",
    "batman",
    "1981",
    "alice wonderland",
    "metallica",
    "beatles",
};

var startTime = DateTime.Now.AddDays(-14);
var endTime = DateTime.Now;
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
        var tag = tags[Random.Shared.Next(0, tags.Length)];
        var project = await projectRepository.EnsureNameExistsAsync(projectName);
        var frame = new Frame
        {
            Time = currentDate.Add(currentTime).Ticks,
            ProjectId = project!.Id,
        };
        await frameRepository.InsertAsync(frame);
        await tagRepository.EnsureTagsExistsAsync([tag]);
        await frameRepository.AssociateTagsAsync(frame.Id, [tag]);
    }

    currentDate = currentDate.AddDays(1);
}