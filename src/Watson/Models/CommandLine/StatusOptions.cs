using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Watson.Models.CommandLine;

[ExcludeFromCodeCoverage]
[Verb("status", HelpText = "Display the status of the current frame")]
public class StatusOptions
{
}