$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

Write-Host "TimeStamper installation"
Write-Host "Now that you've installed the Chocolatey Package, you will need to setup your PowerShell environment. Do this by dotsourcing functions.ps1 from this directory."
Write-Host "Simply add the following to your PowerShell profile: ``. $toolsDir\functions.ps1``"
Write-Host "You will then need to alias anything you want over to TimeStamper: ``New-Alias command timestamper``"