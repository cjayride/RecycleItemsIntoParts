# RecycleItemsIntoParts

Patched for Valheim v0.217.46+

A fork of [aedenthorn/DiscardInventoryItem](https://github.com/aedenthorn/ValheimMods/tree/master/DiscardInventoryItem) 

[ThunderStore](https://thunderstore.io/c/valheim/p/cjayride/RecycleItemsIntoParts) | [GitHub](https://github.com/cjayride/RecycleItemsIntoParts)

# This mod recycles items into parts

- Break down (recycle) items on the fly, by drag clicking an item and clicking [Delete]

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

# Recycle EpicLoot Enchanted Gear for Magic Parts | DefaultSetting = true

> ReturnEnchantedResources = true

CJAYCRAFT PLAYERS - This setting must be: true

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

# Contact

- Twitter: twitter.com/cjayride

- Discord: discord.gg/cjayride (find me at the top of the user list) "cjayride"

- Twitch: twitch.tv/cjayride


