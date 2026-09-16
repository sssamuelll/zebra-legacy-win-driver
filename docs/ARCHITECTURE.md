# Arquitectura

## Alcance

Este repositorio es una capa de compatibilidad de espacio de usuario para **Windows 10/11 x64**. No implementa un driver, no instala servicios, no toca el kernel y no firma código. La cola y el driver los proporciona ZDesigner v5, obtenido por el operador desde Zebra.

```text
profiles.json
     │
     ▼
ZebraLegacy.Cli ── valida / crea smoke label / dry-run por defecto
     │                         │
     │                         ▼
     │                 ZebraLegacy.Core
     │                 perfiles + EPL2/ZPL
     │
     └── --send (solo Windows, acción explícita)
              ▼
       WindowsRawPrinter
       winspool.drv: OpenPrinter → StartDoc → StartPage → WritePrinter
              ▼
       cola ZDesigner v5 → puerto configurado → impresora
```

## Límites y seguridad

- `ZebraLegacy.Core` no conoce Win32 y se prueba en cualquier SO con .NET 8.
- `ZebraLegacy.Cli` contiene el adaptador `winspool.drv`; envía `DataType=RAW` para no transformar EPL2/ZPL.
- El comando `smoke` solo calcula metadatos y SHA-256 salvo que se añada `--send`.
- No se registra el payload ni se escriben `.prn` por defecto. Los errores muestran operación y código Win32, no el contenido.
- Payload máximo de 1 MiB, dimensiones acotadas, texto ASCII neutralizado y perfiles validados.
- La detección del modelo físico y del lenguaje precede a cualquier envío.

## Componentes

- `src/ZebraLegacy.Core`: modelo, validación y generadores deterministas.
- `src/ZebraLegacy.Cli`: configuración JSON, UX y spooler RAW.
- `tests/ZebraLegacy.Core.Tests`: pruebas puras, sin hardware.
- `tests/ZebraLegacy.IntegrationTests`: apertura de cola en modo lectura; inconclusa sin Windows o `ZEBRA_TEST_PRINTER`; nunca imprime.
- `scripts`: inventario de solo lectura y comprobación del entorno.

## Flujo de errores

El adaptador confirma página y documento solo después de una escritura completa; ante cualquier fallo intenta abortar el trabajo y siempre cierra el handle. Una operación fallida produce `RawPrintException`; no reintenta automáticamente, porque repetir un trabajo podría duplicar etiquetas. El operador decide si limpia la cola o repite.
