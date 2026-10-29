# ♻️ RecycleItemsIntoParts

**Updated for Valheim 1.0.21**

Recycle unwanted items into crafting materials with a simple **Delete** key shortcut.

A fork of [DiscardInventoryItem by aedenthorn](https://github.com/aedenthorn/ValheimMods/tree/master/DiscardInventoryItem).

[Thunderstore](https://thunderstore.io/c/valheim/p/cjayride/RecycleItemsIntoParts/) · [GitHub](https://github.com/cjayride/RecycleItemsIntoParts)

---

## 🎮 How to Use

1. Select an item in your inventory.
2. Press **Delete**.
3. Review the materials you'll receive.
4. Click **Yes** to recycle or **No** to cancel.

Press **Escape** or right-click to cancel, too.

## ✨ What Can Be Recycled?

- 🪵 Crafting materials
- ✨ Epic Loot enchantment materials
- 💎 Epic Loot Magic Shards
- 🪙 Coins
- 🏆 Trophies
- 🍖 Consumables

Each category can be enabled or disabled in the configuration.

---

## ⚙️ Configuration

Launch the game once to generate the config file:

`BepInEx/config/cjayride.RecycleItemsIntoParts.cfg`

### 🖥️ Server Settings

```ini
ServerEnforced = true
```

**Default:** `true`

When enabled, everyone follows the server's recycling rules.

**CJAYCRAFT:** Must be `true`.

### 🪵 Resource Returns

```ini
ReturnResources = 0.40
ReturnResourcesMagic = 0.40
```

| Setting | Purpose |
|---|---|
| `ReturnResources` | Percentage of crafting materials returned. |
| `ReturnResourcesMagic` | Percentage of enchantment materials returned. |

**Default:** `0.40` = 40%

Returned quantities are rounded. For example, 3 Wood returns 1 Wood.

### ✨ Enchanted Gear

```ini
ReturnEnchantedResources = true
DowngradeEnchantRarity = true
MagicDustSoftener = true
```

- **ReturnEnchantedResources** — Recycle enchanted gear into magic materials.
- **DowngradeEnchantRarity** — Return materials one rarity tier below the item's rarity.
- **MagicDustSoftener** — Green magic items return 1–2 Magic Dust.

**CJAYCRAFT:** Keep `ReturnEnchantedResources = true`.

### 🚫 Optional Recycling Restrictions

Change these settings to control what players can recover.

| Setting | Default | Effect |
|---|---|---|
| `RecycleCoins` | `false` | Allow Coins to be returned. |
| `RecycleTrophy` | `true` | Allow Trophies to be returned. |
| `RecycleConsumables` | `true` | Allow food and consumables to be recycled. |
| `RecycleShards` | `true` | Allow Epic Loot Magic Shards to be recycled. |

**CJAYCRAFT server settings:**

```ini
RecycleCoins = false
RecycleTrophy = false
RecycleConsumables = false
RecycleShards = false
```

These restrictions help protect the server economy and custom recipes.

### ✅ Confirmation Popup

```ini
RequireConfirm = true
```

When enabled, the popup shows the item and the materials you'll receive before recycling.

**CJAYCRAFT:** Keep this enabled.

---

## 📦 Installation

1. Install BepInEx.
2. Install RecycleItemsIntoParts.
3. Launch Valheim to generate the config.
4. Adjust settings as needed.

For CJAYCRAFT, use the server settings listed above.

## 📬 Contact

- **X:** [@cjayride](https://x.com/cjayride)
- **Discord:** [discord.gg/cjayride](https://discord.gg/cjayride)
- **Twitch:** [twitch.tv/cjayride](https://twitch.tv/cjayride)

## 🤖 Development

The code was **not AI-generated**. AI was used to help verify compatibility with Valheim 1.0.