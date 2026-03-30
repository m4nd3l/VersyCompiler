[Setup]
AppName=Versy Compiler
AppVersion=0.0.1
DefaultDirName={commonpf}\VersyCompiler
DefaultGroupName=Versy Compiler
OutputBaseFilename=VersySetup
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
; Questo serve per i permessi di scrittura nel registro per il PATH
PrivilegesRequired=admin 

[Files]
; Puntiamo alla cartella che hai estratto/generato
Source: ".\Versy 0.0.1\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: modifypath; Description: "Add Versy to PATH (highly suggested)"; Flags: unchecked

[Code]
const
    PathKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';

procedure CurStepChanged(CurStep: TSetupStep);
var
    OldPath: String;
    NewPath: String;
begin
    if (CurStep = ssPostInstall) and IsTaskSelected('modifypath') then
    begin
        if RegQueryStringValue(HKEY_LOCAL_MACHINE, PathKey, 'Path', OldPath) then
        begin
            // Controlla se il percorso è già presente per non duplicarlo
            if Pos(ExpandConstant('{app}'), OldPath) = 0 then
            begin
                NewPath := OldPath + ';' + ExpandConstant('{app}');
                RegWriteStringValue(HKEY_LOCAL_MACHINE, PathKey, 'Path', NewPath);
            end;
        end;
    end;
end;