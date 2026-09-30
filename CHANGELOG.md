# v1.8.5
- Recycle confirm list shows each returned item's icon next to its name
- Removed the hover-delete hint popup (it fired on every hover). Recycle is drag the item, then Delete
- EpicLoot enchanted gear still returns downgraded enchant materials when enabled

# v1.8.4
- Hover + Delete checks every inventory grid slot under the mouse in screen space (including extra inventory mods)

# v1.8.3
- Hover + Delete follows the item tooltip under the mouse
- If no item is held or hovered, Delete explains to drag the item first

# v1.8.2
- Hover + Delete uses the inventory slot under the mouse cursor (UI raycast)

# v1.8.1
- Yes / No are real colored buttons on the recycle window (not cloned hidden inventory buttons)
- Hover + Delete uses the slot the pointer entered, not only a dragged item

# v1.8.0
- Recycle Yes/No is a small inventory window (not the full-screen Valheim pause popup)
- Yes and No buttons work; Escape or right-click cancels

# v1.7.9
- Recycle confirmation is a Yes/No popup on top of the inventory, listing the item and parts you will get
- Hover now uses the inventory cell under the mouse (no longer recycles slot 1 by mistake)

# v1.7.8
- Recycle preview and results show in the center HUD as one line (item + returned parts)
- Optional top-left notifications: `UseTopLeftNotifications = false` by default
- Hover + Delete uses the hovered inventory slot (not only a dragged item)
- RecycleHotkey is read as Unity KeyCode `Delete` (stops BepInEx 'delete' keybind errors)

# v1.7.7
- Recycle works on a hovered item or a dragged item (Delete no longer requires a drag)
- First Delete shows a confirmation with the item name and returned parts; press Delete again within 4 seconds to confirm
- HUD messages list what is being recycled and what you get back
- RecycleHotkey uses KeyCode.Delete so it registers in Valheim 1.0 / Unity 6

# v1.7.6
- Added togglable server-enforced config (`ServerEnforced = true` by default). Host/server recycle settings apply to clients; set false for per-player configs. Recycle hotkey stays local.

# v1.7.5
- Enchant materials are no longer doubled on upgraded items (quality loop)
- Enchant returns drop one EpicLoot rarity (Legendary -> Epic). Magic rarity returns none
- Default craft return is 25%. Amounts under 0.5 round to nothing, so 1 bronze returns 0 bronze

# v1.7.4
- Thunderstore version bump (1.7.3 was already published)

# v1.7.3
- Fixed enchanted gear recycling dropping the first craft material (for example a boar skirt returning essences but no hides) when coins were also in the EpicLoot enchant cost

# v1.7.2
- Recycling no longer returns Valheim 1.0 upgrade idols (for example Wooden Protection Idol)

# v1.7.1
- Fixed NullReferenceException when recycling (ObjectDB.m_items can contain objects without ItemDrop in Valheim 1.0)

# v1.7.0
- Updated for Valheim 1.0
- Fixed Inventory.AddItem for the 1.0 signature (avoids MissingMethodException when returning recycled parts)
- Trophy recycle option now matches ItemType.Trophy
- Shard recycle option now uses RecycleShards instead of RecycleTrophy when deciding whether to delete shards
- Updated BepInExPack dependency to 5.4.2350

# v1.6.0
- Removed terminal reload
- Added compatibility for v0.217.46

# v1.5.1 - 2024/4/7
- Fixed reference to assembly files

# v1.5.0 - 2024/4/7
- Change for EpicLoot, committed by blradlof (verfied by "aedenthorn") on Dec 16, 2023. Line 86 changed EpicLoot.Crafting.EnchantTabController to EpicLoot.Crafting.EnchantHelper

# v1.4.0 - 2023/7/9
- Update for Mistlands. I've tested it with a combination of mods like EpicLoot, EquipmentandQuickSlots, ExtendedPlayerInventory, BetterArchery. 
- Note: The BetterArchery QUIVER (if enabled) breaks the game if ExtendedPlayerInventor is also installed.
- I'll continue to test as I complete the rebuild of CJAYCRAFT modpack. As far as I can tell, this mod is working, but shoot me a message if you discover an issue before I get to play more :)