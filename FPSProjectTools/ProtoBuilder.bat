@echo off
SET PROTO_DIR=.\protos

for %%f in (%PROTO_DIR%\*.proto) do (
    ".\protoc\protoc.exe" --csharp_out=..\FPSProject\Assets\Scripts\Protos --grpc_out=..\FPSProject\Assets\Scripts\Protos --plugin=protoc-gen-grpc=".\protoc\grpc_csharp_plugin.exe" %%f
)

echo All proto files processed!
pause