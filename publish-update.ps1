# Script de publication de mise à jour automatique sur GitHub
param (
    [string]$CommitMessage = "Mise à jour automatique"
)

$ErrorActionPreference = "Stop"

# 1. Vérification de la configuration Git
try {
    $remoteUrl = git remote get-url origin
    if (-not $remoteUrl) {
        Write-Host "Erreur: Aucun dépôt distant 'origin' n'est configuré." -ForegroundColor Red
        Write-Host "Veuillez ajouter votre dépôt GitHub avec : git remote add origin https://github.com/VOTRE_USER/VOTRE_REPO.git" -ForegroundColor Yellow
        exit 1
    }
}
catch {
    Write-Host "Erreur: Impossible de vérifier le dépôt distant Git." -ForegroundColor Red
    Write-Host "Assurez-vous d'avoir initialisé git (git init) et configuré un remote." -ForegroundColor Yellow
    exit 1
}

# Extraction des infos du repo (ex: https://github.com/user/repo.git -> user/repo)
$repoPath = $remoteUrl -replace "https://github.com/", "" -replace ".git", "" -replace "\n", "" -replace "\r", ""
$branch = git branch --show-current
if (-not $branch) { $branch = "main" }

$rawBaseUrl = "https://raw.githubusercontent.com/$repoPath/$branch"

Write-Host "Dépôt détecté : $repoPath (Branche: $branch)" -ForegroundColor Cyan
Write-Host "URL de base Raw : $rawBaseUrl" -ForegroundColor Cyan

# 2. Gestion de la version
$csprojPath = "src/ElMansourSyndicManager/ElMansourSyndicManager.csproj"
$content = Get-Content $csprojPath
$versionLine = $content | Select-String "<Version>(.*)</Version>"
$currentVersion = "1.0.0"

if ($versionLine) {
    $currentVersion = $versionLine.Matches.Groups[1].Value
}
else {
    # Si pas de version, on l'ajoute
    Write-Host "Pas de version trouvée dans le csproj, ajout de 1.0.0" -ForegroundColor Yellow
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
$newContent = $content -replace "<Version>.*</Version>", "<Version>$newVersion</Version>"
$newContent = $newContent -replace "<AssemblyVersion>.*</AssemblyVersion>", "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
$newContent = $newContent -replace "<FileVersion>.*</FileVersion>", "<FileVersion>$newVersion.0</FileVersion>"

# Si les balises n'existent pas, on les ajoute dans le premier PropertyGroup
if (-not ($newContent | Select-String "<Version>")) {
    $newContent = $newContent -replace "</PropertyGroup>", "  <Version>$newVersion</Version>`n    <AssemblyVersion>$newVersion.0</AssemblyVersion>`n    <FileVersion>$newVersion.0</FileVersion>`n  </PropertyGroup>"
}

Set-Content $csprojPath $newContent

# 3. Construction de l'installateur
Write-Host "Construction de l'application et de l'installateur..." -ForegroundColor Cyan
./build-installer.ps1

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la construction." -ForegroundColor Red
    exit 1
}

# 4. Mise à jour de update.xml
$installerUrl = "$rawBaseUrl/installer-output/setup.exe"
$updateXmlPath = "update.xml"

$xmlContent = @"
<item>
    <version>$newVersion.0</version>
    <url>$installerUrl</url>
    <changelog>$rawBaseUrl/CHANGELOG.md</changelog>
    <mandatory>false</mandatory>
</item>
"@

Set-Content $updateXmlPath $xmlContent
Write-Host "Fichier update.xml mis à jour avec l'URL : $installerUrl" -ForegroundColor Green

# 5. Mise à jour du code pour pointer vers le bon update.xml (si nécessaire)
# On vérifie si MainWindow.xaml.cs pointe bien vers le bon update.xml GitHub
$mainCsPath = "src/ElMansourSyndicManager/MainWindow.xaml.cs"
$mainCsContent = Get-Content $mainCsPath -Raw
$githubUpdateUrl = "$rawBaseUrl/update.xml"

if (-not ($mainCsContent | Select-String -Pattern ([regex]::Escape($githubUpdateUrl)))) {
    Write-Host "Mise à jour de MainWindow.xaml.cs pour pointer vers GitHub..." -ForegroundColor Cyan
    $mainCsContent = $mainCsContent -replace 'AutoUpdater.Start\(".*"\);', "AutoUpdater.Start(`"$githubUpdateUrl`");"
    Set-Content $mainCsPath $mainCsContent
}

# 6. Git Commit & Push
Write-Host "Envoi vers GitHub..." -ForegroundColor Cyan
git add .
git commit -m "$CommitMessage - v$newVersion"
git push origin $branch

Write-Host "---------------------------------------------------" -ForegroundColor Green
Write-Host "Mise à jour v$newVersion publiée avec succès !" -ForegroundColor Green
Write-Host "Les utilisateurs recevront la mise à jour au prochain lancement." -ForegroundColor Green
Write-Host "---------------------------------------------------" -ForegroundColor Green
