[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string] $PrinterName
)

$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'Windows is required.' }

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
    throw 'The queue does not use a driver whose name contains ZDesigner.'
}
Write-Host 'Read-only check completed; nothing was printed.'
