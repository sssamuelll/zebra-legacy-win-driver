# Installation on Windows 10/11 x64

> This repository **does not download or include** drivers. Use official sources only and comply with the Zebra license.

## 1. Obtain the official software

1. Open the official support portal: <https://www.zebra.com/us/en/support-downloads.html>.
2. Search for the exact model on the nameplate: **GC420d**, **TLP 2844**, or **TLP 2844-Z**.
3. In Drivers, locate **ZDesigner v5** compatible with Windows x64. Verify publisher/digital signature, version, declared OS, and license terms.
4. If the portal no longer offers a compatible package, stop and escalate to Zebra or the security owner; do not use mirrors or repackaged bundles.

The download is not automated, to avoid redistribution, stale URLs, and implicit license acceptance.

## 2. Before installation

- Complete `docs/INVENTORY_AND_PHYSICAL_PLAN.md`.
- Leave USB disconnected until the installer requests it.
- Create a restore point or follow the corporate procedure.
- Verify that no same-named queue is in production use.

## 3. Install and create the queue

Run the official installer as administrator and follow its wizard. Select the **exact model** and the inventoried port:

- USB: the `USB00x` port created for the correct device.
- Serial: the correct COM port with matching host/printer parameters.
- Parallel: the validated LPT port/adapter.
- Network: Standard TCP/IP with a reserved IP; document RAW 9100 or another authorized protocol.

Assign a name containing `ZDesigner`, because validation prevents accidental use of generic queues. Under Printer Properties → Advanced, confirm the expected ZDesigner driver and a RAW processor/data type. **Do not click “Print Test Page” yet.**

## 4. Install the CLI

Install the .NET 8 SDK from <https://dotnet.microsoft.com/download/dotnet/8.0> and run:

```powershell
dotnet restore .\ZebraLegacy.sln
dotnet build .\ZebraLegacy.sln -c Release --no-restore
dotnet test .\ZebraLegacy.sln -c Release --no-build --filter "TestCategory!=Integration"
Copy-Item .\profiles.example.json .\profiles.local.json
```

Edit `profiles.local.json` with the verified queue and model. Validate and dry-run first:

```powershell
dotnet run --project .\src\ZebraLegacy.Cli -- validate --config .\profiles.local.json
dotnet run --project .\src\ZebraLegacy.Cli -- smoke --config .\profiles.local.json --profile warehouse-gc420d
```

Reserve `--send` for the approved step in the physical plan.

## 5. Uninstallation

Delete the CLI like ordinary files. Removing the queue/driver is a separate administrative action: check dependencies from other queues and follow the corporate procedure. Do not blindly delete the package from the driver store.
