using System.ComponentModel;
using System.Reflection;
using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class FrameRepository : Repository<Frame>, IFrameRepository
{
    #region Props

    private static string ProjectTableName => typeof(Project).GetCustomAttribute<DescriptionAttribute>()!.Description;
    private static string TagTableName => typeof(Tag).GetCustomAttribute<DescriptionAttribute>()!.Description;

    private string FrameTagTableName => string.Join(
        "_",
        TableName,
        TagTableName
    );

    #endregion

    #region Constructors

    public FrameRepository(IAppDbContext dbContext, IIdHelper idHelper) : base(dbContext, idHelper)
    {
    }

    #endregion

    #region Public methods

    public override async Task<Frame?> GetByIdAsync(string id)
    {
        var frame = await base.GetByIdAsync(id);
        if (frame is null) return null;

        await InjectProject(frame);
        await InjectTags(frame);

        return frame;
    }

    public override async Task<IEnumerable<Frame>> GetAsync()
    {
        var frames = await base.GetAsync();
        var framesList = frames.ToList();

        foreach (var frame in framesList)
        {
            await InjectProject(frame);
            await InjectTags(frame);
        }

        return framesList;
    }

    public async Task<Frame?> GetNextFrameAsync(DateTime time)
    {
        var timestamp = time.Ticks;
        var frame = await DbContext.Connection.QueryFirstOrDefaultAsync<Frame>(
            $"SELECT * FROM {TableName} WHERE Time > @Time ORDER BY Time ASC LIMIT 1",
            new { Time = timestamp }
        );
        if (frame is null) return null;

        await InjectProject(frame);
        await InjectTags(frame);
        return frame;
    }

    public async Task<Frame?> GetPreviousFrameAsync(DateTime time)
    {
        var timestamp = time.Ticks;
        var frame = await DbContext.Connection.QueryFirstOrDefaultAsync<Frame>(
            $"SELECT * FROM {TableName} WHERE Time < @Time ORDER BY Time DESC LIMIT 1",
            new { Time = timestamp }
        );
        if (frame is null) return null;

        await InjectProject(frame);
        await InjectTags(frame);
        return frame;
    }

    public Task<IEnumerable<Frame>> GetAsync(DateTime fromTime, DateTime toTime)
    {
        var fromTimestamp = fromTime.Ticks;
        var toTimestamp = toTime.Ticks;
        return DbContext.Connection.QueryAsync<Frame>(
            $"SELECT * FROM {TableName} WHERE Time >= @FromTimestamp AND Time <= @ToTimestamp ORDER BY Time ASC",
            new
            {
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp
            }
        );
    }

    public async Task<IEnumerable<Frame>> GetAsync(
        DateTime fromTime,
        DateTime toTime,
        List<string> projectIds,
        List<string> tagIds,
        List<string> ignoredProjectIds,
        List<string> ignoredTagIds
    )
    {
        var fromTimestamp = fromTime.Ticks;
        var toTimestamp = toTime.Ticks;
        var sql = $"SELECT * FROM {TableName} WHERE Time >= @FromTimestamp AND Time <= @ToTimestamp";

        if (projectIds.Count != 0)
        {
            sql += $" AND ProjectId IN ('{string.Join("','", projectIds)}')";
        }

        if (ignoredProjectIds.Count != 0)
        {
            sql += $" AND ProjectId NOT IN ('{string.Join("','", ignoredProjectIds)}')";
        }

        if (tagIds.Count != 0)
        {
            sql +=
                $" AND Id IN (SELECT FrameId FROM {FrameTagTableName} WHERE TagId IN ('{string.Join("','", tagIds)}'))";
        }

        if (ignoredTagIds.Count != 0)
        {
            sql +=
                $" AND Id NOT IN (SELECT FrameId FROM {FrameTagTableName} WHERE TagId IN ('{string.Join("','", ignoredTagIds)}'))";
        }

        var frames = await DbContext.Connection.QueryAsync<Frame>(
            sql,
            new
            {
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
            }
        );

        var framesLst = frames.ToList();
        foreach (var frame in framesLst)
        {
            await InjectProject(frame);
            await InjectTags(frame);
        }

        return framesLst;
    }

    public async Task AssociateTagsAsync(string frameId, IEnumerable<string> tags)
    {
        var frame = await GetByIdAsync(frameId);
        if (frame is null) return;

        var tagModels = await DbContext.Connection.QueryAsync<Tag>(
            $"SELECT * FROM {TagTableName} WHERE Name IN @Names",
            new { Names = tags }
        );

        foreach (var tagModel in tagModels)
        {
            await DbContext.Connection.ExecuteAsync(
                $"INSERT INTO {FrameTagTableName} (Id, FrameId, TagId) VALUES (@Id, @FrameId, @TagId)",
                new { Id = IdHelper.GenerateId(), FrameId = frameId, TagId = tagModel.Id }
            );
        }
    }

    #endregion

    #region Protected methods

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute($"""
                                      CREATE TABLE IF NOT EXISTS {TableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         Time REAL NOT NULL,
                                         ProjectId TEXT NOT NULL
                                      );
                                      """);

        CreateIndexIfNotExists(
            $"idx_{TableName}_pk",
            $"CREATE UNIQUE INDEX idx_{TableName}_pk ON {TableName} (Id)"
        );
        CreateIndexIfNotExists(
            $"idx_{TableName}_ProjectId_fk",
            $"CREATE INDEX idx_{TableName}_ProjectId_fk ON {TableName} (ProjectId)"
        );

        DbContext.Connection.Execute($"""
                                      CREATE TABLE IF NOT EXISTS {FrameTagTableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         FrameId TEXT NOT NULL,
                                         TagId TEXT NOT NULL
                                      );
                                      """);

        CreateIndexIfNotExists(
            $"idx_{FrameTagTableName}_pk",
            $"CREATE UNIQUE INDEX idx_{FrameTagTableName}_pk ON {FrameTagTableName} (Id)"
        );
        CreateIndexIfNotExists(
            $"idx_{FrameTagTableName}_FrameId_fk",
            $"CREATE INDEX idx_{FrameTagTableName}_FrameId_fk ON {FrameTagTableName} (FrameId)"
        );
        CreateIndexIfNotExists(
            $"idx_{FrameTagTableName}_TagId_fk",
            $"CREATE INDEX idx_{FrameTagTableName}_TagId_fk ON {FrameTagTableName} (TagId)"
        );
    }

    protected override string BuildInsertQuery()
    {
        return $"INSERT INTO {TableName} (Id, Time, ProjectId) VALUES (@Id, @Time, @ProjectId)";
    }

    protected override string BuildUpdateQuery()
    {
        return $"UPDATE {TableName} SET Time = @Time, ProjectId = @ProjectId WHERE Id = @Id";
    }

    #endregion

    #region Private methods

    private async Task InjectProject(Frame frame)
    {
        var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
            $"SELECT * FROM {ProjectTableName} WHERE Id = @ProjectId",
            new { frame.ProjectId }
        );
        frame.Project = project;
    }

    private async Task InjectTags(Frame frame)
    {
        var tags = await DbContext.Connection.QueryAsync<Tag>(
            $"SELECT * FROM {TagTableName} WHERE Id IN (SELECT TagId FROM {FrameTagTableName} WHERE FrameId = @FrameId)",
            new { FrameId = frame.Id }
        );
        frame.Tags = tags.ToList();
    }

    #endregion
}