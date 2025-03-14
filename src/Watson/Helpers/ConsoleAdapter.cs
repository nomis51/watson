using Spectre.Console;
using Spectre.Console.Rendering;
using Watson.Helpers.Abstractions;

namespace Watson.Helpers;

public class ConsoleAdapter : IConsoleAdapter
{
    #region Members

    private readonly IAnsiConsole _ansiConsole = AnsiConsole.Console;

    #endregion

    #region Constructors

    public void WriteLine(string text)
    {
        _ansiConsole.WriteLine(text);
    }

    public void WriteLine(string text, params object[] args)
    {
        _ansiConsole.MarkupLine("{0}", args);
    }

    public void WriteLine()
    {
        _ansiConsole.WriteLine();
    }

    public void MarkupLine(string text)
    {
        _ansiConsole.MarkupLine(text);
    }

    public void MarkupLine(string text, params object[] args)
    {
        _ansiConsole.MarkupLine(text, args);
    }

    public void Write(IRenderable renderable)
    {
        _ansiConsole.Write(renderable);
    }

    #endregion
}