# Zebra Legacy Win Driver

Capa de compatibilidad para **Zebra GC420d, TLP 2844 y TLP 2844-Z en Windows 10/11 x64**. Usa el driver oficial **ZDesigner v5** y el spooler de Windows; **no es un driver kernel, no está firmado y no sustituye soporte del fabricante**.

El repositorio no contiene, descarga ni redistribuye binarios Zebra.

## Qué incluye

- CLI .NET 8 sin dependencias de runtime externas.
- Perfiles JSON validados por modelo, lenguaje, dpi y tamaño.
- Smoke labels EPL2 y ZPL con salida ASCII controlada.
- Envío RAW explícito mediante `winspool.drv`; dry-run por defecto y errores sin exponer payloads.
- Núcleo portable separado de Win32.
- Unit tests sin hardware e integración de solo lectura omitida sin Windows/cola.
- Inventario PowerShell y plan físico para distinguir TLP 2844 de TLP 2844-Z.

## Límites importantes

- No instala ni empaqueta ZDesigner.
- No detecta el modelo físico de forma infalible: hay que leer placa y autodiagnóstico.
- No calibra, actualiza firmware ni administra colas.
- `--send` imprime realmente; no lo use antes de completar el plan supervisado.
- La licencia de este repositorio está pendiente; consulte `LICENSE`.

## Inicio rápido (sin imprimir)

Requiere .NET 8 SDK. En Windows PowerShell:

```powershell
dotnet restore .\ZebraLegacy.sln
dotnet build .\ZebraLegacy.sln -c Release --no-restore
dotnet test .\ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration"
Copy-Item .\profiles.example.json .\profiles.local.json

dotnet run --project .\src\ZebraLegacy.Cli -- validate --config .\profiles.local.json
dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile almacen-gc420d
```

La última orden genera el payload en memoria y solo muestra lenguaje, longitud y SHA-256:

```text
DRY-RUN: no se ha enviado nada. Añada --send solo tras completar el plan físico.
```

Para obtener un smoke test EPL2 use un perfil `Epl2`; para ZPL use `Zpl`. `Tlp2844` sin sufijo Z rechaza ZPL deliberadamente. Solo tras aprobación física, Windows y una cola dedicada:

```powershell
# ACCIÓN REAL: envía un único trabajo RAW
# dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile almacen-gc420d --send
```

La línea permanece comentada para impedir ejecución accidental al copiar el bloque.

## Instalación del driver oficial

Consulte [`docs/INSTALLATION.md`](docs/INSTALLATION.md). En resumen: busque el **modelo exacto** en <https://www.zebra.com/us/en/support-downloads.html>, compruebe que ZDesigner v5 declara compatibilidad x64 con su versión de Windows y acepte la licencia directamente con Zebra. No use mirrors. Este proyecto no descarga nada.

## Flujo operativo

1. Ejecute `scripts/Inventory-Zebra.ps1` (solo lectura).
2. Complete [`docs/INVENTORY_AND_PHYSICAL_PLAN.md`](docs/INVENTORY_AND_PHYSICAL_PLAN.md).
3. Instale/configure ZDesigner v5 desde fuentes oficiales.
4. Cree `profiles.local.json` (ignorado por Git) a partir del ejemplo.
5. Ejecute `validate` y `smoke` sin `--send`.
6. Una persona autorizada revisa modelo, lenguaje, puerto, consumible y cola.
7. Solo entonces ejecute una vez con `--send` y observe físicamente.

## Configuración

```json
{
  "profiles": [
    {
      "name": "almacen-gc420d",
      "printerName": "ZDesigner GC420d",
      "model": "Gc420d",
      "language": "Zpl",
      "dpi": 203,
      "widthDots": 812,
      "heightDots": 406
    }
  ]
}
```

Modelos: `Gc420d`, `Tlp2844`, `Tlp2844Z`. Lenguajes: `Epl2`, `Zpl`. El nombre de cola debe contener `ZDesigner`. Los modelos cubiertos usan perfiles de 203 dpi.

## Diseño y decisión técnica

- [Arquitectura y controles](docs/ARCHITECTURE.md)
- [ADR-0001: reutilizar ZDesigner y spooler](docs/adr/0001-reutilizar-zdesigner-spooler.md)

El preflight comparó brevemente ZDesigner v5, Zebra Setup Utilities, SDKs/librerías y un driver propio. El driver oficial + spooler RAW resulta la opción mantenible con menor privilegio y sin duplicar una solución existente.

## Desarrollo y CI

Consulte [`docs/CI.md`](docs/CI.md). La CI compila en Windows/macOS y excluye integración. Las pruebas de integración solo abren una cola indicada por `ZEBRA_TEST_PRINTER`; nunca llaman a `WritePrinter`.

## Seguridad y diagnóstico

Los payloads no se imprimen en consola ni se guardan por defecto. El adaptador limita el tamaño, cierra handles en `finally` y no reintenta para evitar duplicados. Ante error, registre código Win32, estado de cola y resultado físico, pero no seriales ni datos sensibles en Git.
