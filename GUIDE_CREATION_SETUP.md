# 📦 Guide de Création du Setup - El Mansour Syndic Manager

## ✅ Prérequis

### 1. Installer Inno Setup
- Télécharger depuis: https://jrsoftware.org/isdl.php
- Version recommandée: Inno Setup 6.2.2 ou supérieure
- Installer avec les options par défaut

### 2. Vérifier .NET 8 SDK
```powershell
dotnet --version
# Doit afficher 8.0.x ou supérieur
```

## 🔨 Étapes de Compilation

### Étape 1: Publier l'Application (✅ DÉJÀ FAIT)

L'application a déjà été publiée dans:
```
src\ElMansourSyndicManager\bin\Release\net8.0-windows\win-x64\publish\
```

Si vous devez republier:
```powershell
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true
```

### Étape 2: Compiler l'Installateur avec Inno Setup

#### Option A: Via l'Interface Graphique (Recommandé)
1. Ouvrir **Inno Setup Compiler**
2. Cliquer sur **File > Open**
3. Sélectionner: `c:\Users\adamk\Desktop\raisidance application\installer-script.iss`
4. Cliquer sur **Build > Compile** (ou appuyer sur F9)
5. Attendre la fin de la compilation

#### Option B: Via la Ligne de Commande
```powershell
# Depuis le dossier racine du projet
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer-script.iss"
```

### Étape 3: Localiser l'Installateur

Après compilation, le fichier sera créé dans:
```
installer-output\ElMansourSyndicManager-Setup-v1.0.0.exe
```

## 📋 Fonctionnalités de l'Installateur

### ✨ Inclus dans le Setup:
- ✅ Vérification automatique de .NET 8 Runtime
- ✅ Installation de tous les fichiers de l'application
- ✅ Création des dossiers de données (Receipts, reports, backups, logs)
- ✅ Raccourcis dans le menu Démarrer
- ✅ Option de raccourci sur le bureau
- ✅ Entrées dans le registre Windows
- ✅ Désinstallateur complet
- ✅ Conservation des données utilisateur lors de la désinstallation
- ✅ Interface en français

### 🎯 Comportement lors de l'Installation:
1. Vérifie si .NET 8 Runtime est installé
   - Si non installé: propose de télécharger
2. Demande le dossier d'installation (par défaut: `C:\Program Files\ElMansourSyndicManager`)
3. Propose de créer un raccourci sur le bureau
4. Copie tous les fichiers
5. Crée les dossiers de données avec permissions complètes
6. Enregistre l'application dans le registre
7. Propose de lancer l'application

## 🧪 Tester l'Installateur

### Test 1: Installation Propre
1. Exécuter `ElMansourSyndicManager-Setup-v1.0.0.exe`
2. Suivre l'assistant d'installation
3. Vérifier que l'application se lance correctement
4. Vérifier les dossiers créés dans `C:\Program Files\ElMansourSyndicManager\data`

### Test 2: Mise à Jour
1. Modifier la version dans `installer-script.iss` (ligne 5): `#define MyAppVersion "1.0.1"`
2. Recompiler
3. Exécuter le nouveau setup
4. Vérifier que les données sont conservées

### Test 3: Désinstallation
1. Aller dans **Paramètres > Applications**
2. Chercher "El Mansour Syndic Manager"
3. Cliquer sur **Désinstaller**
4. Vérifier que:
   - L'application est supprimée
   - Les données utilisateur sont conservées (base de données, reçus, rapports)
   - Les logs temporaires sont supprimés

## 🎨 Personnalisation (Optionnel)

### Ajouter une Icône à l'Installateur

1. **Convertir le logo PNG en ICO:**
   - Utiliser un outil en ligne: https://convertio.co/png-ico/
   - Ou utiliser un logiciel comme GIMP, IrfanView
   - Taille recommandée: 256x256 pixels

2. **Modifier le script:**
   ```iss
   ; Décommenter et mettre à jour la ligne:
   SetupIconFile=src\ElMansourSyndicManager\Assets\logo.ico
   ```

3. **Recompiler**

### Modifier la Version

Dans `installer-script.iss`, ligne 5:
```iss
#define MyAppVersion "1.0.0"  // Changer ici
```

## 📤 Distribution

### Méthode 1: Distribution Directe
- Copier `ElMansourSyndicManager-Setup-v1.0.0.exe` sur une clé USB
- Envoyer par email (si < 25 MB)
- Partager via OneDrive/Google Drive

### Méthode 2: Hébergement Web
- Héberger sur un serveur web
- Créer un lien de téléchargement
- Partager le lien aux utilisateurs

### Méthode 3: Réseau Local
- Placer sur un partage réseau
- Les utilisateurs peuvent installer depuis le réseau

## 🔧 Dépannage

### Problème: "Inno Setup n'est pas reconnu"
**Solution:** Ajouter Inno Setup au PATH ou utiliser le chemin complet:
```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer-script.iss"
```

### Problème: "Fichiers source introuvables"
**Solution:** Vérifier que l'application a été publiée:
```powershell
dotnet publish src\ElMansourSyndicManager\ElMansourSyndicManager.csproj -c Release -r win-x64 --self-contained true
```

### Problème: "Erreur de compilation"
**Solution:** 
1. Ouvrir le script dans Inno Setup
2. Vérifier les messages d'erreur
3. Corriger les chemins de fichiers si nécessaire

## 📊 Taille de l'Installateur

Taille approximative: **150-200 MB**
- Application compilée: ~100 MB
- Runtime .NET (si inclus): ~50-100 MB
- Compression: LZMA2 Ultra64 (meilleure compression)

## ✅ Checklist Finale

Avant de distribuer l'installateur:

- [ ] L'application a été testée en mode Release
- [ ] Tous les reçus et rapports s'affichent correctement
- [ ] Le logo est présent
- [ ] La version est correcte dans le script
- [ ] L'installateur a été testé sur une machine propre
- [ ] La désinstallation fonctionne correctement
- [ ] Les données sont conservées après mise à jour
- [ ] .NET 8 Runtime est détecté correctement

## 🎉 Prochaines Étapes

Une fois l'installateur créé:

1. **Tester sur plusieurs machines**
   - Windows 10 (version 1809+)
   - Windows 11

2. **Créer une documentation utilisateur**
   - Guide d'installation
   - Guide d'utilisation
   - FAQ

3. **Planifier les mises à jour**
   - Système de versioning
   - Notes de version
   - Processus de mise à jour

## 📞 Support

Pour toute question sur la création du setup:
- Vérifier la documentation Inno Setup: https://jrsoftware.org/ishelp/
- Consulter les exemples dans: `C:\Program Files (x86)\Inno Setup 6\Examples\`

---

**Créé le:** 23 novembre 2025  
**Version du guide:** 1.0  
**Application:** El Mansour Syndic Manager v1.0.0
