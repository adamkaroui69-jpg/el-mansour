@echo off
echo =====================================
echo El Mansour - Diagnostic et Lancement
echo =====================================
echo.

echo [DIAGNOSTIC] Verification des fichiers...
if not exist "src\ElMansourSyndicManager\bin\Debug\net8.0-windows\ElMansourSyndicManager.exe" (
    echo ERREUR: L'executable n'existe pas!
    echo Lancement du build complet...
    dotnet build --configuration Debug
)

echo.
echo [DIAGNOSTIC] Verification des DLLs natives...
dir "src\ElMansourSyndicManager\bin\Debug\net8.0-windows\runtimes" /s /b 2>nul | find "libSkiaSharp"
if errorlevel 1 (
    echo ATTENTION: Les DLLs natives SkiaSharp peuvent etre manquantes
)

echo.
echo [LANCEMENT] Demarrage de l'application...
echo.

cd src\ElMansourSyndicManager
dotnet run --no-build --configuration Debug 2>&1

cd ..\..
echo.
echo Application fermee.
pause
