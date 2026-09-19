#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="$SCRIPT_DIR/build/native"
OUTPUT_DIR="$SCRIPT_DIR/src/Btd700Ctl.Gui/bin/Release/net8.0"

echo "=== Initializing submodules ==="
git -C "$SCRIPT_DIR" submodule update --init --recursive

echo "=== Building C library ==="
cmake -B "$BUILD_DIR" \
    -S "$SCRIPT_DIR/vendor/btd700ctl" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON
cmake --build "$BUILD_DIR" --target btd700ctl --config Release

echo "=== Building Interop project ==="
dotnet build "$SCRIPT_DIR/src/Btd700Ctl.Interop/Btd700Ctl.Interop.csproj" -c Release

echo "=== Building GUI project ==="
dotnet build "$SCRIPT_DIR/src/Btd700Ctl.Gui/Btd700Ctl.Gui.csproj" -c Release

echo "=== Copying native library ==="
cp -P "$BUILD_DIR"/libbtd700ctl.so* "$OUTPUT_DIR/" 2>/dev/null || cp "$BUILD_DIR"/libbtd700ctl.so "$OUTPUT_DIR/"

echo "=== Build complete ==="
echo "Run: $OUTPUT_DIR/btd700ctl-gui"
