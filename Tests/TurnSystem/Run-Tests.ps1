param(
    [string]$UnityPath,
    [string]$ValidationPath = (Join-Path $env:TEMP 'EngineTeamPro-TurnSystem-Composition')
)

$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$UnityPath) {
    $instance = Get-Content -LiteralPath (Join-Path $repoPath 'Library/EditorInstance.json') | ConvertFrom-Json
    $UnityPath = $instance.app_path
}

$runtimePath = Join-Path $ValidationPath 'Assets/Runtime'
$testPath = Join-Path $ValidationPath 'Assets/Tests'
New-Item -ItemType Directory -Force -Path $runtimePath, $testPath,
    (Join-Path $ValidationPath 'Packages'), (Join-Path $ValidationPath 'ProjectSettings') | Out-Null

# Use real Unity components in a separate project so the open working scene stays intact.
$sources = @(
    'TAE/Modules/HealthModule.cs', 'TAE/CoreLib/MonoSingleton.cs',
    'TAE/Managers/TurnManager.cs',
    'TAE/BattleUnit.cs', 'TAE/Attacker.cs', 'TAE/ICommand.cs', 'TAE/AttackCommand.cs',
    'TAE/PlayerAttack.cs', 'DLJ/AttackUI.cs', 'DLJ/PlayableUI.cs', 'DLJ/PlayableUISO.cs',
    'TAE/Test/TestTurnSystem.cs'
)
foreach ($source in $sources) {
    $sourcePath = Join-Path $repoPath "Assets/01.Scripts/$source"
    if (Test-Path -LiteralPath $sourcePath) {
        Copy-Item -LiteralPath $sourcePath -Destination $runtimePath
        if (Test-Path -LiteralPath "$sourcePath.meta") {
            Copy-Item -LiteralPath "$sourcePath.meta" -Destination $runtimePath
        }
    }
}
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $ValidationPath 'ProjectSettings')
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectSettings.asset') -Destination (Join-Path $ValidationPath 'ProjectSettings')
Copy-Item -Path (Join-Path $PSScriptRoot '*Tests.cs') -Destination $testPath
$resourcePath = Join-Path $ValidationPath 'Assets/Resources'
$scenePath = Join-Path $ValidationPath 'Assets/Scenes'
New-Item -ItemType Directory -Force -Path $resourcePath, $scenePath | Out-Null
foreach ($prefab in @('Player', 'Test', 'TurnManager')) {
    Copy-Item -LiteralPath (Join-Path $repoPath "Assets/03.Prefabs/TAE/$prefab.prefab") -Destination $resourcePath
    Copy-Item -LiteralPath (Join-Path $repoPath "Assets/03.Prefabs/TAE/$prefab.prefab.meta") -Destination $resourcePath
}
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/00.Scenes/TAE/TestScene.unity') -Destination $scenePath
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/00.Scenes/TAE/TestScene.unity.meta') -Destination $scenePath
'{"name":"TurnSystem.Runtime","references":["Unity.InputSystem"]}' | Set-Content -LiteralPath (Join-Path $runtimePath 'TurnSystem.Runtime.asmdef') -Encoding utf8
'{"name":"TurnSystem.Tests","references":["TurnSystem.Runtime","Unity.InputSystem"],"optionalUnityReferences":["TestAssemblies"]}' |
    Set-Content -LiteralPath (Join-Path $testPath 'TurnSystem.Tests.asmdef') -Encoding utf8
'{"dependencies":{"com.unity.test-framework":"1.6.0","com.unity.inputsystem":"1.19.0","com.unity.ugui":"2.0.0"}}' |
    Set-Content -LiteralPath (Join-Path $ValidationPath 'Packages/manifest.json') -Encoding utf8

$resultPath = Join-Path $ValidationPath 'results.xml'
$logPath = Join-Path $ValidationPath 'editor.log'
if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
$arguments = @('-batchmode', '-nographics', '-projectPath', "`"$ValidationPath`"",
    '-runTests', '-testPlatform', 'PlayMode', '-testResults', "`"$resultPath`"", '-logFile', "`"$logPath`"")
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if (!(Test-Path -LiteralPath $resultPath)) {
    Get-Content -LiteralPath $logPath -Tail 100
    throw "Unity did not produce test results (exit $($process.ExitCode)). Log: $logPath"
}
[xml]$results = Get-Content -LiteralPath $resultPath
$summary = $results.'test-run'
Write-Output "Result=$($summary.result) Total=$($summary.total) Passed=$($summary.passed) Failed=$($summary.failed)"
Write-Output "Results: $resultPath"
Write-Output "Log: $logPath"
foreach ($failure in $results.SelectNodes('//test-case[@result="Failed"]')) {
    Write-Output "$($failure.fullname): $($failure.failure.message.InnerText)"
}
if ($process.ExitCode -ne 0 -or $summary.failed -ne '0' -or $summary.total -eq '0') {
    throw 'Turn system tests failed.'
}
