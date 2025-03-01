using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
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

    public Task<Project?> GetByNameAsync(string name)
    {
        return DbContext.Connection
            .QueryFirstOrDefaultAsync<Project>(
                $"SELECT * FROM {TableName} WHERE Name = @Name",
                new { Name = name }
            );
    }

    public async Task<Project?> EnsureNameExistsAsync(string name)
    {
        var existingProject = await GetByNameAsync(name);
        if (existingProject is not null) return existingProject;

        var project = new Project
        {
            Name = name
        };

        if (!await InsertAsync(project)) return null;

        return project;
    }

    public Task<bool> RenameAsync(string id, string name)
    {
        return UpdateAsync(new Project
        {
            Id = id,
            Name = name
        });
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

    protected override string BuildUpdateQuery()
    {
        return $"UPDATE {TableName} SET Name = @Name WHERE Id = @Id";
    }

    #endregion
}