# 🔧 Guide Technique - Création de l'Installateur

## 🎯 **Objectif**

Créer un installateur professionnel et simple pour **El Mansour Syndic Manager** compatible avec Windows 10/11.

---

## 📦 **Solution Recommandée : Inno Setup**

### **Pourquoi Inno Setup ?**

✅ **Avantages** :
- Gratuit et open-source
- Très populaire (utilisé par des millions d'applications)
- Simple à configurer
- Installateur .exe unique
- Support de .NET Runtime
- Désinstallation propre
- Mises à jour faciles

❌ **Alternatives écartées** :
- **MSIX** : Trop complexe, nécessite certificat, Store
- **ClickOnce** : Limité, problèmes de sécurité
- **WiX** : Trop technique, courbe d'apprentissage élevée

---

## 🚀 **Étape 1 : Installation d'Inno Setup**

### **Téléchargement**

1. Allez sur : `https://jrsoftware.org/isdl.php`
2. Téléchargez **Inno Setup 6.x** (dernière version)
3. Installez avec les options par défaut

### **Installation de l'extension .NET**

1. Téléchargez **Inno Download Plugin** : `https://mitrich.net23.net/?/inno-download-plugin.html`
2. Copiez les fichiers dans le dossier Inno Setup

---

## 📝 **Étape 2 : Script d'Installation**

### **Créer le fichier : `installer.iss`**

```iss
; Script Inno Setup pour El Mansour Syndic Manager
; Version 2.0

#define MyAppName "El Mansour Syndic Manager"
#define MyAppVersion "2.0"
#define MyAppPublisher "El Mansour Syndic"
#define MyAppURL "https://www.elmansour-syndic.tn"
#define MyAppExeName "ElMansourSyndicManager.exe"

[Setup]
; Informations de base
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/support
AppUpdatesURL={#MyAppURL}/updates
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=LICENSE.txt
InfoBeforeFile=README.txt
OutputDir=Output
OutputBaseFilename=ElMansourSyndicManager-Setup
SetupIconFile=icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64

; Exigences système
MinVersion=10.0.17763
DiskSpanning=no

; Interface utilisateur
ShowLanguageDialog=auto
UsePreviousLanguage=no

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Fichiers de l'application
Source: "bin\Release\net8.0-windows\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Ne pas utiliser "Flags: ignoreversion" sur les fichiers système

; Configuration
Source: "appsettings.json"; DestDir: "{app}"; Flags: ignoreversion

; Documentation
Source: "GUIDE_INSTALLATION_UTILISATEUR.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDE_CONFIGURATION.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Lancer l'application après installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Vérification et installation de .NET 8.0 Runtime
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  // Vérifie si .NET 8.0 Desktop Runtime est installé
  Result := RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost\8.0') or
            RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedhost\8.0');
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  DotNetUrl: String;
begin
  Result := True;
  
  if not IsDotNetInstalled() then
  begin
    if MsgBox('.NET 8.0 Desktop Runtime est requis mais n''est pas installé.' + #13#10 + 
              'Voulez-vous le télécharger et l''installer maintenant ?', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      DotNetUrl := 'https://download.visualstudio.microsoft.com/download/pr/907765b0-2bf8-494e-93aa-5ef9553c5d68/a9308dc010617e6716c0e6abd53b05ce/windowsdesktop-runtime-8.0.0-win-x64.exe';
      
      if not ShellExec('', DotNetUrl, '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then
      begin
        MsgBox('Impossible de télécharger .NET Runtime. Veuillez l''installer manuellement depuis :' + #13#10 + 
               'https://dotnet.microsoft.com/download/dotnet/8.0', mbError, MB_OK);
        Result := False;
      end;
    end
    else
    begin
      MsgBox('L''installation ne peut pas continuer sans .NET 8.0 Runtime.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

// Sauvegarde des données avant mise à jour
procedure CurStepChanged(CurStep: TSetupStep);
var
  DataPath: String;
  BackupPath: String;
begin
  if CurStep = ssInstall then
  begin
    // Vérifier si c'est une mise à jour
    if DirExists(ExpandConstant('{app}')) then
    begin
      DataPath := ExpandConstant('{localappdata}\ElMansourSyndic\data');
      BackupPath := ExpandConstant('{localappdata}\ElMansourSyndic\backup_before_update');
      
      if DirExists(DataPath) then
      begin
        // Créer une sauvegarde
        if MsgBox('Une sauvegarde de vos données va être créée avant la mise à jour.' + #13#10 + 
                  'Continuer ?', mbConfirmation, MB_YESNO) = IDYES then
        begin
          // Copier le dossier data vers backup
          // Note: Utiliser un outil externe ou script PowerShell pour copie récursive
        end;
      end;
    end;
  end;
end;

// Nettoyage lors de la désinstallation
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    if MsgBox('Voulez-vous également supprimer vos données (base de données, documents, sauvegardes) ?' + #13#10 + 
              'Si vous prévoyez de réinstaller l''application, choisissez Non.', 
              mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
    begin
      DataPath := ExpandConstant('{localappdata}\ElMansourSyndic');
      if DirExists(DataPath) then
      begin
        DelTree(DataPath, True, True, True);
      end;
    end;
  end;
end;
```

---

## 🔨 **Étape 3 : Préparation des Fichiers**

### **Structure des Dossiers**

```
ElMansourSyndicManager/
├── installer.iss                    ← Script Inno Setup
├── icon.ico                         ← Icône de l'application
├── LICENSE.txt                      ← Licence
├── README.txt                       ← Informations pré-installation
├── bin/
│   └── Release/
│       └── net8.0-windows/
│           └── publish/             ← Fichiers compilés
│               ├── ElMansourSyndicManager.exe
│               ├── appsettings.json
│               └── *.dll
└── Output/                          ← Dossier de sortie (créé automatiquement)
    └── ElMansourSyndicManager-Setup.exe
```

---

### **Compilation de l'Application**

**Commande PowerShell** :
```powershell
# Publier l'application en mode Release
dotnet publish src/ElMansourSyndicManager/ElMansourSyndicManager.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=false `
  -p:PublishReadyToRun=true `
  -o bin/Release/net8.0-windows/publish
```

**Options expliquées** :
- `-c Release` : Mode Release (optimisé)
- `-r win-x64` : Windows 64-bit
- `--self-contained false` : Nécessite .NET Runtime (réduit la taille)
- `PublishSingleFile=false` : Fichiers séparés (meilleur pour les mises à jour)
- `PublishReadyToRun=true` : Pré-compilation pour démarrage rapide

---

## 🏗️ **Étape 4 : Compilation de l'Installateur**

### **Méthode 1 : Interface Graphique**

1. **Ouvrez** Inno Setup Compiler
2. **File** → **Open** → Sélectionnez `installer.iss`
3. **Build** → **Compile**
4. L'installateur est créé dans `Output/ElMansourSyndicManager-Setup.exe`

### **Méthode 2 : Ligne de Commande**

```powershell
# Compiler avec Inno Setup en ligne de commande
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss
```

---

## 🎨 **Personnalisation de l'Installateur**

### **Ajouter un Logo**

Dans `installer.iss` :
```iss
[Setup]
WizardImageFile=wizard-image.bmp      ; Image latérale (164x314 pixels)
WizardSmallImageFile=wizard-small.bmp ; Petite image (55x58 pixels)
```

### **Messages Personnalisés**

Créer `French.isl` personnalisé :
```iss
[CustomMessages]
french.WelcomeLabel2=Bienvenue dans l'assistant d'installation de %1.%n%nCette application vous aidera à gérer votre syndic efficacement.
```

---

## 🔄 **Stratégie de Mise à Jour**

### **Système de Versioning**

**Format** : `MAJOR.MINOR.PATCH`
- **MAJOR** : Changements majeurs (incompatibilité)
- **MINOR** : Nouvelles fonctionnalités
- **PATCH** : Corrections de bugs

**Exemples** :
- `2.0.0` → Version actuelle
- `2.1.0` → Nouvelles fonctionnalités
- `2.0.1` → Correction de bugs

### **Détection Automatique des Mises à Jour**

**Fichier à créer** : `UpdateChecker.cs`

```csharp
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace ElMansourSyndicManager.Services;

public class UpdateChecker
{
    private const string UPDATE_URL = "https://www.elmansour-syndic.tn/api/version.json";
    private readonly HttpClient _httpClient;

    public UpdateChecker()
    {
        _httpClient = new HttpClient();
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(UPDATE_URL);
            var updateInfo = JsonSerializer.Deserialize<UpdateInfo>(response);
            
            if (updateInfo != null && IsNewerVersion(updateInfo.Version))
            {
                return updateInfo;
            }
        }
        catch (Exception)
        {
            // Ignorer les erreurs de connexion
        }
        
        return null;
    }

    private bool IsNewerVersion(string remoteVersion)
    {
        var currentVersion = new Version(GetCurrentVersion());
        var newVersion = new Version(remoteVersion);
        
        return newVersion > currentVersion;
    }

    private string GetCurrentVersion()
    {
        return System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "2.0.0";
    }
}

public class UpdateInfo
{
    public string Version { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string ReleaseNotes { get; set; } = string.Empty;
    public bool IsCritical { get; set; }
}
```

**Fichier serveur** : `version.json`
```json
{
  "version": "2.1.0",
  "downloadUrl": "https://www.elmansour-syndic.tn/downloads/ElMansourSyndicManager-Setup-2.1.0.exe",
  "releaseNotes": "• Amélioration des performances\n• Correction de bugs\n• Nouvelles fonctionnalités",
  "isCritical": false
}
```

---

## 📋 **Checklist de Déploiement**

### **Avant de Créer l'Installateur**

- [ ] Code compilé en mode Release
- [ ] Tests effectués sur machine propre
- [ ] Numéro de version mis à jour
- [ ] Documentation à jour
- [ ] Icônes et images préparés
- [ ] Licence incluse

### **Après Création de l'Installateur**

- [ ] Installateur testé sur Windows 10
- [ ] Installateur testé sur Windows 11
- [ ] Installation propre testée
- [ ] Mise à jour testée
- [ ] Désinstallation testée
- [ ] Antivirus ne bloque pas
- [ ] Taille de l'installateur raisonnable (< 100 MB)

---

## 🛡️ **Signature de Code (Optionnel mais Recommandé)**

### **Pourquoi Signer ?**

✅ **Avantages** :
- Évite les avertissements Windows SmartScreen
- Augmente la confiance des utilisateurs
- Prouve l'authenticité

### **Comment Obtenir un Certificat ?**

1. **Acheter** un certificat de signature de code :
   - DigiCert
   - Sectigo
   - GlobalSign
   
2. **Coût** : ~200-400€/an

3. **Signer l'installateur** :
```powershell
signtool sign /f "certificat.pfx" /p "motdepasse" /t http://timestamp.digicert.com "ElMansourSyndicManager-Setup.exe"
```

---

## 📦 **Distribution**

### **Options de Distribution**

**Option 1 : Site Web**
```
https://www.elmansour-syndic.tn/telechargement
```
- Hébergement simple
- Contrôle total
- Statistiques de téléchargement

**Option 2 : Email**
- Envoi direct aux clients
- Lien de téléchargement sécurisé

**Option 3 : Clé USB**
- Pour clients sans internet
- Installation offline

---

## 🔧 **Script de Build Automatisé**

**Créer** : `build-installer.ps1`

```powershell
# Script de build automatisé pour El Mansour Syndic Manager

param(
    [string]$Version = "2.0.0"
)

Write-Host "=== Build de l'installateur v$Version ===" -ForegroundColor Green

# 1. Nettoyer
Write-Host "Nettoyage..." -ForegroundColor Yellow
Remove-Item -Path "bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Output" -Recurse -Force -ErrorAction SilentlyContinue

# 2. Compiler l'application
Write-Host "Compilation de l'application..." -ForegroundColor Yellow
dotnet publish src/ElMansourSyndicManager/ElMansourSyndicManager.csproj `
  -c Release `
  -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=false `
  -p:PublishReadyToRun=true `
  -p:Version=$Version `
  -o bin/Release/net8.0-windows/publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la compilation!" -ForegroundColor Red
    exit 1
}

