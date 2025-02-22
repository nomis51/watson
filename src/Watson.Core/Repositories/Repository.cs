using System.ComponentModel;
using System.Reflection;
using System.Runtime;
using Dapper;
using Watson.Core.Abstractions;
using Watson.Core.Helpers;
using Watson.Core.Helpers.Abstractions;
using Watson.Core.Models.Abstractions;
using Watson.Core.Repositories.Abstractions;

namespace Watson.Core.Repositories;

public abstract class Repository<TModel> : IRepository<TModel>
    where TModel : DbModel
{
    #region Members

    protected readonly IIdHelper IdHelper;
    protected readonly IAppDbContext DbContext;

    #endregion

    #region Props

    protected string TableName => typeof(TModel).GetCustomAttribute<DescriptionAttribute>()!.Description;

    #endregion

    #region Constructors

    protected Repository(IAppDbContext dbContext, IIdHelper idHelper)
    {
        DbContext = dbContext;
        IdHelper = idHelper;

        InitializeTable();
    }

    #endregion

    #region Public methods

    public virtual Task<TModel?> GetByIdAsync(string id)
    {
        return DbContext.Connection
            .QueryFirstOrDefaultAsync<TModel>(
                $"SELECT * FROM {TableName} WHERE Id = @Id",
                new { Id = id }
            );
    }

    public virtual Task<IEnumerable<TModel>> GetAsync()
    {
        return DbContext.Connection
            .QueryAsync<TModel>($"SELECT * FROM {TableName}");
    }

    public virtual async Task<bool> InsertAsync(TModel model)
    {
        if (string.IsNullOrEmpty(model.Id))
        {
            model.Id = IdHelper.GenerateId();
        }

        var sql = BuildInsertQuery();

        var result = await DbContext.Connection.ExecuteAsync(sql, model);
        return result > 0;
    }

    #endregion

    #region Protected methods

    protected abstract void InitializeTable();
    protected abstract string BuildInsertQuery();

    protected void CreateIndexIfNotExists(string indexName, string statement)
    {
        var result = DbContext.Connection.QuerySingleOrDefault<string>(
            "SELECT name FROM sqlite_master WHERE type='index' AND name=@IndexName",
            new { IndexName = indexName }
        );

        if (!string.IsNullOrEmpty(result)) return;

        DbContext.Connection.Execute(statement);
    }

    #endregion
}