# RecycleItemsIntoParts — 1.8.9 startup crash fix

## Goal
- Fix ArgumentNullException in CacheGameApi / Harmony FieldRefAccess on load.

## Status
- Completed

## Cause
- Harmony FieldRefAccess throws if the typed field lookup returns null. That ran in Awake and aborted the plugin.

## Fix
- Cache FieldInfo with AccessTools.Field (same approach as 1.8.7) instead of FieldRefAccess.
- Missing fields skip instead of crashing.

## Build
- Release build succeeded (0 errors).
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\RecycleItemsIntoParts\bin\Release\net480\RecycleItemsIntoParts.dll`
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\dist\cjayride-RecycleItemsIntoParts-1.8.9.zip`

## Next
- Replace 1.8.8 with 1.8.9 and confirm BepInEx loads the plugin.
