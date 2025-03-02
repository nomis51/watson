using Watson.Tests.Helpers;

namespace Watson.Tests.Abstractions;

public abstract class ConsoleTest : CommandTest, IDisposable
{
    #region Members

    protected readonly ConsoleHelper ConsoleHelper = new();

    #endregion

    #region Constructors

    public new void Dispose()
    {
        base.Dispose();
        GC.SuppressFinalize(this);
        ConsoleHelper.Dispose();
    }

    #endregion
}