# RecycleItemsIntoParts — Valheim 1.0.21 release

## Goal
- Verify Release build against Valheim 1.0.21, update BepInExPack to 5.4.2360, changelog, Thunderstore `/dist` zip, git tag `v1.8.7`.

## Status
- Completed (pending git push/tag)

## Changes
- Version 1.8.7: manifest BepInEx `denikson-BepInExPack_Valheim-5.4.2360`, changelog, README compatibility note.
- `dist/` staged for Thunderstore (DLL, manifest, README, CHANGELOG, icon); zip `cjayride-RecycleItemsIntoParts-1.8.7.zip`.

## Build
- `dotnet build RecycleItemsIntoParts.sln -c Release -p:GamePath=D:\SteamLibrary\steamapps\common\Valheim` — succeeded (0 errors, existing warnings).
- Valheim Steam buildid: 26249040 (1.0.21).
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\RecycleItemsIntoParts\bin\Release\net480\RecycleItemsIntoParts.dll`

## Next
- Commit, push, tag `v1.8.7` on GitHub.
