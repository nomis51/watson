using System.Text;
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

    public ConsoleAdapter()
    {
        _ansiConsole.Profile.Encoding = Encoding.Unicode;
    }

    #endregion

    #region Public methods

    public void WriteLine(string text)
    {
        _ansiConsole.WriteLine(text);
    }

    public void WriteLine(string text, params object[] args)
    {
        _ansiConsole.MarkupLine(text, args);
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

    public void Write(string text)
    {
        _ansiConsole.Write(text);
    }

    public void SetEncoding(Encoding encoding)
    {
        _ansiConsole.Profile.Encoding = encoding;
    }

    #endregion
}