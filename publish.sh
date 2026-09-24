#!/bin/bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

DATE_VERSION="$(date '+%Y.%m.%d')"

read -rp "Build version minor [$DATE_VERSION-0]: " MINOR
MINOR="${MINOR:-0}"

VERSION="${DATE_VERSION}-${MINOR}"

echo "Building Btd700Ctl version $VERSION"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

BUILD_DIR="$SCRIPT_DIR/build/native"
PUBLISH_DIR="$SCRIPT_DIR/dist/btd700ctl-gui"

echo "=== Initializing submodules ==="

git -C "$SCRIPT_DIR" submodule update --init --recursive

echo "=== Cleaning previous publish ==="

rm -rf "$PUBLISH_DIR"
mkdir -p "$PUBLISH_DIR"

echo "=== Building C library ==="

cmake \
    -B "$BUILD_DIR" \
    -S "$SCRIPT_DIR/vendor/btd700ctl" \
    -DCMAKE_BUILD_TYPE=Release \
    -DBUILD_SHARED_LIBS=ON

cmake \
    --build "$BUILD_DIR" \
    --target btd700ctl \
    --config Release

echo "=== Publishing GUI ==="

dotnet publish \
    "$SCRIPT_DIR/src/Btd700Ctl.Gui/Btd700Ctl.Gui.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained true \
    -p:Version="$VERSION" \
    -p:AssemblyVersion="1.0.0.0" \
    -p:FileVersion="$VERSION" \
    -p:InformationalVersion="$VERSION" \
    -o "$PUBLISH_DIR"

echo "=== Copying native library ==="

cp -P "$BUILD_DIR"/libbtd700ctl.so* "$PUBLISH_DIR/"

echo "=== Publish complete ==="
echo
echo "Application:"
echo "  $PUBLISH_DIR/btd700ctl-gui"