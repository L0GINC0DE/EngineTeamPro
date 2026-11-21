param([ValidateSet('Behavior','ImportSheet','Integration')][string]$Mode = 'Behavior')
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$validationPath = Join-Path $repoPath '.utmp/DLJ_SwordIdleIntegration/Validation'
$runtimePath = Join-Path $validationPath 'Assets/Runtime'
$editorPath = Join-Path $validationPath 'Assets/Editor'
$spritePath = Join-Path $validationPath 'Assets/02.Sprites/DLJ'
$scenePath = Join-Path $validationPath 'Assets/00.Scenes/DLJ'
New-Item -ItemType Directory -Force -Path $runtimePath,$editorPath,$spritePath,$scenePath,(Join-Path $validationPath 'Packages'),(Join-Path $validationPath 'ProjectSettings') | Out-Null
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/01.Scripts/DLJ/PlayerAppearance.cs'),(Join-Path $repoPath 'Assets/01.Scripts/DLJ/PlayerAppearance.cs.meta'),(Join-Path $repoPath 'Assets/01.Scripts/DLJ/UI/PlayableUISO.cs') -Destination $runtimePath -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'WeaponIdleValidation.cs') -Destination (Join-Path $editorPath 'DLJ_SwordIdleValidation.cs') -Force
foreach ($name in @('SwordAnimation.aseprite','SwordAnimation.aseprite.meta','PlayerBodyLibrary.asset','PlayerBodyLibrary.asset.meta')) {
    Copy-Item -LiteralPath (Join-Path $repoPath "Assets/02.Sprites/DLJ/$name") -Destination $spritePath -Force
}
$sheetPath = Join-Path $repoPath 'Assets/02.Sprites/DLJ/SwordIdle_DLJ_RedParticles.png'
if (Test-Path -LiteralPath $sheetPath) {
    Copy-Item -LiteralPath $sheetPath -Destination $spritePath -Force
    if (Test-Path -LiteralPath "$sheetPath.meta") { Copy-Item -LiteralPath "$sheetPath.meta" -Destination $spritePath -Force }
}
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/00.Scenes/DLJ/PlayerTest.unity') -Destination $scenePath -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $validationPath 'ProjectSettings') -Force
'{"dependencies":{"com.unity.2d.animation":"13.0.5","com.unity.2d.aseprite":"3.0.2"}}' | Set-Content -LiteralPath (Join-Path $validationPath 'Packages/manifest.json') -Encoding ascii
$logPath = Join-Path $validationPath "$Mode.log"
$arguments = @('-batchmode','-nographics','-quit','-projectPath',$validationPath,'-executeMethod',"DLJ_SwordIdleValidation.$Mode",'-logFile',$logPath)
$process = Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.20f1\Editor\Unity.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
Get-Content -LiteralPath $logPath | Select-String -Pattern 'DLJ_SWORD_IDLE_|Missing sword|error CS|Exception:'
if ($process.ExitCode -ne 0) { throw "Unity $Mode failed, exit $($process.ExitCode). See $logPath" }
if (!(Select-String -LiteralPath $logPath -Pattern 'DLJ_SWORD_IDLE_.*_PASS' -Quiet)) { throw "Validation marker missing. See $logPath" }
