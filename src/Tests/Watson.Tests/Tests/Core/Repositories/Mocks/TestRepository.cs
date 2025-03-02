using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Repositories;

namespace Watson.Tests.Tests.Core.Repositories.Mocks;

public class TestRepository : Repository<TestModel>
{
    #region Constructors

    public TestRepository(IAppDbContext dbContext, IIdHelper idHelper)
        : base(dbContext, idHelper)
    {
    }

    #endregion

    #region Protected methods

    protected override void InitializeTable()
    {
        DbContext.Connection.Execute(
            $"""
                         CREATE TABLE IF NOT EXISTS {TableName} (
                             Id TEXT PRIMARY KEY,
                             Name TEXT NOT NULL
                         );
             """
        );
    }

    protected override string BuildInsertQuery()
    {
        return $"INSERT INTO {TableName} (Id,Name) VALUES (@Id,@Name)";
    }

    protected override string BuildUpdateQuery()
    {
        return $"UPDATE {TableName} SET Name = @Name WHERE Id = @Id";
    }

    #endregion
}