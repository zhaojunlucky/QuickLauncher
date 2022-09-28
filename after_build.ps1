$myJson = Get-Content QuickLauncher\obj\gitversion.json -Raw | ConvertFrom-Json 
$mmp = $myJson.AssemblySemFileVer
echo "$mmp"
7z a QuickLauncherSetup-"$mmp".zip QuickLauncherInstaller\Output\*