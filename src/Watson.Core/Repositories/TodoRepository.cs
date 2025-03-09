using System.ComponentModel;
using System.Reflection;
using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class TodoRepository : Repository<Todo>, ITodoRepository
{
    #region Props

    private static string TagTableName => typeof(Tag).GetCustomAttribute<DescriptionAttribute>()!.Description;

    private string FrameTagTableName => string.Join(
        "_",
        TableName,
        TagTableName
    );

    #endregion

    #region Constructors

    public TodoRepository(IAppDbContext dbContext, IIdHelper idHelper) : base(dbContext, idHelper)
    {
    }

    #endregion

    #region Protected methods

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute($"""
                                      CREATE TABLE IF NOT EXISTS {TableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         Description TEXT NOT NULL,
                                         ProjectId TEXT NOT NULL,
                                         DueTime REAL DEFAULT NULL,
                                         Priority INT DEFAULT NULL,
                                         IsCompleted INT DEFAULT 0
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
        return
            $"INSERT INTO {TableName} (Id, Description, ProjectId, DueTime, Priority) VALUES (@Id, @Description, @ProjectId, @DueTime, @Priority)";
    }

    protected override string BuildUpdateQuery()
    {
        return
            $"UPDATE {TableName} SET Description = @Description, ProjectId = @ProjectId, DueTime = @DueTime, Priority = @Priority WHERE Id = @Id";
    }

    #endregion
}