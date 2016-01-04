@echo off

set OUT_PATH=..\..\Project\Assets\Code\Proto
set PROTO_PATH=ProtoFile
set PROTO_T_PATH=Proto_t\

copy /y %PROTO_T_PATH%common.xslt %PROTO_PATH%
copy /y %PROTO_T_PATH%csharp.xslt %PROTO_PATH%
copy /y %PROTO_T_PATH%protobuf-net.dll %PROTO_PATH%
copy /y %PROTO_T_PATH%protogen.exe %PROTO_PATH%

cd %PROTO_PATH%

rem create file 
if not exist %OUT_PATH% md %OUT_PATH%

for /f "delims=\" %%a in ('dir /b /a-d /o-d "*.proto"') do ProtoGen.exe -i:%%a -o:%OUT_PATH%\%%~na_proto.cs -ns:ProtoBuf




del common.xslt
del csharp.xslt
del protobuf-net.dll
del protogen.exe

pause