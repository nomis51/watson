using Watson.Core;

namespace Watson.Tests.Abstractions;

public abstract class CommandTest : IDisposable
{
    #region Members

    protected readonly AppDbContext DbContext;
    private readonly string _dbFilePath = Path.GetTempFileName();

    #endregion

    #region Constructors

    protected CommandTest()
    {
        DbContext = new AppDbContext($"Data Source={_dbFilePath};Cache=Shared;Pooling=False");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DbContext.Connection.Close();
        DbContext.Connection.Dispose();

        if (File.Exists(_dbFilePath))
        {
            File.Delete(_dbFilePath);
        }
    }

    #endregion
}