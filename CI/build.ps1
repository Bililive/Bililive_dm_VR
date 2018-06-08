$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip {
    param([string]$zipfile, [string]$outpath)
    $absfile = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($zipfile)
    $abspath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($outpath)
    [System.IO.Compression.ZipFile]::ExtractToDirectory($absfile, $abspath)
}

$basepath = Get-Location
Set-Location .\desktop\Bililive_dm_VR\

$builderror = $false
nuget restore -verbosity quiet
if ($LastExitCode -ne 0) {
    $host.SetShouldExit($LastExitCode) 
    $builderror = $true
}
msbuild /v:m /p:Configuration=Release /l:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll"
if ($LastExitCode -ne 0) {
    $host.SetShouldExit($LastExitCode) 
    $builderror = $true
}

Set-Location $basepath
if ($builderror) {
    Exit
}

$resultpath = '.\build\'
$zipfilename = '.\rendererbuild.zip'
$wpfbuildpath = '.\desktop\Bililive_dm_VR\Bililive_dm_VR.Desktop\bin\Release\'

$proj = Invoke-RestMethod -Method Get -Uri "https://ci.appveyor.com/api/projects/genteure/bililive-dm-vr/build/$env:APPVEYOR_BUILD_VERSION"
Invoke-RestMethod -Method Get -Uri "https://ci.appveyor.com/api/buildjobs/$($proj.build.jobs[0].jobId)/artifacts/build.zip" -OutFile $zipfilename
if (!(Test-Path $zipfilename)) {
    throw "Artifact Download Failed"
}
Unzip $zipfilename $resultpath

@("Bililive_dm_VR.Desktop.exe", "BinarySerializer.dll", "Newtonsoft.Json.dll", "Xceed.Wpf.Toolkit.dll").ForEach( 
    {
        Copy-Item "$($wpfbuildpath)$($_)" $resultpath
    }
)

