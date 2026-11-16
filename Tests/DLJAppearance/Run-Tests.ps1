param([string]$ValidationPath)
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$ValidationPath) { $ValidationPath = Join-Path $repoPath 'Temp/DLJ-Appearance-Validation' }
$unityPath = (Get-Content -LiteralPath (Join-Path $repoPath 'Library/EditorInstance.json') | ConvertFrom-Json).app_path
$runtimePath = Join-Path $ValidationPath 'Assets/Runtime'
$testPath = Join-Path $ValidationPath 'Assets/Tests'
$spritePath = Join-Path $ValidationPath 'Assets/Sprites'
New-Item -ItemType Directory -Force -Path $runtimePath, $testPath, $spritePath, (Join-Path $ValidationPath 'Packages'), (Join-Path $ValidationPath 'ProjectSettings') | Out-Null
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/01.Scripts/DLJ/UI/PlayableUISO.cs') -Destination $runtimePath -Force
$appearancePath = Join-Path $repoPath 'Assets/01.Scripts/DLJ/DLJ_PlayerAppearance.cs'
if (Test-Path -LiteralPath $appearancePath) { Copy-Item -LiteralPath $appearancePath -Destination $runtimePath -Force }
Copy-Item -Path (Join-Path $repoPath 'Assets/02.Sprites/DLJ/Player_0*_DLJ_Idle.aseprite*') -Destination $spritePath -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PlayerAppearanceTests.cs') -Destination $testPath -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $ValidationPath 'ProjectSettings') -Force
'{"name":"Appearance.Runtime","references":["Unity.2D.Animation.Runtime"]}' | Set-Content -LiteralPath (Join-Path $runtimePath 'Appearance.Runtime.asmdef') -Encoding ascii
'{"name":"Appearance.Tests","references":["Appearance.Runtime","Unity.2D.Animation.Runtime"],"includePlatforms":["Editor"],"optionalUnityReferences":["TestAssemblies"]}' | Set-Content -LiteralPath (Join-Path $testPath 'Appearance.Tests.asmdef') -Encoding ascii
'{"dependencies":{"com.unity.test-framework":"1.6.0","com.unity.2d.animation":"13.0.5","com.unity.2d.aseprite":"3.0.2"}}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Packages/manifest.json') -Encoding ascii
$resultPath = Join-Path $ValidationPath 'results.xml'
$logPath = Join-Path $ValidationPath 'editor.log'
if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
$arguments = @('-batchmode', '-nographics', '-projectPath', "`"$ValidationPath`"", '-runTests', '-testPlatform', 'EditMode', '-testResults', "`"$resultPath`"", '-logFile', "`"$logPath`"")
$process = Start-Process -FilePath $unityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if (!(Test-Path -LiteralPath $resultPath)) {
    Get-Content -LiteralPath $logPath -Tail 65
    throw "Unity did not produce test results (exit $($process.ExitCode))."
}
[xml]$results = Get-Content -LiteralPath $resultPath
$summary = $results.'test-run'
Write-Output "Result=$($summary.result) Total=$($summary.total) Passed=$($summary.passed) Failed=$($summary.failed)"
foreach ($failure in $results.SelectNodes('//test-case[@result="Failed"]')) { Write-Output "$($failure.fullname): $($failure.failure.message.InnerText)" }
if ($process.ExitCode -ne 0 -or $summary.failed -ne '0' -or $summary.total -eq '0') { throw 'Player appearance tests failed.' }
