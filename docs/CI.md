# CI y verificación local

## Local

Requiere SDK .NET 8:

```bash
dotnet restore ZebraLegacy.sln
dotnet build ZebraLegacy.sln -c Release --no-restore
dotnet test ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration" --logger "console;verbosity=normal"
dotnet format ZebraLegacy.sln --verify-no-changes --no-restore
```

En Windows, la prueba de integración de solo lectura se habilita definiendo una cola explícita; **no envía bytes**:

```powershell
$env:ZEBRA_TEST_PRINTER = 'ZDesigner GC420d'
dotnet test .\tests\ZebraLegacy.IntegrationTests -c Release --filter TestCategory=Integration
```

Sin Windows o sin variable, MSTest la marca inconclusa/omitida.

## Automatización

`.github/workflows/ci.yml` compila y ejecuta unit tests en Windows y macOS con .NET 8. Excluye la categoría `Integration` para no depender de hardware ni spooler. No publica artefactos y no descarga drivers Zebra.
