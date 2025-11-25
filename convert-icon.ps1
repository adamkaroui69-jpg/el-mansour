# Script pour convertir PNG en ICO
param(
    [string]$InputPng,
    [string]$OutputIco
)

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path $InputPng)) {
    Write-Host "Erreur: Fichier source introuvable: $InputPng" -ForegroundColor Red
    exit 1
}

$sourceImg = [System.Drawing.Bitmap]::FromFile($InputPng)
$icon = [System.Drawing.Icon]::FromHandle($sourceImg.GetHicon())

$fileStream = New-Object System.IO.FileStream($OutputIco, [System.IO.FileMode]::Create)
$icon.Save($fileStream)
$fileStream.Close()
$sourceImg.Dispose()
$icon.Dispose()

Write-Host "Conversion réussie: $OutputIco" -ForegroundColor Green
