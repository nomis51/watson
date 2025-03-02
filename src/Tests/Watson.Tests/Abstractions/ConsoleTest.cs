using Watson.Tests.Helpers;

namespace Watson.Tests.Abstractions;

public abstract class ConsoleTest : IDisposable
{
    #region Members

    protected readonly ConsoleHelper ConsoleHelper = new();

    #endregion

    #region Constructors

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ConsoleHelper.Dispose();
    }

    #endregion
}