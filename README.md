# BonkWithFriends

A multiplayer mod for **Megabonk** built on MelonLoader. Adds co-op gameplay via Steam networking.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Megabonk** with **MelonLoader** installed
- `Megabonk_Multiplayer_Mod_v3.7z` archive

## Installation

### 1. Extract base mod files

Extract `Megabonk_Multiplayer_Mod_v3.7z` into the game's root directory (where the `.exe` is located):

```
Megabonk/
├── MelonLoader/
├── Mods/
├── UserLibs/
└── Megabonk.exe
```

### 2. Build from source

```bash
C:/dotnet8sdk/dotnet build D:/BonkWithFriends/src/Megabonk.BonkWithFriends.csproj -c Release
```

> **Note:** adjust the paths to `dotnet` and the project to match your setup.

The compiled DLL will appear in `src/bin/Release/net6.0/`.

### 3. Install the compiled mod

Copy `Megabonk.BonkWithFriends.dll` from the build output into the game's `Mods` folder:

```
Megabonk/Mods/Megabonk.BonkWithFriends.dll
```

## Project Structure

```
src/
├── Megabonk.BonkWithFriends/               # Core module
├── Megabonk.BonkWithFriends.Managers/       # Managers (enemies, items, player, server)
├── Megabonk.BonkWithFriends.Networking/     # Networking code and messages
├── Megabonk.BonkWithFriends.HarmonyPatches/ # Harmony patches for game logic
├── Megabonk.BonkWithFriends.UI/             # User interface
└── Megabonk.BonkWithFriends.csproj
```
