using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Database;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public class AliasRepository : Repository<Alias>, IAliasRepository
{
    #region Constructors

    public AliasRepository(IAppDbContext dbContext, IIdHelper idHelper) : base(dbContext, idHelper)
    {
    }

    #endregion

    #region Public methods

    public Task<Alias?> GetByNameAsync(string name)
    {
        return DbContext.Connection
            .QueryFirstOrDefaultAsync<Alias>(
                $"SELECT * FROM {TableName} WHERE Name = @Name",
                new { Name = name }
            );
    }

    #endregion

    #region Protected methods

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute(
            $"""
             CREATE TABLE IF NOT EXISTS {TableName} (
                Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                Name TEXT NOT NULL UNIQUE,
                Command TEXT NOT NULL
             );
             """
        );
    }

    protected override string BuildInsertQuery()
    {
        return $"INSERT INTO {TableName} (Id, Name, Command) VALUES (@Id, @Name, @Command)";
    }

    protected override string BuildUpdateQuery()
    {
        return $"UPDATE {TableName} SET Command = @Command WHERE Id = @Id";
    }

    #endregion
}