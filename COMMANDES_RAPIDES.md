# ⚡ Commandes Rapides - Setup

## 🔨 Compiler l'Installateur

### Méthode 1: Script Automatique (Recommandé)
```powershell
.\build-installer.ps1
```

### Méthode 2: Inno Setup GUI
1. Ouvrir Inno Setup Compiler
2. Ouvrir `installer-script.iss`
3. Appuyer sur F9

### Méthode 3: Ligne de Commande
```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer-script.iss"
```

## 📦 Republier l'Application (si nécessaire)

```powershell
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true
```

## 🧪 Tester l'Installateur

```powershell
# Exécuter l'installateur
.\installer-output\ElMansourSyndicManager-Setup-v1.0.0.exe
```

## 📂 Ouvrir les Dossiers Importants

```powershell
# Dossier de sortie de l'installateur
explorer installer-output

# Dossier de publication
explorer src\ElMansourSyndicManager\bin\Release\net8.0-windows\win-x64\publish

# Dossier racine
explorer .
```

## 🔄 Workflow Complet

```powershell
# 1. Republier (si changements)
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true

# 2. Compiler l'installateur
.\build-installer.ps1

# 3. Tester
.\installer-output\ElMansourSyndicManager-Setup-v1.0.0.exe
```

## 📝 Changer la Version

1. Ouvrir `installer-script.iss`
2. Modifier ligne 5: `#define MyAppVersion "1.0.1"`
3. Recompiler

## 🎯 Commandes Utiles

```powershell
# Vérifier .NET
dotnet --version

# Nettoyer les builds
dotnet clean

# Voir la taille de l'installateur
Get-ChildItem installer-output\*.exe | Select-Object Name, @{Name="Size (MB)";Expression={[math]::Round($_.Length/1MB, 2)}}

# Ouvrir les guides
notepad SETUP_RESUME.md
notepad GUIDE_CREATION_SETUP.md
notepad README_INSTALLATION.md
```

---

**Astuce:** Ajoutez ce fichier aux favoris pour un accès rapide! 🌟
