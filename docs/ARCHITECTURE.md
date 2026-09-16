# Architecture

## Scope

This repository is a user-space compatibility layer for **Windows 10/11 x64**. It does not implement a driver, install services, touch the kernel, or sign code. The queue and driver are provided by ZDesigner v5, obtained from Zebra by the operator.

```text
profiles.json
     │
     ▼
ZebraLegacy.Cli ── validates / creates smoke label / dry-run by default
     │                         │
     │                         ▼
     │                 ZebraLegacy.Core
     │                 profiles + EPL2/ZPL
     │
     └── --send (Windows only, explicit action)
              ▼
       WindowsRawPrinter
       winspool.drv: OpenPrinter → StartDoc → StartPage → WritePrinter
              ▼
       ZDesigner v5 queue → configured port → printer
```

## Boundaries and safety

- `ZebraLegacy.Core` has no Win32 dependency and is tested on any OS with .NET 8.
- `ZebraLegacy.Cli` contains the `winspool.drv` adapter; it submits `DataType=RAW` so EPL2/ZPL is not transformed.
- The `smoke` command only calculates metadata and SHA-256 unless `--send` is added.
- The payload is not logged and `.prn` files are not written by default. Errors show the operation and Win32 code, not the content.
- Payloads are limited to 1 MiB, dimensions are bounded, ASCII text is sanitized, and profiles are validated.
- Physical model and language identification must precede any submission.

## Components

- `src/ZebraLegacy.Core`: model, validation, and deterministic generators.
- `src/ZebraLegacy.Cli`: JSON configuration, UX, and RAW spooler.
- `tests/ZebraLegacy.Core.Tests`: pure tests with no hardware.
- `tests/ZebraLegacy.IntegrationTests`: opens a queue read-only; inconclusive without Windows or `ZEBRA_TEST_PRINTER`; never prints.
- `scripts`: read-only inventory and environment checks.

## Error flow

The adapter confirms the page and document only after a complete write. On any failure it attempts to abort the job and always closes the handle. A failed operation produces `RawPrintException`; it does not retry automatically because repeating a job could duplicate labels. The operator decides whether to clear the queue or retry.
