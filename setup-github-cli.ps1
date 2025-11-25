# Script de configuration GitHub CLI
# Ce script configure le PATH et lance l'authentification

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Configuration GitHub CLI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Ajouter GitHub CLI au PATH de cette session
$ghPath = "C:\Program Files\GitHub CLI"
if (Test-Path "$ghPath\gh.exe") {
    $env:Path += ";$ghPath"
    Write-Host "GitHub CLI trouve et ajoute au PATH" -ForegroundColor Green
}
else {
    Write-Host "ERREUR: GitHub CLI n'est pas installe correctement" -ForegroundColor Red
    Write-Host "Reinstallez avec: winget install --id GitHub.cli" -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Verification de l'authentification..." -ForegroundColor Yellow

# Vérifier si déjà authentifié
$authCheck = & gh auth status 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Vous etes deja authentifie!" -ForegroundColor Green
    Write-Host $authCheck
    Write-Host ""
    Write-Host "Vous pouvez maintenant utiliser:" -ForegroundColor Cyan
    Write-Host "  ./publish-update-private.ps1" -ForegroundColor White
}
else {
    Write-Host ""
    Write-Host "Vous devez vous authentifier." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Instructions:" -ForegroundColor Cyan
    Write-Host "1. Un code va s'afficher (ex: ABCD-1234)" -ForegroundColor White
    Write-Host "2. Votre navigateur va s'ouvrir automatiquement" -ForegroundColor White
    Write-Host "3. Collez le code dans la page GitHub" -ForegroundColor White
    Write-Host "4. Autorisez l'acces" -ForegroundColor White
    Write-Host ""
    Write-Host "Appuyez sur Entree pour continuer..." -ForegroundColor Yellow
    Read-Host
    
    # Lancer l'authentification
    & gh auth login
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  Authentification reussie!" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Vous pouvez maintenant publier des mises a jour avec:" -ForegroundColor Cyan
        Write-Host "  ./publish-update-private.ps1" -ForegroundColor White
    }
    else {
        Write-Host ""
        Write-Host "Erreur lors de l'authentification." -ForegroundColor Red
        Write-Host "Reessayez en executant ce script a nouveau." -ForegroundColor Yellow
    }
}

Write-Host ""
