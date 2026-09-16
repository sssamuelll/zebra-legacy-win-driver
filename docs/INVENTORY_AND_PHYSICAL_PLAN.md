# Inventory and physical test plan

Do not submit jobs until this document has been completed and reviewed for each unit.

## 1. Unambiguous identification

Photograph/transcribe without sensitive data:

- Exact model on the rear/bottom nameplate: `GC420d`, `TLP 2844`, or `TLP 2844-Z`.
- Serial number (store it in the corporate inventory, not Git).
- Power-supply voltage and cable condition.
- Firmware shown on the configuration/self-test label.

**TLP 2844 ≠ TLP 2844-Z.** Do not infer the language from the enclosure or an existing queue. In this project:

- `Tlp2844`: EPL2 only.
- `Tlp2844Z`: EPL2 or ZPL, after confirming the nameplate/configuration.
- `Gc420d`: EPL2 or ZPL; prefer the language already standardized in the environment.

## 2. Connectors and port

Mark what is observed; do not connect two interfaces simultaneously:

- [ ] Direct USB-B, without a hub.
- [ ] DB9 serial: cable/pinout, COM port, baud, bits, parity, stop bits, and flow control.
- [ ] Centronics parallel: LPT port or exact adapter model.
- [ ] Internal Ethernet/external server: MAC, IP, DHCP/reservation, and protocol.

Correlate the physical connector with `Get-Printer ... PortName` and `Inventory-Zebra.ps1`. A name containing “USB” or “Zebra” does not prove that it is the correct unit.

## 3. Self-test without a host

1. Turn off the printer and remove jobs/the queue from the host.
2. Load the correct media and check sensors/printhead closure.
3. Follow the official model-specific manual to print the configuration label with the **FEED** button (the sequence varies). Do not improvise a sequence from another model.
4. Record the model/firmware, active language, dpi, sensors, and communication parameters.
5. Exit diagnostic mode according to the manual and restart if appropriate.

The self-test consumes one label but does not use the PC, helping separate hardware failures from queue failures.

## 4. Supervised smoke-test plan

- [ ] Inventory reviewed by two people or the equipment owner.
- [ ] Queue empty and not shared; test window approved.
- [ ] Model, language, width/height in dots, and media match.
- [ ] CLI `validate` returns 0.
- [ ] CLI `smoke` without `--send` shows `DRY-RUN`, length, and SHA-256.
- [ ] Operator is beside the printer and can turn it off/cancel the queue.
- [ ] Run exactly once with `--send`.
- [ ] Verify one label: complete text, correct orientation, no continuous feed.
- [ ] Record the result externally without committing serial numbers or payloads.

## 5. Stop criteria

Turn off/cancel and do not retry automatically if there is continuous feed, characters/commands printed as text, a blocked queue, an ambiguous model, incorrect size, abnormal temperature/noise, or more than one label. Review language, port, driver, dimensions, and calibration before another attempt.
