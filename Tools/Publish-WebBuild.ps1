# Run from PowerShell in the project root AFTER testing and committing the release.
# Creates/updates only the pages-build branch; main is never force-pushed.
[CmdletBinding()]
param(
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
function Invoke-ProjectGit([string[]]$Arguments) {
    $result = & git @Arguments
    if ($LASTEXITCODE -ne 0) { throw "git failed: $($Arguments -join ' ')" }
    return $result
}
Push-Location $projectRoot
try {
    if (-not (Test-Path $UnityPath)) { throw 'Unity Editor not found. Supply -UnityPath with your Editor path.' }
    if (Invoke-ProjectGit @('status','--porcelain')) { throw 'Save and commit your release changes first; publishing requires a clean working tree.' }
    if (Get-Process Unity -ErrorAction SilentlyContinue) { throw 'Close Unity Editor before running this script.' }
    $sourceRevision = Invoke-ProjectGit @('rev-parse','HEAD')
    $remoteUrl = Invoke-ProjectGit @('remote','get-url','origin')
    $authorName = Invoke-ProjectGit @('config','user.name')
    $authorEmail = Invoke-ProjectGit @('config','user.email')
    $logPath = Join-Path $projectRoot 'web-release.log'
    $argsForUnity = @('-batchmode','-nographics','-quit','-projectPath',('"'+$projectRoot+'"'),'-buildTarget','WebGL','-executeMethod','HatHop.Editor.WebReleaseBuilder.Build','-logFile',('"'+$logPath+'"'))
    $process = Start-Process -FilePath $UnityPath -ArgumentList $argsForUnity -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Unity build failed. Read $logPath" }
    $site = Join-Path $projectRoot 'Builds\WebGL'
    if (-not (Test-Path (Join-Path $site 'index.html'))) { throw 'Build did not produce index.html.' }
    if (Invoke-ProjectGit @('status','--porcelain')) { throw 'Unity changed tracked project settings during the build. Review and commit those changes, then rerun to publish the matching revision.' }
    $infoPath = Join-Path $site 'release-info.json'
    $info = Get-Content $infoPath -Raw | ConvertFrom-Json
    $info | Add-Member -NotePropertyName sourceRevision -NotePropertyValue $sourceRevision -Force
    $info | ConvertTo-Json | Set-Content $infoPath -Encoding UTF8
    $large = Get-ChildItem $site -File -Recurse | Where-Object Length -ge 100MB
    if ($large) { throw 'A build file exceeds GitHub Git limits. Use the Unity CI artifact workflow instead.' }
    $tempRepo = Join-Path ([IO.Path]::GetTempPath()) ('leap-of-faith-pages-' + [guid]::NewGuid().ToString('N'))
    New-Item $tempRepo -ItemType Directory | Out-Null
    Push-Location $tempRepo
    try {
        Invoke-ProjectGit @('init','-b','pages-build') | Out-Null
        Invoke-ProjectGit @('config','user.name',$authorName)
        Invoke-ProjectGit @('config','user.email',$authorEmail)
        Invoke-ProjectGit @('remote','add','origin',$remoteUrl)
        $existing = Invoke-ProjectGit @('ls-remote','--heads','origin','pages-build')
        if ($existing) {
            Invoke-ProjectGit @('fetch','origin','pages-build') | Out-Null
            Invoke-ProjectGit @('checkout','-B','pages-build','FETCH_HEAD') | Out-Null
            # This isolated checkout contains generated hosting output only.
            Invoke-ProjectGit @('rm','-r','--ignore-unmatch','.') | Out-Null
        }
        Get-ChildItem $site -Force | Copy-Item -Destination $tempRepo -Recurse -Force
        $workflowFolder = Join-Path $tempRepo '.github\workflows'
        New-Item $workflowFolder -ItemType Directory -Force | Out-Null
        Copy-Item (Join-Path $projectRoot '.github\workflows\pages-build.yml') $workflowFolder
        Invoke-ProjectGit @('add','--all') | Out-Null
        Invoke-ProjectGit @('commit','-m',"Publish Leap of Faith from $sourceRevision") | Out-Null
        Invoke-ProjectGit @('push','-u','origin','pages-build') | Out-Null
    } finally { Pop-Location }
    Write-Host 'Build pushed. Check GitHub Actions > Publish tested Web build for the actual deployment result.'
    Write-Host "Temporary build checkout retained at $tempRepo"
} finally { Pop-Location }
