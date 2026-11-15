# RecycleItemsIntoParts — 1.8.10 keyboard confirm

## Goal
- Enter confirms recycle Yes; Escape cancels No without closing inventory.

## Status
- Completed

## Changes
- TickPopupKeys: Return/KeypadEnter = Yes (OK on blocked popup); Escape/right-click = No
- InventoryGui.Hide prefix: Escape on the confirm window does not close inventory
- Version 1.8.10

## Build
- Release: 0 errors
- DLL: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\RecycleItemsIntoParts\bin\Release\net480\RecycleItemsIntoParts.dll` (FileVersion 1.8.10)
- Zip: `D:\AI-Agent-Projects\cjaycraft\Cjayride.RecycleItemsIntoParts\dist\cjayride-RecycleItemsIntoParts-1.8.10.zip` (flat: manifest.json, icon.png, README.md, CHANGELOG.md, RecycleItemsIntoParts.dll)

## Testing
- Not in-game. Compile succeeded.

## Next
- In-game: Delete item, Enter to recycle, Escape to cancel with inventory still open
