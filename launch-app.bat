@echo off
echo =====================================
echo El Mansour Syndic Manager - Launcher
echo =====================================
echo.

echo [1/3] Nettoyage...
dotnet clean >nul 2>&1
if errorlevel 1 (
    echo Erreur lors du nettoyage!
    pause
    exit /b 1
)
echo OK - Nettoyage termine
echo.

echo [2/3] Construction...
dotnet build --configuration Debug >nul 2>&1
if errorlevel 1 (
    echo Erreur lors de la construction!
    echo Executez 'dotnet build' pour voir les details.
    pause
    exit /b 1
)
echo OK - Construction reussie
echo.

echo [3/3] Lancement de l'application...
echo.
dotnet run --project "src\ElMansourSyndicManager\ElMansourSyndicManager.csproj" --no-build

echo.
echo Application fermee.
pause
