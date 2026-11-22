# RecycleItemsIntoParts — 1.8.12 skip undiscovered mats

## Goal
- Recycle must not return materials the player has never discovered (`Player.IsKnownMaterial`).
- Toggle: `ReturnUndiscoveredMaterials` (default false).

## Status
- Built (not in-game tested).

## Changes
- QueueRequirement skips unknown mats when the toggle is false (craft + enchant returns).
- Pack client/server cfg + manifests 4.4.66 depend on 1.8.12.

## Build
- `dotnet build` Release: 0 errors
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\RecycleItemsIntoParts\bin\Release\net480\RecycleItemsIntoParts.dll` FileVersion 1.8.12
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\dist\cjayride-RecycleItemsIntoParts-1.8.12.zip` (flat root)

## Next
- In-game: recycle a Flametal-recipe weapon before discovering Flametal — confirm popup should omit Flametal; known mats still return.
