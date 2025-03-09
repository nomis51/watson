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

    private static string ProjectTableName => typeof(Project).GetCustomAttribute<DescriptionAttribute>()!.Description;
    private static string TagTableName => typeof(Tag).GetCustomAttribute<DescriptionAttribute>()!.Description;

    private string TodoTagTableName => string.Join(
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

    #region Public methods

    public override async Task<Todo?> InsertAsync(Todo model)
    {
        var todo = await base.InsertAsync(model);
        if (todo is null) return null;

        await InjectProject(todo);
        await InjectTags(todo);
        return todo;
    }

    public async Task AssociateTagsAsync(string todoId, IEnumerable<string> tags)
    {
        var todo = await GetByIdAsync(todoId);
        if (todo is null) return;

        var tagModels = await DbContext.Connection.QueryAsync<Tag>(
            $"SELECT * FROM {TagTableName} WHERE Name IN @Names",
            new { Names = tags }
        );

        foreach (var tagModel in tagModels)
        {
            await DbContext.Connection.ExecuteAsync(
                $"INSERT INTO {TodoTagTableName} (Id, TodoId, TagId) VALUES (@Id, @TodoId, @TagId)",
                new { Id = IdHelper.GenerateId(), TodoId = todoId, TagId = tagModel.Id }
            );
        }
    }

    public async Task<IEnumerable<Todo>> GetAsync(
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
                $" AND Id IN (SELECT TodoId FROM {TodoTagTableName} WHERE TagId IN ('{string.Join("','", tagIds)}'))";
        }

        if (ignoredTagIds.Count != 0)
        {
            sql +=
                $" AND Id NOT IN (SELECT TodoId FROM {TodoTagTableName} WHERE TagId IN ('{string.Join("','", ignoredTagIds)}'))";
        }

        var todos = await DbContext.Connection.QueryAsync<Todo>(
            sql,
            new
            {
                FromTimestamp = fromTimestamp,
                ToTimestamp = toTimestamp,
            }
        );

        var todosLst = todos.ToList();
        foreach (var todo in todosLst)
        {
            await InjectProject(todo);
            await InjectTags(todo);
        }

        return todosLst;
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
                                      CREATE TABLE IF NOT EXISTS {TodoTagTableName} (
                                         Id TEXT NOT NULL PRIMARY KEY UNIQUE,
                                         TodoId TEXT NOT NULL,
                                         TagId TEXT NOT NULL
                                      );
                                      """);

        CreateIndexIfNotExists(
            $"idx_{TodoTagTableName}_pk",
            $"CREATE UNIQUE INDEX idx_{TodoTagTableName}_pk ON {TodoTagTableName} (Id)"
        );
        CreateIndexIfNotExists(
            $"idx_{TodoTagTableName}_TodoId_fk",
            $"CREATE INDEX idx_{TodoTagTableName}_TodoId_fk ON {TodoTagTableName} (TodoId)"
        );
        CreateIndexIfNotExists(
            $"idx_{TodoTagTableName}_TagId_fk",
            $"CREATE INDEX idx_{TodoTagTableName}_TagId_fk ON {TodoTagTableName} (TagId)"
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

    #region Private methods

    private async Task InjectProject(Todo todo)
    {
        var project = await DbContext.Connection.QueryFirstOrDefaultAsync<Project>(
            $"SELECT * FROM {ProjectTableName} WHERE Id = @ProjectId",
            new { todo.ProjectId }
        );
        todo.Project = project;
    }

    private async Task InjectTags(Todo todo)
    {
        var tags = await DbContext.Connection.QueryAsync<Tag>(
            $"SELECT * FROM {TagTableName} WHERE Id IN (SELECT TagId FROM {TodoTagTableName} WHERE TodoId = @TodoId)",
            new { TodoId = todo.Id }
        );
        todo.Tags = tags.ToList();
    }

    #endregion
}