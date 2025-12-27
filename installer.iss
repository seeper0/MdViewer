; MdViewer Installer Script for Inno Setup
; 한국어 지원 포함

#define MyAppName "MdViewer"
#define MyAppVersion "0.2.1"
#define MyAppPublisher "seeper0"
#define MyAppURL "https://github.com/seeper0/MdViewer"
#define MyAppExeName "MdViewer.exe"

[Setup]
; 앱 기본 정보
AppId={{B8E5A8F0-1234-5678-9ABC-DEF012345678}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=D:\Project\MdViewer\installer-output
OutputBaseFilename=MdViewer-v{#MyAppVersion}-Setup
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

; 아이콘 및 이미지 (선택사항)
; SetupIconFile=icon.ico

; 권한 설정
PrivilegesRequired=lowest

; 언어
ShowLanguageDialog=auto

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Runtime-dependent 버전 (1MB)
Source: "D:\Project\MdViewer\MdViewer.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
; .md 파일 연결은 프로그램 첫 실행시 자동으로 처리됨

[Code]
// .NET 8 Runtime 체크 및 설치 안내
function IsDotNetInstalled(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
begin
  Result := True;

  if not IsDotNetInstalled() then
  begin
    if MsgBox('이 애플리케이션을 실행하려면 .NET 8 Desktop Runtime이 필요합니다.' + #13#10 + #13#10 +
              '지금 .NET 다운로드 페이지를 여시겠습니까?' + #13#10 + #13#10 +
              '"아니오"를 선택하면 설치를 계속하지만, 프로그램 실행을 위해 나중에 .NET을 설치해야 합니다.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/download/dotnet/8.0/runtime', '', '', SW_SHOW, ewNoWait, ResultCode);
    end;
  end;
end;
