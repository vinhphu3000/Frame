

set FolderName=ProtoFile
for /f "delims=\" %%a in ('dir /b /a-d /o-d "%FolderName%\*.proto*"') do echo %%~na
pause