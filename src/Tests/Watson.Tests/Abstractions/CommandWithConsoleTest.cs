using NSubstitute;
using Spectre.Console;
using Spectre.Console.Rendering;
using Watson.Helpers.Abstractions;

namespace Watson.Tests.Abstractions;

public abstract class CommandWithConsoleTest : CommandTest, IDisposable
{
    #region Members

    protected readonly IConsoleAdapter ConsoleAdapter = Substitute.For<IConsoleAdapter>();
    private readonly List<string> _consoleOutput = [];

    #endregion

    #region Constructors

    protected CommandWithConsoleTest()
    {
        ConsoleAdapter.When(e => e.MarkupLine(Arg.Any<string>()))
            .Do(e => _consoleOutput.Add(
                GenerateSpectreMarkupOutput(e.Arg<string>())
            ));

        ConsoleAdapter.When(e => e.MarkupLine(Arg.Any<string>(), Arg.Any<object[]>()))
            .Do(e => _consoleOutput.Add(
                GenerateSpectreMarkupOutput(e.Arg<string>(), e.Arg<object[]>())
            ));

        ConsoleAdapter.When(e => e.WriteLine(Arg.Any<string>()))
            .Do(e => _consoleOutput.Add(e.Arg<string>()));

        ConsoleAdapter.When(e => e.WriteLine(Arg.Any<string>(), Arg.Any<object[]>()))
            .Do(e => _consoleOutput.Add(string.Format(e.Arg<string>(), e.Arg<object[]>())));

        ConsoleAdapter.When(e => e.Write(Arg.Any<IRenderable>()))
            .Do(e => _consoleOutput.Add(
                GenerateSpectreRenderableOutput(e.Arg<IRenderable>())
            ));
    }

    public new void Dispose()
    {
        GC.SuppressFinalize(this);
        base.Dispose();
    }

    #endregion

    #region Protected methods

    protected List<string> GetConsoleOutputLines()
    {
        return _consoleOutput;
    }

    protected string GetConsoleOutput()
    {
        return string.Join(Environment.NewLine, _consoleOutput);
    }

    protected static string GenerateSpectreMarkupOutput(string input, params object[]? args)
    {
        using var writer = new StringWriter();
        var ansiConsole = AnsiConsole.Create(new AnsiConsoleSettings());
        var originalOut = ansiConsole.Profile.Out;
        ansiConsole.Profile.Out = new AnsiConsoleOutput(writer);

        if (args is not null)
        {
            ansiConsole.MarkupLine(input, args);
        }
        else
        {
            ansiConsole.MarkupLine(input);
        }

        ansiConsole.Profile.Out = originalOut;
        return writer.ToString().Trim();
    }

    protected static string GenerateSpectreRenderableOutput(IRenderable renderable)
    {
        using var writer = new StringWriter();
        var ansiConsole = AnsiConsole.Create(new AnsiConsoleSettings());
        var originalOut = ansiConsole.Profile.Out;
        ansiConsole.Profile.Out = new AnsiConsoleOutput(writer);
        ansiConsole.Write(renderable);
        ansiConsole.Profile.Out = originalOut;
        return writer.ToString().Trim();
    }

    #endregion
}