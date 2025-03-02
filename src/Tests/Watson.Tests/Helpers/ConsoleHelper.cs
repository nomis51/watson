using Spectre.Console;

namespace Watson.Tests.Helpers;

public class ConsoleHelper : IDisposable
{
    #region Members

    private static readonly SemaphoreSlim ConsoleLock = new(1, 1);
    private readonly StringWriter _stringWriter = new();
    private readonly TextWriter _mockConsoleOut;
    private readonly TextWriter _originalConsoleOut;
    private readonly IAnsiConsole _originalAnsiConsole = AnsiConsole.Console;

    #endregion

    #region Constructors

    public ConsoleHelper()
    {
        ConsoleLock.Wait();
        _originalConsoleOut = Console.Out;
        _mockConsoleOut = TextWriter.Synchronized(_stringWriter);
        Console.SetOut(_mockConsoleOut);

        AnsiConsole.Console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(Console.Out),
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        RestoreConsole();
        _mockConsoleOut.Dispose();
        _stringWriter.Dispose();
        ConsoleLock.Release();
    }

    #endregion

    #region Public methods

    public string GetMockOutput()
    {
        return _stringWriter.ToString().Trim();
    }

    public string GetSpectreMarkupOutput(string input)
    {
        var writer = new StringWriter();
        var originalOut = AnsiConsole.Profile.Out;
        AnsiConsole.Profile.Out = new AnsiConsoleOutput(writer);
        AnsiConsole.MarkupLine(input);
        AnsiConsole.Profile.Out = originalOut;
        return writer.ToString().Trim();
    }

    #endregion

    #region Private methods

    private void RestoreConsole()
    {
        Console.SetOut(_originalConsoleOut);
        AnsiConsole.Console = _originalAnsiConsole;
    }

    #endregion
}