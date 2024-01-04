$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$functionsFile = "$toolsDir/functions.ps1"
if (Test-Path $functionsFile) {
    Write-Warning "PowerShell functions file detected. Starting with version 0.2.0, the functions are no longer included. See https://timestamper.knoxy.ca/shell-setup for information on setting up your shell."
    Remove-Item $toolsDir/functions.ps1 -ErrorAction Stop -Force
}
