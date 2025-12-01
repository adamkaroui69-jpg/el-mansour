# Script d'exécution des tests unitaires
# El Mansour Syndic Manager

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("All", "Services", "Domain", "Quick")]
    [string]$TestType = "All",
    
    [Parameter(Mandatory = $false)]
    [switch]$ShowDetails
)

$ErrorActionPreference = "Stop"
$testProjectPath = "tests\ElMansourSyndicManager.Tests.Unit"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Tests Unitaires - El Mansour Syndic" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Navigation vers le dossier du projet de tests
if (Test-Path $testProjectPath) {
    Set-Location $testProjectPath
}
else {
    Write-Host "Dossier de tests introuvable: $testProjectPath" -ForegroundColor Red
    exit 1
}

try {
    switch ($TestType) {
        "All" {
            Write-Host "Exécution de tous les tests..." -ForegroundColor Yellow
            Write-Host ""
            
            if ($ShowDetails) {
                dotnet test --logger "console;verbosity=detailed"
            }
            else {
                dotnet test
            }
        }
        
        "Services" {
            Write-Host "Exécution des tests de services..." -ForegroundColor Yellow
            Write-Host ""
            
            $filter = "FullyQualifiedName~Services"
            if ($ShowDetails) {
                dotnet test --filter $filter --logger "console;verbosity=detailed"
            }
            else {
                dotnet test --filter $filter
            }
        }
        
        "Domain" {
            Write-Host "Exécution des tests du domaine..." -ForegroundColor Yellow
            Write-Host ""
            
            $filter = "FullyQualifiedName~Domain"
            if ($ShowDetails) {
                dotnet test --filter $filter --logger "console;verbosity=detailed"
            }
            else {
                dotnet test --filter $filter
            }
        }
        
        "Quick" {
            Write-Host "Exécution rapide des tests (sans build)..." -ForegroundColor Yellow
            Write-Host ""
            
            dotnet test --no-build --no-restore
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Tests terminés avec succès!" -ForegroundColor Green
    }
    else {
        Write-Host "Certains tests ont échoué!" -ForegroundColor Red
        exit 1
    }
    
}
catch {
    Write-Host ""
    Write-Host "Erreur lors de l'exécution des tests:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}
finally {
    # Retour au dossier racine
    Set-Location ..\..
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
