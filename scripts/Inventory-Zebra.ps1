[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') {
    throw 'This inventory requires Windows.'
}

Write-Host '=== Zebra/ZDesigner queues (read-only) ==='
$printers = Get-Printer | Where-Object {
    $_.Name -match 'Zebra|ZDesigner|GC420|2844' -or $_.DriverName -match 'Zebra|ZDesigner|GC420|2844'
}
$printers | Select-Object Name, DriverName, PortName, PrinterStatus, Shared | Format-Table -AutoSize

Write-Host "`n=== Related drivers ==="
Get-PrinterDriver | Where-Object { $_.Name -match 'Zebra|ZDesigner|GC420|2844' } |
    Select-Object Name, Manufacturer, MajorVersion | Format-Table -AutoSize

Write-Host "`n=== Referenced ports ==="
$portNames = $printers.PortName | Sort-Object -Unique
Get-PrinterPort | Where-Object { $portNames -contains $_.Name } |
    Select-Object Name, Description, PrinterHostAddress, PortNumber | Format-Table -AutoSize

Write-Host "`nNo queues were modified and no jobs were submitted."
