# RecycleItemsIntoParts — 1.8.11 skip disabled recipes

## Goal
- Recycle of uncraftable items (EpicLoot `m_enabled=false` recipes, e.g. Silver Ring / Winter Band) must not return craft mats (silver, tin, leftover coins). Magic items still return enchant materials.

## Status
- Completed (compile + zip). Not in-game tested.

## Changes
- TryRecycle: ignore recipes with `!m_enabled` for craft-mat returns; still return enchant mats
- EpicLoot leftover `SilverRing-recipe` set to `Silver,1,0` in pack cfgs (earlier)
- Version 1.8.11; pack manifests + changelogs

## Build
- `dotnet build` Release: 0 errors
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\RecycleItemsIntoParts\bin\Release\net480\RecycleItemsIntoParts.dll` FileVersion 1.8.11
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\dist\cjayride-RecycleItemsIntoParts-1.8.11.zip` (flat root)

## Next
- In-game recycle Winter Band: no silver/tin/coins; only downgraded enchant mats if magic
