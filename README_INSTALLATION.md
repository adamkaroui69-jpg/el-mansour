# 🚀 Installation Rapide - El Mansour Syndic Manager

## Pour Compiler l'Installateur

### Méthode Rapide (Interface Graphique)

1. **Télécharger et installer Inno Setup:**
   - Aller sur: https://jrsoftware.org/isdl.php
   - Télécharger "Inno Setup 6.2.2" ou supérieur
   - Installer avec les options par défaut

2. **Ouvrir le script:**
   - Lancer **Inno Setup Compiler**
   - Fichier > Ouvrir
   - Sélectionner: `installer-script.iss`

3. **Compiler:**
   - Cliquer sur **Build > Compile** (ou F9)
   - Attendre la fin de la compilation

4. **Récupérer l'installateur:**
   - Le fichier sera dans: `installer-output\ElMansourSyndicManager-Setup-v1.0.0.exe`

### Méthode Ligne de Commande

```powershell
# Depuis ce dossier, exécuter:
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "installer-script.iss"
```

## Pour Installer l'Application (Utilisateurs Finaux)

1. **Exécuter** `ElMansourSyndicManager-Setup-v1.0.0.exe`
2. **Suivre** l'assistant d'installation
3. **Accepter** les options par défaut
4. **Lancer** l'application à la fin

### Configuration Requise
- Windows 10 (version 1809+) ou Windows 11
- .NET 8 Runtime Desktop (l'installateur le détectera automatiquement)
- 500 MB d'espace disque
- Droits administrateur pour l'installation

## 📁 Structure Après Installation

```
C:\Program Files\ElMansourSyndicManager\
├── ElMansourSyndicManager.exe    # Application principale
├── appsettings.json               # Configuration
├── data\                          # Données de l'application
│   ├── Receipts\                  # Reçus PDF
│   ├── reports\                   # Rapports PDF/CSV
│   ├── backups\                   # Sauvegardes
│   └── logs\                      # Fichiers de log
└── [autres DLL et fichiers]
```

## 🔄 Mise à Jour

Pour mettre à jour:
1. Exécuter le nouveau setup
2. Les données seront automatiquement conservées
3. L'ancienne version sera remplacée

## ❌ Désinstallation

1. **Paramètres Windows** > Applications
2. Chercher "El Mansour Syndic Manager"
3. Cliquer sur **Désinstaller**

**Note:** Les données utilisateur (base de données, reçus, rapports) seront conservées et peuvent être supprimées manuellement si nécessaire.

## 📖 Documentation Complète

Voir `GUIDE_CREATION_SETUP.md` pour plus de détails.

---

**Version:** 1.0.0  
**Date:** Novembre 2025
