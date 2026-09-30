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