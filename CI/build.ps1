$basepath = Get-Location
Set-Location .\desktop\Bililive_dm_VR\

nuget restore -verbosity quiet
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
msbuild /v:m /p:Configuration=Release /l:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"
if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }

Set-Location $basepath
