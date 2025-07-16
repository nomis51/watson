using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Watson.Models.CommandLine;

[ExcludeFromCodeCoverage]
[Verb("cancel", HelpText = "Cancel the currently running task")]
public class CancelOptions
{
}