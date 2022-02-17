$myJson = Get-Content QuickLauncher\obj\gitversion.json -Raw | ConvertFrom-Json 
$mmp = $myJson.MajorMinorPatch
echo "$mmp"
7z a QuickLauncherSetup-"$mmp".zip QuickLauncherSetup\Release\*