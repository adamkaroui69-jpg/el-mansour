# Script de publication avec GitHub Releases
# Pour dépôt privé - El Mansour Syndic Manager

param (
    [string]$CommitMessage = "Mise à jour automatique"
)

$ErrorActionPreference = "Stop"

# 1. Vérification de la configuration Git
try {
    $remoteUrl = git remote get-url origin
    if (-not $remoteUrl) {
        Write-Host "Erreur: Aucun depot distant 'origin' n'est configure." -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "Erreur: Impossible de verifier le depot distant Git." -ForegroundColor Red
    exit 1
}

# Extraction des infos du repo
$repoPath = $remoteUrl -replace "https://github.com/", "" -replace ".git", "" -replace "\n", "" -replace "\r", ""
$branch = git branch --show-current
if (-not $branch) { $branch = "main" }

Write-Host "Depot detecte : $repoPath (Branche: $branch)" -ForegroundColor Cyan

# 2. Gestion de la version
$csprojPath = "src/ElMansourSyndicManager/ElMansourSyndicManager.csproj"
$content = Get-Content $csprojPath -Raw
$versionMatch = [regex]::Match($content, '<Version>([\d.]+)</Version>')
$currentVersion = "1.0.0"

if ($versionMatch.Success) {
    $currentVersion = $versionMatch.Groups[1].Value
}

# Incrémentation simple (Patch)
$vParts = $currentVersion.Split('.')
if ($vParts.Length -eq 3) {
    $newVersion = "{0}.{1}.{2}" -f $vParts[0], $vParts[1], ([int]$vParts[2] + 1)
}
else {
    $newVersion = "1.0.1"
}

Write-Host "Version actuelle : $currentVersion" -ForegroundColor Gray
Write-Host "Nouvelle version : $newVersion" -ForegroundColor Green

# Mise à jour du csproj
$content = $content -replace "<Version>.*</Version>", "<Version>$newVersion</Version>"
$content = $content -replace "<AssemblyVersion>.*</AssemblyVersion>", "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
$content = $content -replace "<FileVersion>.*</FileVersion>", "<FileVersion>$newVersion.0</FileVersion>"

# Si les balises n'existent pas, on les ajoute
if (-not ($content -match "<Version>")) {
    $content = $content -replace "(?s)(<PropertyGroup>.*?)(</PropertyGroup>)", "`$1  <Version>$newVersion</Version>`r`n    <AssemblyVersion>$newVersion.0</AssemblyVersion>`r`n    <FileVersion>$newVersion.0</FileVersion>`r`n  `$2"
}

Set-Content $csprojPath $content

# 3. Construction de l'installateur
Write-Host "Construction de l'application et de l'installateur..." -ForegroundColor Cyan
./build-installer.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la construction." -ForegroundColor Red
    exit 1
}

# 4. Vérifier que gh CLI est installé
$ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
if (-not $ghInstalled) {
    Write-Host "ERREUR: GitHub CLI (gh) n'est pas installe!" -ForegroundColor Red
    Write-Host "Telechargez-le depuis: https://cli.github.com/" -ForegroundColor Yellow
    Write-Host "" -ForegroundColor Yellow
    Write-Host "Ou installez avec winget:" -ForegroundColor Yellow
    Write-Host "  winget install --id GitHub.cli" -ForegroundColor White
    exit 1
}

# 5. Vérifier l'authentification GitHub
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Vous devez vous authentifier avec GitHub CLI:" -ForegroundColor Yellow
    Write-Host "  gh auth login" -ForegroundColor White
    exit 1
}

# 6. Créer une release GitHub
$tagName = "v$newVersion"
$installerPath = "installer-output/ElMansourSyndicManager-Setup-v1.0.0.exe"

# Renommer l'installateur avec la bonne version
$newInstallerPath = "installer-output/ElMansourSyndicManager-Setup-v$newVersion.exe"
if (Test-Path $installerPath) {
    Copy-Item $installerPath $newInstallerPath -Force
}

Write-Host "Creation de la release GitHub $tagName..." -ForegroundColor Cyan

# Créer la release avec le fichier
gh release create $tagName $newInstallerPath `
    --title "Version $newVersion" `
    --notes "Mise a jour automatique - Version $newVersion`n`n$CommitMessage" `
    --repo $repoPath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la creation de la release." -ForegroundColor Red
    exit 1
}

# 7. Récupérer l'URL de téléchargement de la release
$releaseUrl = "https://github.com/$repoPath/releases/download/$tagName/ElMansourSyndicManager-Setup-v$newVersion.exe"

Write-Host "URL de telechargement : $releaseUrl" -ForegroundColor Green

# 8. Mise à jour de update.xml
$updateXmlPath = "update.xml"
$changelogUrl = "https://github.com/$repoPath/releases/tag/$tagName"

$xmlContent = @"
<item>
    <version>$newVersion.0</version>
    <url>$releaseUrl</url>
    <changelog>$changelogUrl</changelog>
    <mandatory>true</mandatory>
</item>
"@

Set-Content $updateXmlPath $xmlContent
Write-Host "Fichier update.xml mis a jour" -ForegroundColor Green

# 9. Mise à jour du code pour pointer vers le bon update.xml
$mainCsPath = "src/ElMansourSyndicManager/MainWindow.xaml.cs"
$mainCsContent = Get-Content $mainCsPath -Raw
$githubUpdateUrl = "https://raw.githubusercontent.com/$repoPath/$branch/update.xml"

if (-not ($mainCsContent -match [regex]::Escape($githubUpdateUrl))) {
    Write-Host "Mise a jour de MainWindow.xaml.cs..." -ForegroundColor Cyan
    $mainCsContent = $mainCsContent -replace 'AutoUpdater\.Start\(".*"\);', "AutoUpdater.Start(`"$githubUpdateUrl`");"
    Set-Content $mainCsPath $mainCsContent
}

# 10. Git Commit & Push
Write-Host "Envoi vers GitHub..." -ForegroundColor Cyan
git add .
git commit -m "$CommitMessage - v$newVersion"
git push origin $branch

Write-Host "---------------------------------------------------" -ForegroundColor Green
Write-Host "Mise a jour v$newVersion publiee avec succes !" -ForegroundColor Green
Write-Host "Release GitHub : https://github.com/$repoPath/releases/tag/$tagName" -ForegroundColor Cyan
Write-Host "Les utilisateurs recevront la mise a jour au prochain lancement." -ForegroundColor Green
Write-Host "---------------------------------------------------" -ForegroundColor Green
