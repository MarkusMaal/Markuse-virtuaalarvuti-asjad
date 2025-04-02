
clear
& "$(Split-Path $MyInvocation.MyCommand.Path)/_devTool_Logo.ps1"
Get-ChildItem . -Directory -Exclude ".vscode",".vs",".idea",".git","out" | Foreach-Object {
	$pn = $_.Name
	Write-Output "- Compiling $pn..."
	dotnet publish $pn -c Release -o out -p:PublishReadyToRun=true -p:PublishSingleFile=true -p:PublishTrimmed=true --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true
}