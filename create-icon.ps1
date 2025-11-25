Add-Type -AssemblyName System.Drawing

# Charger l'image PNG
$pngPath = "src\ElMansourSyndicManager\Assets\logo.png"
$icoPath = "src\ElMansourSyndicManager\Assets\logo_new.ico"

Write-Host "Chargement de $pngPath..."
$img = [System.Drawing.Image]::FromFile((Resolve-Path $pngPath))

# Créer un bitmap à partir de l'image
$bitmap = New-Object System.Drawing.Bitmap $img

# Créer l'icône
Write-Host "Création de l'icône..."
$icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())

# Sauvegarder l'icône
Write-Host "Sauvegarde vers $icoPath..."
$fileStream = [System.IO.File]::Create((Join-Path $PWD $icoPath))
$icon.Save($fileStream)
$fileStream.Close()

# Nettoyer
$bitmap.Dispose()
$img.Dispose()

Write-Host "Icône créée avec succès : $icoPath"
