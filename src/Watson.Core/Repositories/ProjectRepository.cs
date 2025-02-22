using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    #region Constructors

    public ProjectRepository(IAppDbContext dbContext, IIdHelper idHelper) : base(dbContext, idHelper)
    {
    }

    #endregion

    #region Public methods

    public Task<bool> DoesNameExistAsync(string name)
    {
        return DbContext.Connection
            .QueryFirstOrDefaultAsync<bool>(
                $"SELECT 1 FROM {TableName} WHERE Name = @Name",
                new { Name = name }
            );
    }

    #endregion


    #region Protected methods

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute($"""
                                      CREATE TABLE IF NOT EXISTS {TableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         Name TEXT NOT NULL UNIQUE
                                      );
                                      """);
        CreateIndexIfNotExists(
            $"idx_{TableName}_pk",
            $"CREATE UNIQUE INDEX idx_{TableName}_pk ON {TableName} (Id)"
        );
    }

    protected override string BuildInsertQuery()
    {
        return $"INSERT INTO {TableName} (Id, Name) VALUES (@Id, @Name)";
    }

    #endregion
}