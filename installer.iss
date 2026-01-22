; Script Inno Setup pour El Mansour Syndic Manager
; Version 2.0
; Créé le 2026-01-15

#define MyAppName "El Mansour Syndic Manager"
#define MyAppVersion "3.2.0"
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
DefaultDirName={autopf}\ElMansourSyndic
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=ElMansourSyndicManager-Setup-v{#MyAppVersion}
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
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Fichiers de l'application
Source: "bin\Publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Documentation
Source: "GUIDE_INSTALLATION_UTILISATEUR.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDE_CONFIGURATION.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDE_DEPLOIEMENT.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDE_CLOUD_SYNC.md"; DestDir: "{app}\Docs"; Flags: ignoreversion
Source: "GUIDE_SYNCHRONISATION_RESEAU.md"; DestDir: "{app}\Docs"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Documentation"; Filename: "{app}\Docs"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Lancer l'application après installation
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
// Vérification de .NET 8.0 Runtime
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
  Output: AnsiString;
begin
  Result := False;
  
  // Exécuter dotnet --list-runtimes pour vérifier
  if Exec('cmd.exe', '/c dotnet --list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    // Si la commande réussit, .NET est installé
    Result := True;
  end;
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  DotNetUrl: String;
begin
  Result := True;
  
  if not IsDotNetInstalled() then
  begin
    if MsgBox('.NET 8.0 Desktop Runtime est requis mais n''est pas installé.' + #13#10 + #13#10 + 
              'Voulez-vous ouvrir la page de téléchargement maintenant ?' + #13#10 + 
              '(L''installation continuera après l''installation de .NET)', 
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      DotNetUrl := 'https://dotnet.microsoft.com/download/dotnet/8.0';
      ShellExec('open', DotNetUrl, '', '', SW_SHOW, ewNoWait, ResultCode);
      
      MsgBox('Veuillez installer .NET 8.0 Desktop Runtime, puis relancer cet installateur.', 
             mbInformation, MB_OK);
      Result := False;
    end
    else
    begin
      MsgBox('L''installation ne peut pas continuer sans .NET 8.0 Runtime.', 
             mbError, MB_OK);
      Result := False;
    end;
  end;
end;

// Sauvegarde des données avant mise à jour
procedure CurStepChanged(CurStep: TSetupStep);
var
  DataPath: String;
  BackupPath: String;
  ResultCode: Integer;
begin
  if CurStep = ssInstall then
  begin
    // Vérifier si c'est une mise à jour
    if DirExists(ExpandConstant('{app}')) then
    begin
      DataPath := ExpandConstant('{localappdata}\ElMansourSyndic\data');
      
      if DirExists(DataPath) then
      begin
        if MsgBox('Une installation existante a été détectée.' + #13#10 + 
                  'Vos données seront automatiquement préservées.' + #13#10 + #13#10 + 
                  'Continuer la mise à jour ?', 
                  mbConfirmation, MB_YESNO) = IDNO then
        begin
          Abort;
        end;
      end;
    end;
  end;
end;

// Message après installation
procedure CurPageChanged(CurPageID: Integer);
begin
  if CurPageID = wpFinished then
  begin
    // Message de bienvenue
  end;
end;

// Nettoyage lors de la désinstallation
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataPath: String;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataPath := ExpandConstant('{localappdata}\ElMansourSyndic');
    
    if DirExists(DataPath) then
    begin
      if MsgBox('Voulez-vous également supprimer vos données ?' + #13#10 + 
                '(base de données, documents, sauvegardes)' + #13#10 + #13#10 + 
                'Si vous prévoyez de réinstaller l''application, choisissez Non.', 
                mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
      begin
        DelTree(DataPath, True, True, True);
        MsgBox('Toutes les données ont été supprimées.', mbInformation, MB_OK);
      end
      else
      begin
        MsgBox('Vos données ont été conservées dans :' + #13#10 + DataPath, 
               mbInformation, MB_OK);
      end;
    end;
  end;
end;
