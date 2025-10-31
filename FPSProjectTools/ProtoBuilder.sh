#!/bin/bash

PROTO_DIR=./protos
OUT_DIR=../Count_Sheap/Assets/Scripts/Protos
PROTOC_BIN=./protoc/mac/bin/protoc
#GRPC_PLUGIN=./protoc/grpc_csharp_plugin

for file in "$PROTO_DIR"/*.proto; do
  "$PROTOC_BIN" \
    --csharp_out="$OUT_DIR" \
    --grpc_out="$OUT_DIR" \
#    --plugin=protoc-gen-grpc="$GRPC_PLUGIN" \
    "$file"
done

echo "All proto files processed!"