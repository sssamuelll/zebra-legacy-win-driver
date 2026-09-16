# ADR-0001: Reuse ZDesigner v5 and the Windows spooler

- Status: accepted
- Date: 2026-09-16

## Context

GC420d and TLP 2844 are legacy printers. The goal is to maintain a controllable printing path on Windows 10/11 x64 without developing or distributing a driver. A kernel driver requires specialized expertise, signing, security maintenance, and a disproportionate compatibility matrix.

## Alternatives preflight

Existing options were briefly reviewed:

1. **Official ZDesigner v5 + Windows spooler:** the manufacturer's solution for these generations; manages queue, port, and OS compatibility.
2. **Zebra Setup Utilities:** useful for installation/diagnostics, but does not replace an automatable, testable project interface.
3. **SDKs/Browser Print or generic libraries:** add dependencies and do not solve legacy-driver installation; for a small EPL2/ZPL payload, the spooler's RAW API is sufficient.
4. **Custom driver, CUPS, or kernel redesign:** high cost and risk, with no advantage for the requested Windows scope.

Availability and licensing of the exact package must be verified on Zebra's official portal at deployment time; the project neither pins nor copies external binaries.

## Decision

Reuse the official **ZDesigner v5 driver**, installed by an administrator, and the Windows spooler. Generate EPL2/ZPL in a portable core and submit RAW bytes only on explicit request. Do not download, package, modify, or redistribute Zebra binaries.

## Consequences

- Smaller privileged-code surface and lower maintenance burden.
- Final installation and compatibility depend on Zebra and Windows.
- The queue must use the correct model, port, and RAW processor.
- Each unit requires inventory and a supervised physical test.
- No manufacturer support or signing is promised for this project.
