using System.Text;
using Spectre.Console.Rendering;

namespace Watson.Helpers.Abstractions;

public interface IConsoleAdapter
{
    void WriteLine(string text);
    void WriteLine(string text, params object[] args);
    void WriteLine();
    void MarkupLine(string text);
    void MarkupLine(string text, params object[] args);
    void Write(IRenderable renderable);
    void Write(string text);
    void SetEncoding(Encoding encoding);
}