
clear
& "$(Split-Path $MyInvocation.MyCommand.Path)/_devTool_Logo.ps1"
Get-ChildItem . -Directory -Exclude ".vscode",".vs",".idea",".git","out" | Foreach-Object {
	$pn = $_.Name
	Write-Output "- Cleaning $pn..."
	Remove-Item "out/$pn.pdb"
}