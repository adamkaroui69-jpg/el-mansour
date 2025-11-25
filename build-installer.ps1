# Script de compilation automatique de l'installateur
# El Mansour Syndic Manager

param(
    [string]$Version = "1.0.0",
    [switch]$SkipPublish = $false,
    [switch]$OpenOutput = $true
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Compilation de l'Installateur" -ForegroundColor Cyan
Write-Host "  El Mansour Syndic Manager v$Version" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Chemins
$projectRoot = $PSScriptRoot
$projectFile = Join-Path $projectRoot "src\ElMansourSyndicManager\ElMansourSyndicManager.csproj"
$issScript = Join-Path $projectRoot "installer-script.iss"
$outputDir = Join-Path $projectRoot "installer-output"
$innoSetupPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

# Vérifier Inno Setup
if (-not (Test-Path $innoSetupPath)) {
    Write-Host "Erreur: Inno Setup n'est pas installe!" -ForegroundColor Red
    Write-Host "   Telecharger depuis: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

# Etape 1: Publier l'application (si necessaire)
if (-not $SkipPublish) {
    Write-Host "Etape 1/3: Publication de l'application..." -ForegroundColor Yellow
    
    $publishArgs = @(
        "publish",
        $projectFile,
        "-c", "Release",
        "-r", "win-x64",
        "--self-contained", "true",
        "-p:PublishSingleFile=false"
    )
    
    & dotnet $publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Erreur lors de la publication!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Publication reussie!" -ForegroundColor Green
    Write-Host ""
}
else {
    Write-Host "Publication ignoree (--SkipPublish)" -ForegroundColor Gray
    Write-Host ""
}

# Etape 2: Mettre a jour la version dans le script ISS (optionnel)
Write-Host "Etape 2/3: Verification du script Inno Setup..." -ForegroundColor Yellow

if (Test-Path $issScript) {
    Write-Host "Script trouve: $issScript" -ForegroundColor Green
}
else {
    Write-Host "Script introuvable: $issScript" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Etape 3: Compiler l'installateur
Write-Host "Etape 3/3: Compilation de l'installateur..." -ForegroundColor Yellow

& $innoSetupPath $issScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la compilation!" -ForegroundColor Red
    exit 1
}

Write-Host "Compilation reussie!" -ForegroundColor Green
Write-Host ""

# Afficher les resultats
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Compilation Terminee!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Trouver le fichier de sortie
$setupFile = Get-ChildItem -Path $outputDir -Filter "*.exe" | Select-Object -First 1

if ($setupFile) {
    $sizeInMB = [math]::Round($setupFile.Length / 1MB, 2)
    
    Write-Host "Fichier cree:" -ForegroundColor Cyan
    Write-Host "   Nom: $($setupFile.Name)" -ForegroundColor White
    Write-Host "   Taille: $sizeInMB MB" -ForegroundColor White
    Write-Host "   Emplacement: $($setupFile.FullName)" -ForegroundColor White
    Write-Host ""
    
    # Ouvrir le dossier de sortie
    if ($OpenOutput) {
        Write-Host "Ouverture du dossier de sortie..." -ForegroundColor Yellow
        Start-Process explorer.exe -ArgumentList $outputDir
    }
    
    Write-Host "L'installateur est pret a etre distribue!" -ForegroundColor Green
}
else {
    Write-Host "Fichier de sortie introuvable dans: $outputDir" -ForegroundColor Yellow
}

Write-Host ""
if ($setupFile) {
    Write-Host "Pour tester l'installateur, executez:" -ForegroundColor Cyan
    Write-Host "   $($setupFile.FullName)" -ForegroundColor White
}
Write-Host ""
