[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') {
    throw 'Este inventario requiere Windows.'
}

Write-Host '=== Colas Zebra/ZDesigner (solo lectura) ==='
$printers = Get-Printer | Where-Object {
    $_.Name -match 'Zebra|ZDesigner|GC420|2844' -or $_.DriverName -match 'Zebra|ZDesigner|GC420|2844'
}
$printers | Select-Object Name, DriverName, PortName, PrinterStatus, Shared | Format-Table -AutoSize

Write-Host "`n=== Drivers relacionados ==="
Get-PrinterDriver | Where-Object { $_.Name -match 'Zebra|ZDesigner|GC420|2844' } |
    Select-Object Name, Manufacturer, MajorVersion | Format-Table -AutoSize

Write-Host "`n=== Puertos referenciados ==="
$portNames = $printers.PortName | Sort-Object -Unique
Get-PrinterPort | Where-Object { $portNames -contains $_.Name } |
    Select-Object Name, Description, PrinterHostAddress, PortNumber | Format-Table -AutoSize

Write-Host "`nNo se ha modificado ninguna cola ni se ha enviado ningún trabajo."
