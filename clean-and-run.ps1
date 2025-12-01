# Script de nettoyage et relance de l'application
Write-Host "=== Nettoyage du projet ===" -ForegroundColor Cyan

# 1. Arrêter tous les processus dotnet en cours
Write-Host "Arrêt des processus dotnet..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# 2. Nettoyer le projet
Write-Host "Nettoyage des fichiers de build..." -ForegroundColor Yellow
dotnet clean

# 3. Supprimer les dossiers bin et obj
Write-Host "Suppression des dossiers bin et obj..." -ForegroundColor Yellow
Get-ChildItem -Path . -Include bin, obj -Recurse -Directory | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue

# 4. Restaurer les packages
Write-Host "Restauration des packages NuGet..." -ForegroundColor Yellow
dotnet restore

# 5. Compiler le projet
Write-Host "Compilation du projet..." -ForegroundColor Yellow
dotnet build

# 6. Lancer l'application
Write-Host "=== Lancement de l'application ===" -ForegroundColor Green
dotnet run --project src/ElMansourSyndicManager/ElMansourSyndicManager.csproj
