$testProjectFilePath = "./src/Tests/Watson.Tests/Watson.Tests.csproj"
$outputFilePath = "./src/Tests/Watson.Tests/bin/Debug/net8.0/TestResults/coverage.xml"
$reportPath = "./src/Tests/Watson.Tests/bin/Debug/net8.0/TestResults/"
$reportFilePath = "./src/Tests/Watson.Tests/bin/Debug/net8.0/TestResults/index.html"

dotnet run --project $testProjectFilePath --coverage --coverage-output coverage.xml --coverage-output-format cobertura

reportgenerator -reports:$outputFilePath -targetdir:$reportPath -reporttypes:"Html"
Start-Process $reportFilePath

