# Zebra Legacy Win Driver

Compatibility layer for **Zebra GC420d, TLP 2844, and TLP 2844-Z on Windows 10/11 x64**. It uses the official **ZDesigner v5** driver and the Windows spooler; **it is not a kernel driver, is not signed, and does not replace manufacturer support**.

This repository does not contain, download, or redistribute Zebra binaries.

## What is included

- .NET 8 CLI with no external runtime dependencies.
- JSON profiles validated by model, language, dpi, and size.
- EPL2 and ZPL smoke labels with controlled ASCII output.
- Explicit RAW submission through `winspool.drv`; dry-run by default and errors that do not expose payloads.
- Portable core separated from Win32.
- Hardware-free unit tests and a read-only integration test skipped without Windows/a printer queue.
- PowerShell inventory and a physical plan to distinguish TLP 2844 from TLP 2844-Z.

## Important limitations

- Does not install or package ZDesigner.
- Cannot identify the physical model infallibly: inspect the nameplate and self-test.
- Does not calibrate, update firmware, or manage queues.
- `--send` really prints; do not use it before completing the supervised plan.
- This repository's license is pending; see `LICENSE`.

## Quick start (without printing)

Requires the .NET 8 SDK. In Windows PowerShell:

```powershell
dotnet restore .\ZebraLegacy.sln
dotnet build .\ZebraLegacy.sln -c Release --no-restore
dotnet test .\ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration"
Copy-Item .\profiles.example.json .\profiles.local.json

dotnet run --project .\src\ZebraLegacy.Cli -- validate --config .\profiles.local.json
dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile warehouse-gc420d
```

The last command generates the payload in memory and displays only its language, length, and SHA-256:

```text
DRY-RUN: nothing was sent. Add --send only after completing the physical plan.
```

For an EPL2 smoke test, use an `Epl2` profile; for ZPL, use `Zpl`. `Tlp2844` without the Z suffix deliberately rejects ZPL. Only after physical approval, on Windows, with a dedicated queue:

```powershell
# REAL ACTION: submits one RAW job
# dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile warehouse-gc420d --send
```

The line remains commented out to prevent accidental execution when the block is copied.

## Installing the official driver

See [`docs/INSTALLATION.md`](docs/INSTALLATION.md). In short: find the **exact model** at <https://www.zebra.com/us/en/support-downloads.html>, verify that ZDesigner v5 declares x64 compatibility with your Windows version, and accept the license directly from Zebra. Do not use mirrors. This project downloads nothing.

## Operating workflow

1. Run `scripts/Inventory-Zebra.ps1` (read-only).
2. Complete [`docs/INVENTORY_AND_PHYSICAL_PLAN.md`](docs/INVENTORY_AND_PHYSICAL_PLAN.md).
3. Install/configure ZDesigner v5 from official sources.
4. Create `profiles.local.json` (ignored by Git) from the example.
5. Run `validate` and `smoke` without `--send`.
6. Have an authorized person review the model, language, port, media, and queue.
7. Only then run once with `--send` and observe the printer physically.

## Configuration

```json
{
  "profiles": [
    {
      "name": "warehouse-gc420d",
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

Models: `Gc420d`, `Tlp2844`, `Tlp2844Z`. Languages: `Epl2`, `Zpl`. The queue name must contain `ZDesigner`. The covered models use 203 dpi profiles.

## Design and technical decision

- [Architecture and controls](docs/ARCHITECTURE.md)
- [ADR-0001: reuse ZDesigner and the spooler](docs/adr/0001-reuse-zdesigner-spooler.md)

The preflight briefly compared ZDesigner v5, Zebra Setup Utilities, SDKs/libraries, and a custom driver. The official driver plus RAW spooler is the maintainable, least-privilege option that avoids duplicating an existing solution.

## Development and CI

See [`docs/CI.md`](docs/CI.md). CI builds on Windows/macOS and excludes integration tests. Integration tests only open a queue specified by `ZEBRA_TEST_PRINTER`; they never call `WritePrinter`.

## Security and diagnostics

Payloads are not printed to the console or saved by default. The adapter limits size, closes handles in `finally`, and does not retry, to avoid duplicates. On error, record the Win32 code, queue status, and physical result, but do not commit serial numbers or sensitive data to Git.
