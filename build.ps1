param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$BuildDir = Join-Path $ScriptDir "build\native"
$OutputDir = Join-Path $ScriptDir "src\Btd700Ctl.Gui\bin\$Configuration\net8.0"

Write-Host "=== Initializing submodules ==="
git -C $ScriptDir submodule update --init --recursive

Write-Host "=== Building C library ==="
cmake -B $BuildDir `
    -S (Join-Path $ScriptDir "vendor\btd700ctl") `
    -DCMAKE_BUILD_TYPE=$Configuration `
    -DBUILD_SHARED_LIBS=ON
cmake --build $BuildDir --target btd700ctl --config $Configuration

Write-Host "=== Building Interop project ==="
dotnet build (Join-Path $ScriptDir "src\Btd700Ctl.Interop\Btd700Ctl.Interop.csproj") -c $Configuration

Write-Host "=== Building GUI project ==="
dotnet build (Join-Path $ScriptDir "src\Btd700Ctl.Gui\Btd700Ctl.Gui.csproj") -c $Configuration

Write-Host "=== Copying native library ==="
$LibSrc = Join-Path $BuildDir "$Configuration\btd700ctl.dll"
Copy-Item $LibSrc $OutputDir -Force

Write-Host "=== Build complete ==="
Write-Host "Run: $OutputDir\btd700ctl-gui.exe"
