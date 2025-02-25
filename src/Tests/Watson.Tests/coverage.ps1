$dotCoverFilters = @(
    "+:module=Watson*",
    "-:module=Watson.Tests",
    "-:class=Watson.Models.CommandLine.*",
    "-:class=Watson.Models.DependencyResolver",
    "-:class=Watson.Extensions.ServicesCollectionExtensions",
    "-:class=Watson.Commands.Command*",
    "-:class=Watson.Cli",
    "-:class=*Program"
)
$dotnetTestFilters = @(
    "FullyQualifiedName~Watson.Tests"
)

$joinedDotCoverFilters = $dotCoverFilters -join ";"
$joinedDotnetTestFilters = $dotnetTestFilters -join ";"
dotnet dotcover cover-dotnet --ReportType=HTML --Output=./.coverage/report.html --HideAutoProperties --Filters="$joinedDotCoverFilters" -- test --no-build --filter "$joinedDotnetTestFilters"
Invoke-Item ./.coverage/report.html