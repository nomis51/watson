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

    #region Protected methods

    protected override void InitializeTable()
    {
        var sql = $"""
                   CREATE TABLE IF NOT EXISTS {TableName} (
                      Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                      Name TEXT NOT NULL UNIQUE
                   );
                   CREATE UNIQUE INDEX idx_{TableName}_pk ON {TableName} (Id); 
                   """;
        DbContext.Connection.Execute(sql);
    }

    protected override string BuildInsertQuery()
    {
        return $"INSERT INTO {TableName} (Id, Name) VALUES (@Id, @Name)";
    }

    #endregion
}