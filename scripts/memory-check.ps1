dotnet build -c Release ./src
dotMemory.exe start ./src/Watson/bin/Release/net8.0/Watson.exe --save-to-dir=./scripts/.dotMemory/snapshots -- $args