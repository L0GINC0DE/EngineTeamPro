param([string]$ValidationPath)
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$ValidationPath) { $ValidationPath = Join-Path $repoPath '.utmp/DLJ_EquippedUI/Validation' }
foreach ($folder in @('Assets/Runtime','Assets/Tests','Assets/Plugins','Packages','ProjectSettings')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $ValidationPath $folder) | Out-Null
}
$sources = @('DLJ','TAE/Agents/HealthModule.cs','TAE/CoreLib/MonoSingleton.cs',
    'TAE/BattleUnit.cs','TAE/Attacker.cs','TAE/Interface/ICommand.cs','TAE/AttackCommand.cs',
    'TAE/Players/PlayerAttack.cs','TAE/Managers/TurnManager.cs','KTH/Parry/KTH_DefenseStats.cs','KTH/HitEffect')
foreach ($source in $sources) {
    $sourcePath = Join-Path $repoPath "Assets/01.Scripts/$source"
    if (Test-Path -LiteralPath $sourcePath -PathType Container) {
        Get-ChildItem -LiteralPath $sourcePath -Filter '*.cs' -Recurse | ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $ValidationPath 'Assets/Runtime') -Force
        }
    } else { Copy-Item -LiteralPath $sourcePath -Destination (Join-Path $ValidationPath 'Assets/Runtime') -Force }
}
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/Plugins/Demigiant/DOTween/DOTween.dll') -Destination (Join-Path $ValidationPath 'Assets/Plugins') -Force
Copy-Item -Path (Join-Path $repoPath 'Assets/Plugins/Demigiant/DOTween/Modules/*.cs') -Destination (Join-Path $ValidationPath 'Assets/Runtime') -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $ValidationPath 'ProjectSettings') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'EquippedUITests.cs') -Destination (Join-Path $ValidationPath 'Assets/Tests') -Force
'{"name":"DLJEquipped.Runtime","references":["Unity.2D.Animation.Runtime","Unity.ugui","Unity.TextMeshPro","Unity.Cinemachine"]}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Assets/Runtime/DLJEquipped.Runtime.asmdef') -Encoding ascii
'{"name":"DLJEquipped.Tests","references":["DLJEquipped.Runtime","Unity.ugui","Unity.2D.Animation.Runtime"],"optionalUnityReferences":["TestAssemblies"]}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Assets/Tests/DLJEquipped.Tests.asmdef') -Encoding ascii
'{"dependencies":{"com.unity.2d.animation":"13.0.5","com.unity.cinemachine":"3.1.7","com.unity.ugui":"2.0.0","com.unity.test-framework":"1.6.0","com.unity.modules.physics2d":"1.0.0","com.unity.modules.audio":"1.0.0"}}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Packages/manifest.json') -Encoding ascii
$resultPath = Join-Path $ValidationPath 'results.xml'
$logPath = Join-Path $ValidationPath 'editor.log'
if (Test-Path -LiteralPath $resultPath) { Remove-Item -LiteralPath $resultPath }
$arguments = @('-batchmode','-nographics','-projectPath',"`"$ValidationPath`"",'-runTests','-testPlatform','PlayMode','-testResults',"`"$resultPath`"",'-logFile',"`"$logPath`"")
$process = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
if (!(Test-Path -LiteralPath $resultPath)) {
    Get-Content -LiteralPath $logPath -Tail 70
    throw "Unity did not produce results (exit $($process.ExitCode))"
}
[xml]$results = Get-Content -LiteralPath $resultPath
$summary = $results.'test-run'
Write-Output "Result=$($summary.result) Total=$($summary.total) Passed=$($summary.passed) Failed=$($summary.failed)"
foreach ($failure in $results.SelectNodes('//test-case[@result="Failed"]')) {
    Write-Output "$($failure.fullname): $($failure.failure.message.InnerText)"
}
if ($process.ExitCode -ne 0 -or $summary.failed -ne '0' -or $summary.total -eq '0') { throw "Equipped UI tests failed. See $resultPath" }
