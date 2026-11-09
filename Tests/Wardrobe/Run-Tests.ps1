param([string]$ValidationPath = (Join-Path $env:TEMP 'EngineTeamPro-Wardrobe-Validation'))
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$unityPath = (Get-Content -LiteralPath (Join-Path $repoPath 'Library/EditorInstance.json') | ConvertFrom-Json).app_path
$runtimePath = Join-Path $ValidationPath 'Assets/Runtime'
$testPath = Join-Path $ValidationPath 'Assets/Tests'
New-Item -ItemType Directory -Force -Path $runtimePath, $testPath, (Join-Path $ValidationPath 'Packages'), (Join-Path $ValidationPath 'ProjectSettings') | Out-Null
$sources = @('TAE/Data/UIData.cs', 'TAE/Data/PlayerData.cs', 'TAE/PlatformPlayer.cs',
    'TAE/PlayerInputReader.cs', 'TAE/AgentMovement.cs', 'TAE/Interactor.cs', 'TAE/Interactable.cs',
    'TAE/Test/InteractTest.cs', 'TAE/UIManager.cs', 'TAE/Weapon.cs', 'TAE/Modules/HealthModule.cs',
    'TAE/CoreLib/MonoSingleton.cs', 'TAE/PlayerAttack.cs', 'TAE/Managers/TurnManager.cs',
    'TAE/BattleUnit.cs', 'TAE/Attacker.cs', 'TAE/ICommand.cs', 'TAE/AttackCommand.cs')
foreach ($source in $sources) {
    Copy-Item -LiteralPath (Join-Path $repoPath "Assets/01.Scripts/$source") -Destination $runtimePath -Force
}
Copy-Item -Path (Join-Path $repoPath 'Assets/01.Scripts/KTH/HitEffect/*.cs') -Destination $runtimePath -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/InputSystem_Actions.cs') -Destination $runtimePath -Force
Copy-Item -Path (Join-Path $repoPath 'Assets/Plugins/Demigiant/DOTween/*.dll') -Destination $runtimePath -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $ValidationPath 'ProjectSettings') -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectSettings.asset') -Destination (Join-Path $ValidationPath 'ProjectSettings') -Force
Copy-Item -Path (Join-Path $PSScriptRoot '*Tests.cs') -Destination $testPath -Force
'{"name":"Wardrobe.Runtime","references":["Unity.InputSystem","Unity.ugui","Unity.Cinemachine"]}' | Set-Content -LiteralPath (Join-Path $runtimePath 'Wardrobe.Runtime.asmdef') -Encoding ascii
'{"name":"Wardrobe.Tests","references":["Wardrobe.Runtime","Unity.InputSystem","Unity.ugui"],"optionalUnityReferences":["TestAssemblies"]}' | Set-Content -LiteralPath (Join-Path $testPath 'Wardrobe.Tests.asmdef') -Encoding ascii
'{"dependencies":{"com.unity.test-framework":"1.6.0","com.unity.inputsystem":"1.19.0","com.unity.ugui":"2.0.0","com.unity.cinemachine":"3.1.7","com.unity.modules.physics2d":"1.0.0"}}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Packages/manifest.json') -Encoding ascii
$resultPath = Join-Path $ValidationPath 'results.xml'
$logPath = Join-Path $ValidationPath 'editor.log'
if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
$arguments = @('-batchmode', '-nographics', '-projectPath', "`"$ValidationPath`"", '-runTests', '-testPlatform', 'PlayMode', '-testResults', "`"$resultPath`"", '-logFile', "`"$logPath`"")
$process = Start-Process -FilePath $unityPath -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if (!(Test-Path -LiteralPath $resultPath)) {
    Get-Content -LiteralPath $logPath -Tail 65
    throw "Unity did not produce test results (exit $($process.ExitCode))."
}
[xml]$results = Get-Content -LiteralPath $resultPath
$summary = $results.'test-run'
Write-Output "Result=$($summary.result) Total=$($summary.total) Passed=$($summary.passed) Failed=$($summary.failed)"
foreach ($failure in $results.SelectNodes('//test-case[@result="Failed"]')) { Write-Output "$($failure.fullname): $($failure.failure.message.InnerText)" }
if ($process.ExitCode -ne 0 -or $summary.failed -ne '0' -or $summary.total -eq '0') { throw 'Wardrobe tests failed.' }
