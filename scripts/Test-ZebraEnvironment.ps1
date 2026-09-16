[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PrinterName
)

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Requiere Windows.' }

$printer = Get-Printer -Name $PrinterName -ErrorAction Stop
$driver = Get-PrinterDriver -Name $printer.DriverName -ErrorAction Stop
$port = Get-PrinterPort -Name $printer.PortName -ErrorAction Stop

[pscustomobject]@{
    PrinterName = $printer.Name
    DriverName  = $driver.Name
    PortName    = $port.Name
    Status      = $printer.PrinterStatus
    IsZDesigner = $driver.Name -match 'ZDesigner'
    Shared      = $printer.Shared
} | Format-List

if ($driver.Name -notmatch 'ZDesigner') {
    throw 'La cola no usa un driver cuyo nombre contenga ZDesigner.'
}
Write-Host 'Comprobación de solo lectura completada; no se imprimió nada.'
