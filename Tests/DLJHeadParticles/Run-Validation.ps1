param([string]$ValidationPath, [ValidateSet('Run','RepairMetadata')][string]$Mode = 'Run')
$ErrorActionPreference = 'Stop'
$repoPath = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
if (!$ValidationPath) { $ValidationPath = Join-Path $repoPath '.utmp/DLJ_BakedHeadParticles/Validation' }
foreach ($folder in @('Assets/Runtime','Assets/Editor','Assets/Sprites/Player','Assets/Baselines','Packages','ProjectSettings')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $ValidationPath $folder) | Out-Null
}
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/01.Scripts/DLJ/PlayerAppearance.cs'),(Join-Path $repoPath 'Assets/01.Scripts/DLJ/UI/PlayableUISO.cs') -Destination (Join-Path $ValidationPath 'Assets/Runtime') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'HeadParticlesValidation.cs') -Destination (Join-Path $ValidationPath 'Assets/Editor') -Force
Copy-Item -Path (Join-Path $repoPath 'Assets/02.Sprites/DLJ/Player/*.aseprite*'),(Join-Path $repoPath 'Assets/02.Sprites/DLJ/Player/PlayerBody_DLJ.controller*') -Destination (Join-Path $ValidationPath 'Assets/Sprites/Player') -Force
Copy-Item -Path (Join-Path $repoPath '.utmp/DLJ_BakedHeadParticles/originals/*.aseprite') -Destination (Join-Path $ValidationPath 'Assets/Baselines') -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'Assets/02.Sprites/DLJ/PlayerBodyLibrary.asset'),(Join-Path $repoPath 'Assets/02.Sprites/DLJ/PlayerBodyLibrary.asset.meta') -Destination (Join-Path $ValidationPath 'Assets/Sprites') -Force
Copy-Item -LiteralPath (Join-Path $repoPath 'ProjectSettings/ProjectVersion.txt') -Destination (Join-Path $ValidationPath 'ProjectSettings') -Force
'{"dependencies":{"com.unity.2d.animation":"13.0.5","com.unity.2d.aseprite":"3.0.2"}}' | Set-Content -LiteralPath (Join-Path $ValidationPath 'Packages/manifest.json') -Encoding ascii
$logPath = Join-Path $ValidationPath 'validation.log'
$arguments = @('-batchmode','-nographics','-quit','-projectPath',$ValidationPath,'-executeMethod',"HeadParticlesValidation.$Mode",'-logFile',$logPath)
$process = Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/6000.3.20f1/Editor/Unity.exe' -ArgumentList $arguments -WindowStyle Hidden -PassThru
$process.WaitForExit()
Get-Content -LiteralPath $logPath | Select-String -Pattern 'DLJ_BAKED_HEAD|DLJ_IMPORTED_PARTICLES|DLJ_METADATA|DLJ_SWORD_IDLE|error CS|Exception:'
if ($process.ExitCode -ne 0) { throw "Baked head particle validation failed ($($process.ExitCode)). See $logPath" }
$marker = if ($Mode -eq 'RepairMetadata') { 'DLJ_METADATA_REPAIR_PASS' } else { 'DLJ_BAKED_HEAD_PARTICLES_PASS' }
if (!(Select-String -LiteralPath $logPath -Pattern $marker -Quiet)) { throw "Validation marker missing. See $logPath" }
