# CI and local verification

## Local

Requires the .NET 8 SDK:

```bash
dotnet restore ZebraLegacy.sln
dotnet build ZebraLegacy.sln -c Release --no-restore
dotnet test ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration" --logger "console;verbosity=normal"
dotnet format ZebraLegacy.sln --verify-no-changes --no-restore
```

On Windows, enable the read-only integration test by specifying an explicit queue; it **does not send bytes**:

```powershell
$env:ZEBRA_TEST_PRINTER = 'ZDesigner GC420d'
dotnet test .\tests\ZebraLegacy.IntegrationTests -c Release --filter TestCategory=Integration
```

Without Windows or the variable, MSTest marks it inconclusive/skipped.

## Automation

`.github/workflows/ci.yml` builds and runs unit tests on Windows and macOS with .NET 8. It excludes the `Integration` category to avoid hardware and spooler dependencies. It does not publish artifacts or download Zebra drivers.