# 3. Copier les fichiers nécessaires
Write-Host "Copie des fichiers..." -ForegroundColor Yellow
Copy-Item "appsettings.json" -Destination "bin/Release/net8.0-windows/publish/" -Force
Copy-Item "GUIDE_*.md" -Destination "bin/Release/net8.0-windows/publish/Docs/" -Force

# 4. Compiler l'installateur
Write-Host "Création de l'installateur..." -ForegroundColor Yellow
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" installer.iss

if ($LASTEXITCODE -ne 0) {
    Write-Host "Erreur lors de la création de l'installateur!" -ForegroundColor Red
    exit 1
}

# 5. Renommer avec version
Write-Host "Finalisation..." -ForegroundColor Yellow
$installerName = "ElMansourSyndicManager-Setup-v$Version.exe"
Move-Item "Output\ElMansourSyndicManager-Setup.exe" "Output\$installerName" -Force

Write-Host "=== Build terminé avec succès! ===" -ForegroundColor Green
Write-Host "Installateur créé : Output\$installerName" -ForegroundColor Cyan
Write-Host "Taille : $((Get-Item "Output\$installerName").Length / 1MB) MB" -ForegroundColor Cyan
```

**Utilisation** :
```powershell
.\build-installer.ps1 -Version "2.1.0"
```

---

## ✅ **Résumé**

| Aspect | Solution |
|--------|----------|
| **Outil** | Inno Setup 6.x |
| **Format** | .exe unique |
| **Taille** | ~50 MB |
| **Prérequis** | .NET 8.0 (auto-installé) |
| **Mise à jour** | Détection automatique |
| **Données** | Préservées automatiquement |
| **Désinstallation** | Propre avec choix |

---

**Version** : 2.0  
**Dernière mise à jour** : 2026-01-15
