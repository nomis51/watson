using System.ComponentModel;
using System.Reflection;
using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models;
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

    #region Protected methods

    public override async Task<Frame?> GetByIdAsync(string id)
    {
        var frame = await base.GetByIdAsync(id);
        if (frame is null) return null;

        var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
            $"SELECT * FROM {ProjectTableName} WHERE Id = @ProjectId",
            new { frame.ProjectId }
        );
        frame.Project = project;

        var tags = await DbContext.Connection.QueryAsync<Tag>(
            $"SELECT * FROM {FrameTagTableName} LEFT OUTER JOIN {TagTableName} WHERE {FrameTagTableName}.FrameId = @FrameId",
            new { FrameId = id }
        );
        frame.Tags = tags.ToList();

        return frame;
    }

    public override async Task<IEnumerable<Frame>> GetAsync()
    {
        var frames = await base.GetAsync();
        var framesList = frames.ToList();

        foreach (var frame in framesList)
        {
            var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
                $"SELECT * FROM {ProjectTableName} WHERE Id = @ProjectId",
                new { frame.ProjectId }
            );
            frame.Project = project;

            var tags = await DbContext.Connection.QueryAsync<Tag>(
                $"SELECT {TagTableName}.* FROM {FrameTagTableName} LEFT OUTER JOIN {TagTableName} WHERE {FrameTagTableName}.FrameId = @FrameId",
                new { FrameId = frame.Id }
            );
            frame.Tags = tags.ToList();
        }

        return framesList;
    }

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute($"""
                                      CREATE TABLE IF NOT EXISTS {TableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         Timestamp INTEGER NOT NULL,
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
        return $"INSERT INTO {TableName} (Id, Timestamp, ProjectId) VALUES (@Id, @Timestamp, @ProjectId)";
    }

    #endregion
}