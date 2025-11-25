; Script d'installation pour El Mansour Syndic Manager
; Créé avec Inno Setup

#define MyAppName "El Mansour Syndic Manager"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Résidence El Mansour"
#define MyAppExeName "ElMansourSyndicManager.exe"
#define MyAppAssocName MyAppName + " File"
#define MyAppAssocExt ".emsm"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; Informations de base
AppId={{A1B2C3D4-E5F6-4A5B-8C9D-0E1F2A3B4C5D}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\ElMansourSyndicManager
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; Dossier de sortie
OutputDir=installer-output
OutputBaseFilename=ElMansourSyndicManager-Setup-v{#MyAppVersion}
; Compression
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes
; Apparence
WizardStyle=modern
; SetupIconFile=src\ElMansourSyndicManager\Assets\logo.ico
WizardSizePercent=120
; Privilèges
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; Architecture
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Désinstallation
UninstallDisplayIcon={app}\Assets\logo.png
UninstallDisplayName={#MyAppName}
; Informations de version
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Application de gestion de syndic
VersionInfoCopyright=Copyright (C) 2025 {#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le bureau"; GroupDescription: "Raccourcis supplémentaires:"; Flags: unchecked
Name: "quicklaunchicon"; Description: "Créer un raccourci dans la barre de lancement rapide"; GroupDescription: "Raccourcis supplémentaires:"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Fichiers de l'application
Source: "src\ElMansourSyndicManager\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Fichier de configuration (ne pas écraser si existe)
Source: "src\ElMansourSyndicManager\bin\Release\net8.0-windows\win-x64\publish\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist
; NOTE: Ne pas utiliser "Flags: ignoreversion" sur les fichiers système partagés

[Dirs]
; Créer les dossiers de données avec permissions complètes
Name: "{app}\data"; Permissions: users-full
Name: "{app}\data\Receipts"; Permissions: users-full
Name: "{app}\data\reports"; Permissions: users-full
Name: "{app}\data\backups"; Permissions: users-full
Name: "{app}\data\logs"; Permissions: users-full

[Icons]
; Raccourci dans le menu Démarrer
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\logo.png"; Comment: "Lancer {#MyAppName}"
Name: "{group}\Désinstaller {#MyAppName}"; Filename: "{uninstallexe}"; Comment: "Désinstaller {#MyAppName}"
; Raccourci sur le bureau (si l'utilisateur le choisit)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\logo.png"; Tasks: desktopicon; Comment: "Lancer {#MyAppName}"
; Raccourci barre de lancement rapide (si l'utilisateur le choisit)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\logo.png"; Tasks: quicklaunchicon

[Registry]
; Enregistrer l'application dans le registre
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}"; Flags: uninsdeletekeyifempty
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\Settings"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "Software\{#MyAppPublisher}\{#MyAppName}\Settings"; ValueType: string; ValueName: "Version"; ValueData: "{#MyAppVersion}"; Flags: uninsdeletekey

[Run]
; Proposer de lancer l'application après installation
Filename: "{app}\{#MyAppExeName}"; Description: "Lancer {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Nettoyer les fichiers temporaires lors de la désinstallation
Type: filesandordirs; Name: "{app}\data\logs"
Type: filesandordirs; Name: "{app}\data\temp"
; NOTE: Les données utilisateur (base de données, reçus, rapports) sont conservées
; L'utilisateur peut les supprimer manuellement si nécessaire

[Code]
// Vérifier si .NET 8 Runtime est installé
function IsDotNet8Installed: Boolean;
var
  ResultCode: Integer;
begin
  // Vérifier si dotnet.exe existe et peut exécuter --list-runtimes
  Result := Exec('dotnet.exe', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup: Boolean;
var
  ResultCode: Integer;
begin
  Result := True;
  
  // Vérifier .NET 8 Runtime
  if not IsDotNet8Installed then
  begin
    if MsgBox('Cette application nécessite .NET 8 Runtime Desktop.' + #13#10 + #13#10 +
              'Voulez-vous télécharger et installer .NET 8 Runtime maintenant?' + #13#10 + #13#10 +
              'Note: L''installation continuera après l''installation de .NET 8.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0/runtime', '', '', SW_SHOW, ewNoWait, ResultCode);
      Result := False; // Arrêter l'installation pour permettre l'installation de .NET
    end
    else
    begin
      MsgBox('L''installation ne peut pas continuer sans .NET 8 Runtime.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Créer la base de données initiale si elle n'existe pas
    // (L'application le fera au premier lancement)
  end;
end;
