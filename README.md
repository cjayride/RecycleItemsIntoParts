# RecycleItemsIntoParts

Updated for Valheim 1.0

A fork of [aedenthorn/DiscardInventoryItem](https://github.com/aedenthorn/ValheimMods/tree/master/DiscardInventoryItem) 

[ThunderStore](https://thunderstore.io/c/valheim/p/cjayride/RecycleItemsIntoParts) | [GitHub](https://github.com/cjayride/RecycleItemsIntoParts)

# This mod recycles items into parts

- Drag an item, then press [Delete]
- A Yes/No popup lists the item and the parts you will get; Yes recycles, No cancels

- Control the percentage (%) of parts returned (in the config)

- Control the types of parts to be recycled: 

**Enchanting Parts [EpicLoot]**

**ShardMagic [EpicLoot]**

**Coins**

**Consumables**

**Trophies** 

# Setup / Configuration

Setup has completely changed, now that all mods have been updated for Mistlands. Check the values in the config file.

# Edit the config file

> BepInEx/config/cjayride.RecycleItemsIntoParts.cfg

Launch the game once to generate the config file and review the options.

# Server enforced | DefaultSetting = true

> ServerEnforced = true

When true, clients use the **server/host** recycle rules (return percent, coins, trophies, enchant downgrade, etc.). Players cannot override those values while connected.

When false, each player uses their own config.

The recycle hotkey is never server-enforced.

CJAYCRAFT PLAYERS - This setting must be: true

# Craft material return | DefaultSetting = 0.40

> ReturnResources = 0.40

40% of the craft recipe, rounded. A 1-wood item returns **no wood** (0.40 rounds to 0). A 3-wood item returns 1 wood.

# EpicLoot enchant return | DefaultSetting = 0.40

> ReturnResourcesMagic = 0.40

After the rarity drop, enchant mats are scaled by this value. 5 Magic Dust at 0.40 returns 2.

# Recycle EpicLoot Enchanted Gear for Magic Parts | DefaultSetting = true

> ReturnEnchantedResources = true

CJAYCRAFT PLAYERS - This setting must be: true

Enchant returns are **one rarity lower** than the item (`DowngradeEnchantRarity = true`). A legendary sledge returns epic runestone/dust/essence, not legendary.

Green Magic items return 1-2 Magic Dust when `MagicDustSoftener = true`.

# Give Back Coins? | DefaultSetting = false

When recycling parts, don't give Coins (enable/disable).

It cost Coins to enchant with Epic Loot, so if you recycle an item it will usually return Coins. Some servers and players may not want this. Items gave back too many coins and it messed up our server economy.

> RecycleCoins = false

CJAYCRAFT PLAYERS - This setting must be: false

# Give Back Trophies | DefaultSetting = True

Don't want to give back Trophies when recycling?

On our server, we didn't want people to get back Trophies, much like we don't want to give back Coins, because it affects the economy of our server. Items can still be recycled, but if it contains a Trophy, it just won't give back the Trophy.

By default, the mod WILL give Trophies.

> RecycleTrophy = true

CJAYCRAFT PLAYERS - This setting must be: false

# Allow Recycle Consumables | DefaultSetting = True

Don't allow recycling of a food TYPE item.

On our server, we didn't want people recycling food because it gave certain items that might affect the economy. Potentially, if an item contains a recipe part that is food, that food item would be returned. This only specifically targets the item that is being recycled. If the item is food, it can't be recycled.

By default, this mod WILL allow you to recycle consumables. 

> RecycleConsumables = true

CJAYCRAFT PLAYERS - This setting must be: RecycleConsumables = false

# Allow  Recycle ShardMagic [EpicLoot] | DefaultSetting = True

Don't allow recycling of the item ShardMagic [EpicLoot].

On our server, we didn't want people recycling the green rarity Magic Shards (Epic Loot) because it interferes with some recipes we're using.

By default, this mod WILL allow you to recycle ShardMagic [EpicLoot]. 

> RecycleShards = true

CJAYCRAFT PLAYERS - This setting must be: false

# Confirm recycle | DefaultSetting = true

> RequireConfirm = true

First [Delete] opens a **Yes / No** window over the inventory (not the full-screen pause popup). It names the item and lists the parts you will get. Yes recycles; No, Escape, or right-click cancels.

# Contact

- 𝕏: x.com/cjayride

- Discord: discord.gg/cjayride (find me at the top of the user list) "cjayride"

- Twitch: twitch.tv/cjayride

# AI Generated

This code was not AI Generated, however, AI was used to verify that it works with the new version of the game.


