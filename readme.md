## BTD700 Control GUI

Aims to be a desktop app for the Sennheiser BTD700 on Linux.
Uses [btd700ctl](https://github.com/sobalap/btd700ctl/) as communication backend.

### Features

- Switching between Standard (HQ) and Gaming mode (Broadcast mode is currently a bit wacky and wip)
- Switching between supported codecs
- Viewing HID device details (Firmware, Serial Number, Manufacturer)

### Setting up non-root access

Please follow the instructions in the [btd700ctl README](vendor/btd700ctl/README.md) to set up udev rules.

### Building & Running

> [!NOTE]  
> Dotnet version 8 and cmake is required to build this project.

Run `build.sh` to build the project. This script will compile the source code and generate the necessary binaries.
The buildscript will build the release config by default.

If Cmake fails, clean the build dir: `rm -rf build/native` and try again.

Run the compiled program using `./src/Btd700Ctl.Gui/bin/Release/net8.0/Btd700Ctl.Gui`

### Credits

- Avalonia UI framework
- btd700ctl (found in vendor/ as submodule)

### AI disclosure

Qwen3.6-27B was used for some tasks, running on my local GPU. 

Specifically for the Interop (P/Invoke) code generation, which would have been pretty tedious by hand.

This was the first real application for my llamacpp setup, if you wanna set it up yourself, check out my guide: [Setting up llamacpp the proper way](https://www.sam-ko.eu/artificial-intelligence/infrastructure-and-apps/setting-up-a-llama.cpp-server-the-proper-way.html)