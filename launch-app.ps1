# Script pour nettoyer, construire et lancer l'application El Mansour Syndic Manager
# Auteur: Assistant
# Date: 2025-12-01

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "El Mansour Syndic Manager - Launcher" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Étape 1: Nettoyage
Write-Host "[1/3] Nettoyage des fichiers de build..." -ForegroundColor Yellow
dotnet clean --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors du nettoyage!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Nettoyage terminé" -ForegroundColor Green
Write-Host ""

# Étape 2: Construction
Write-Host "[2/3] Construction de l'application..." -ForegroundColor Yellow
dotnet build --configuration Debug --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la construction!" -ForegroundColor Red
    Write-Host "Exécutez 'dotnet build' pour voir les détails de l'erreur." -ForegroundColor Yellow
    exit 1
}
Write-Host "✓ Construction réussie" -ForegroundColor Green
Write-Host ""

# Étape 3: Lancement
Write-Host "[3/3] Lancement de l'application..." -ForegroundColor Yellow
Write-Host ""
Write-Host "L'application démarre..." -ForegroundColor Cyan
Write-Host ""

# Lancer l'application avec dotnet run
dotnet run --project "src\ElMansourSyndicManager\ElMansourSyndicManager.csproj" --no-build

Write-Host ""
Write-Host "Application fermée." -ForegroundColor Yellow
