# 🎨 Guide: Ajouter l'Icône du Logo à l'Application

## 📋 Problème
L'application et son raccourci utilisent l'icône par défaut de Windows au lieu du logo de la résidence.

## ✅ Solution en 3 Étapes

---

## Étape 1: Convertir le Logo PNG en ICO

### Option A: Utiliser un Convertisseur en Ligne (Recommandé)

1. **Aller sur**: https://convertio.co/fr/png-ico/
2. **Uploader**: `logo png.png` (dans le dossier racine du projet)
3. **Sélectionner la taille**: 256x256 pixels (recommandé)
4. **Télécharger**: Le fichier `logo.ico`
5. **Copier** le fichier dans: `src\ElMansourSyndicManager\Assets\logo.ico`

### Option B: Utiliser un Logiciel

**Avec GIMP** (gratuit):
1. Ouvrir `logo png.png` dans GIMP
2. Image > Échelle et taille de l'image > 256x256 pixels
3. Fichier > Exporter sous > `logo.ico`
4. Copier dans: `src\ElMansourSyndicManager\Assets\logo.ico`

**Avec Paint.NET** (gratuit):
1. Ouvrir `logo png.png`
2. Redimensionner à 256x256
3. Enregistrer sous > Type: ICO
4. Copier dans: `src\ElMansourSyndicManager\Assets\logo.ico`

---

## Étape 2: Configurer l'Application pour Utiliser l'Icône

### A. Modifier le Fichier .csproj

Ouvrir: `src\ElMansourSyndicManager\ElMansourSyndicManager.csproj`

Ajouter cette ligne dans le groupe `<PropertyGroup>`:

```xml
<ApplicationIcon>Assets\logo.ico</ApplicationIcon>
```

**Exemple complet**:
```xml
<PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <ApplicationIcon>Assets\logo.ico</ApplicationIcon>
</PropertyGroup>
```

### B. S'assurer que l'icône est incluse

Dans le même fichier `.csproj`, vérifier qu'il y a:

```xml
<ItemGroup>
    <Resource Include="Assets\logo.png" />
    <Resource Include="Assets\logo.ico" />
</ItemGroup>
```

---

## Étape 3: Configurer l'Installateur Inno Setup

Ouvrir: `installer-script.iss`

**Décommenter et modifier** la ligne (vers la ligne 27):

**AVANT**:
```iss
; SetupIconFile=src\ElMansourSyndicManager\Assets\logo.ico
```

**APRÈS**:
```iss
SetupIconFile=src\ElMansourSyndicManager\Assets\logo.ico
```

---

## 🔄 Étape 4: Rebuild et Recompiler

### 1. Nettoyer et Republier
```powershell
dotnet clean src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true
```

### 2. Recompiler l'Installateur
- Ouvrir Inno Setup Compiler
- Ouvrir `installer-script.iss`
- Build > Compile (F9)

---

## ✅ Résultat Attendu

Après installation:
- ✅ **Icône du raccourci bureau**: Logo de la résidence
- ✅ **Icône dans la barre des tâches**: Logo de la résidence
- ✅ **Icône de l'exécutable**: Logo de la résidence
- ✅ **Icône de l'installateur**: Logo de la résidence

---

## 🎯 Commandes Rapides (PowerShell)

```powershell
# 1. Convertir PNG en ICO (manuel - voir sites web ci-dessus)

# 2. Copier l'icône (après conversion)
Copy-Item "logo.ico" "src\ElMansourSyndicManager\Assets\logo.ico"

# 3. Rebuild
dotnet clean src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true

# 4. Compiler l'installateur (manuel avec Inno Setup)
```

---

## 📝 Checklist

- [ ] Logo converti en format ICO (256x256 pixels)
- [ ] Fichier `logo.ico` copié dans `src\ElMansourSyndicManager\Assets\`
- [ ] Ligne `<ApplicationIcon>` ajoutée dans `.csproj`
- [ ] Ligne `SetupIconFile` décommentée dans `installer-script.iss`
- [ ] Application republiée
- [ ] Installateur recompilé
- [ ] Testé sur un PC propre

---

## ⚠️ Notes Importantes

### Taille de l'Icône
- **Recommandé**: 256x256 pixels
- **Formats supportés**: 16x16, 32x32, 48x48, 64x64, 128x128, 256x256
- Un fichier ICO peut contenir plusieurs tailles

### Format ICO vs PNG
- **PNG**: Format d'image standard
- **ICO**: Format spécifique Windows pour les icônes
- Windows a besoin du format ICO pour les icônes d'application

### Cache d'Icônes Windows
Si l'icône ne change pas immédiatement:
1. Redémarrer l'Explorateur Windows
2. Ou redémarrer le PC
3. Ou vider le cache d'icônes:
   ```cmd
   ie4uinit.exe -show
   ```

---

**Date**: 23 novembre 2025  
**Version**: 1.0.0
