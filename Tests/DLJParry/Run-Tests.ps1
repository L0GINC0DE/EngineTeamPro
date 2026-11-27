param(
    [string]$UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe',
    [string]$ValidationPath
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$ValidationPath) { $ValidationPath = Join-Path $repo '.utmp/DLJ_Parry' }
$runtime = Join-Path $ValidationPath 'Assets/Runtime'
$tests = Join-Path $ValidationPath 'Assets/Tests'
$packages = Join-Path $ValidationPath 'Packages'
$settings = Join-Path $ValidationPath 'ProjectSettings'
$plugins = Join-Path $ValidationPath 'Assets/Plugins'
New-Item -ItemType Directory -Force -Path $runtime, $tests, $packages, $settings, $plugins | Out-Null

$scripts = Join-Path $repo 'Assets/01.Scripts'
foreach ($pattern in @('DLJ/DLJ_Parry*.cs', 'KTH/Parry/*.cs', 'KTH/HitEffect/*.cs', 'TAE/Agents/HealthModule.cs',
        'KTH/Boss/KTH_Boss*.cs', 'KTH/Boss/KTH_Enemy.cs', 'KTH/Boss/Rest/KTH_Throw.cs')) {
    Copy-Item -Path (Join-Path $scripts $pattern) -Destination $runtime -Force
}
Copy-Item -LiteralPath (Join-Path $repo 'Assets/Plugins/Demigiant/DOTween/DOTween.dll') -Destination $plugins -Force
Copy-Item -LiteralPath (Join-Path $repo 'Assets/Plugins/Demigiant/DOTween/Modules/DOTweenModuleSprite.cs') -Destination $runtime -Force
Copy-Item -LiteralPath (Join-Path $repo 'ProjectSettings/ProjectVersion.txt') -Destination $settings -Force
Copy-Item -Path (Join-Path $PSScriptRoot '*Tests.cs') -Destination $tests -Force
'{"name":"DLJParry.Runtime","references":["Unity.Cinemachine"]}' | Set-Content (Join-Path $runtime 'DLJParry.Runtime.asmdef')
'{"name":"DLJParry.Tests","references":["DLJParry.Runtime"],"optionalUnityReferences":["TestAssemblies"]}' | Set-Content (Join-Path $tests 'DLJParry.Tests.asmdef')
$packageDeps = @{
    'com.unity.cinemachine' = '3.1.7'
    'com.unity.test-framework' = '1.6.0'
    'com.unity.modules.physics2d' = '1.0.0'
}
$projectLock = (Get-Content (Join-Path $repo 'Packages/packages-lock.json') -Raw | ConvertFrom-Json -AsHashtable).dependencies
$packageQueue = [System.Collections.Generic.Queue[string]]::new()
$packageQueue.Enqueue('com.unity.cinemachine')
while ($packageQueue.Count -gt 0) {
    $package = $packageQueue.Dequeue()
    $locked = $projectLock[$package]
    if ($locked.source -ne 'registry' -or $packageDeps[$package] -like 'file:*') { continue }

    $cached = Get-ChildItem (Join-Path $repo 'Library/PackageCache') -Directory -Filter "$package@*" -ErrorAction SilentlyContinue |
        Where-Object { (Get-Content (Join-Path $_.FullName 'package.json') -Raw | ConvertFrom-Json).version -eq $locked.version } |
        Select-Object -First 1
    if (!$cached) { continue }

    $packageDeps[$package] = 'file:' + $cached.FullName.Replace('\', '/')
    foreach ($dependency in $locked.dependencies.Keys) { $packageQueue.Enqueue($dependency) }
}
@{ dependencies = $packageDeps } | ConvertTo-Json | Set-Content (Join-Path $packages 'manifest.json')

$result = Join-Path $ValidationPath ('results-' + [Guid]::NewGuid().ToString('N') + '.xml')
$log = Join-Path $ValidationPath 'editor.log'
$arguments = @('-batchmode', '-nographics', '-projectPath', "`"$ValidationPath`"", '-runTests',
    '-testPlatform', 'PlayMode', '-testResults', "`"$result`"", '-logFile', "`"$log`"")
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if (!(Test-Path -LiteralPath $result)) {
    Get-Content -LiteralPath $log -Tail 70
    throw "Unity did not produce test results (exit $($process.ExitCode))."
}
[xml]$report = Get-Content -LiteralPath $result
foreach ($failure in $report.SelectNodes('//test-case[@result="Failed"]')) {
    Write-Output "$($failure.fullname): $($failure.failure.message.InnerText)"
}
$run = $report.'test-run'
Write-Output "Result=$($run.result) Total=$($run.total) Passed=$($run.passed) Failed=$($run.failed)"
if ($process.ExitCode -ne 0 -or $run.result -ne 'Passed' -or [int]$run.total -lt 1) { throw "See $result" }
