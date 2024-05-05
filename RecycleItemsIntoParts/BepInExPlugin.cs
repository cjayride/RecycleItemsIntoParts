using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DiscardInventoryItem
{
    [BepInPlugin("cjayride.RecycleItemsIntoParts", "Recycle Items Into Parts", "1.6.0")]
    public class BepInExPlugin: BaseUnityPlugin
    {
        private static readonly bool isDebug = true;

        public static ConfigEntry<string> hotKey;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> returnUnknownResources;
        public static ConfigEntry<bool> returnEnchantedResources;
        public static ConfigEntry<float> returnResources;
        private static BepInExPlugin context;
        private static Assembly epicLootAssembly;

        // added by cjayride
        public static ConfigEntry<bool> recycleCoins;
        public static ConfigEntry<bool> betterArchery;
        public static ConfigEntry<int> betterArcheryCount;
        public static ConfigEntry<bool> equipmentAndQuickSlots;
        public static ConfigEntry<int> equipmentAndQuickSlotsCount;
        public static ConfigEntry<int> inventoryRows;
        public static ConfigEntry<bool> recycleConsumables;
        public static ConfigEntry<bool> recycleTrophy;
        public static ConfigEntry<bool> recycleShards;

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        private void Awake()
        {
            context = this;

            hotKey = Config.Bind<string>("General", "RecycleHotkey", "delete", "The hotkey to recycle an item");
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            returnUnknownResources = Config.Bind<bool>("General", "ReturnUnknownResources", true, "Return resources if recipe is unknown");
            returnEnchantedResources = Config.Bind<bool>("General", "ReturnEnchantedResources", true, "Return resources for Epic Loot enchantments");
            returnResources = Config.Bind<float>("General", "ReturnResources", (float)1.0, "Fraction of resources to return (0.0 - 1.0)");

            // added by cjayride
            recycleCoins = Config.Bind<bool>("General", "RecycleCoins", false, "Enable/disable coins on recycling items"); // enchanted items will recycle with coins, in some cases we don't want that because it messes with the economy
            //betterArchery = Config.Bind<bool>("General", "UsingBetterArchery", false, "Set to 'true' if you're using the Better Archery mod");
            //betterArcheryCount = Config.Bind<int>("General", "BetterArcheryInventoryCount", 16, "Number of extra slots added by Better Archery to use in calculation (inventory count fix) (Don't change this unless you know what you're doing.)");
            //inventoryRows = Config.Bind<int>("General", "InventoryRows", 4, "The number of inventory rows you're playing with will affect calculations. Default is 4. Some mods add more rows, so you need to change this here for the mod to work with BetterArchery. BetterArchery quiver slots are 2 rows below your last inventory row.");
            recycleConsumables = Config.Bind<bool>("General", "RecycleConsumables", true, "Enable/disable recycling of consumables (like food) (need to disable for cjaycraft ultimate modpack)");
            recycleTrophy = Config.Bind<bool>("General", "RecycleTrophy", true, "Enable/disable recycling of Trophy items (need to disable for cjaycraft ultimate modpack)");
            recycleShards = Config.Bind<bool>("General", "RecycleShards", true, "Enable/disable recycling of Shards (need to disable for cjaycraft ultimate modpack)");


            if (!modEnabled.Value)
                return;

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        private void Start()
        {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot"))
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;

        }

        [HarmonyPatch(typeof(InventoryGui), "UpdateItemDrag")]
        static class UpdateItemDrag_Patch
        {
            static void Postfix(InventoryGui __instance, ItemDrop.ItemData ___m_dragItem, Inventory ___m_dragInventory, int ___m_dragAmount, ref GameObject ___m_dragGo) {
                if (!modEnabled.Value || !Input.GetKeyDown(hotKey.Value) || ___m_dragItem == null || !___m_dragInventory.ContainsItem(___m_dragItem))
                    return;

                Dbgl($"Discarding {___m_dragAmount}/{___m_dragItem.m_stack} {___m_dragItem.m_dropPrefab.name}");

                // added by cjayride
                // check if inventory will be too full and if recycling is successful
                //bool fullInventory = false;
                bool foundConsumable = false;
                bool foundShards = false;


                //Dbgl("############## if (returnResources.Value > 0))  ##############");

                if (returnResources.Value > 0) {

                    //Dbgl("############## 1111111111111111111 ##############");


                    Recipe recipe = ObjectDB.instance.GetRecipe(___m_dragItem);

                    //Dbgl("returnUnknownResources.Value " + returnUnknownResources.Value);
                    //Dbgl("Player.m_localPlayer.IsRecipeKnown(___m_dragItem.m_shared.m_name " + Player.m_localPlayer.IsRecipeKnown(___m_dragItem.m_shared.m_name));

                    if (recipe != null && (returnUnknownResources.Value || Player.m_localPlayer.IsRecipeKnown(___m_dragItem.m_shared.m_name))) {
                        Dbgl($"Recipe stack: {recipe.m_amount} num of stacks: {___m_dragAmount / recipe.m_amount}");


                        var reqs = recipe.m_resources.ToList();



                       //Dbgl("ITEM TYPE DETECTION");


                       //Dbgl("ITEM TYPE DETECTION " + ObjectDB.instance.GetRecipe(___m_dragItem).m_item.m_itemData.m_shared.m_itemType.ToString());
                        // added by cjayride
                        if (ObjectDB.instance.GetRecipe(___m_dragItem).m_item.m_itemData.m_shared.m_name.ToString() == "Magic $mod_epicloot_assets_shard") {
                            foundShards = true;
                            //Dbgl("############## foundShards = true; ##############");
                        }

                        if (ObjectDB.instance.GetRecipe(___m_dragItem).m_item.m_itemData.m_shared.m_itemType.ToString() == "Consumable") {
                            foundConsumable = true;
                            //Dbgl("############## foundConsumable = true; ##############");
                        }

                        // added by cjayride
                        if (foundShards && !recycleShards.Value) {
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Cannot recycle shards.");
                        } else {

                            if (foundConsumable && !recycleConsumables.Value) {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Cannot recycle consumables.");
                            } else {

                                bool isMagic = false;
                                bool cancel = false;
                                if (epicLootAssembly != null && returnEnchantedResources.Value) {
                                    isMagic = (bool)epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("IsMagic", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ItemDrop.ItemData) }, null).Invoke(null, new[] { ___m_dragItem });
                                }
                                if (isMagic) {
                                    int rarity = (int)epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("GetRarity", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { ___m_dragItem });
                                    List<KeyValuePair<ItemDrop, int>> magicReqs = (List<KeyValuePair<ItemDrop, int>>)epicLootAssembly.GetType("EpicLoot.Crafting.EnchantHelper").GetMethod("GetEnchantCosts", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { ___m_dragItem, rarity });
                                    foreach (var kvp in magicReqs) {
                                        if (!returnUnknownResources.Value && ((ObjectDB.instance.GetRecipe(kvp.Key.m_itemData) && !Player.m_localPlayer.IsRecipeKnown(kvp.Key.m_itemData.m_shared.m_name)) || !Traverse.Create(Player.m_localPlayer).Field("m_knownMaterial").GetValue<HashSet<string>>().Contains(kvp.Key.m_itemData.m_shared.m_name))) {
                                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You don't know all the recipes for this item's materials.");
                                            return;
                                        }
                                        reqs.Add(new Piece.Requirement() {
                                            m_amount = kvp.Value,
                                            m_resItem = kvp.Key
                                        });
                                    }
                                }

                                //Dbgl("############## CHECKING RECIPE AMOUNT ############");

                                if (!cancel && ___m_dragAmount / recipe.m_amount > 0) {

                                    // START ---------------------------------------------> 
                                    // added by cjayride
                                    int indexCount = 0;
                                    int numToRemove = 0;
                                    bool removeIt = false;

                                    //Dbgl("STARTING");
                                    foreach (Piece.Requirement req in reqs) {

                                        //Dbgl("### req.m_resItem.m_itemData.m_shared.m_name " + req.m_resItem.m_itemData.m_shared.m_name + " ###");
                                        //Dbgl("### recycleCoins.Value " + recycleCoins.Value + " ###");

                                        if (req.m_resItem.m_itemData.m_shared.m_name == "$item_coins" && !recycleCoins.Value) {
                                            //Dbgl("IN THE LOOP");
                                            numToRemove = indexCount;
                                            removeIt = true;
                                            break;
                                        }

                                        //Dbgl("##############");
                                        //Dbgl(req.m_resItem.m_itemData.m_shared.m_itemType.ToString());
                                        //Dbgl("##############");


                                        if (req.m_resItem.m_itemData.m_shared.m_itemType.ToString() == "Trophies" && !recycleTrophy.Value) {
                                            //Dbgl("IN THE LOOP");
                                            numToRemove = indexCount;
                                            removeIt = true;
                                            break;
                                        }
                                    }
                                    //Dbgl("ENDING");
                                    if (removeIt)
                                        reqs.RemoveAt(numToRemove);
                                    //Dbgl("GG");
                                    // <-------------------------------------------- END


                                    for (int i = 0; i < ___m_dragAmount / recipe.m_amount; i++) {
                                        foreach (Piece.Requirement req in reqs) {
                                            int quality = ___m_dragItem.m_quality;
                                            for (int j = quality; j > 0; j--) {
                                                GameObject prefab = ObjectDB.instance.m_items.FirstOrDefault(item => item.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == req.m_resItem.m_itemData.m_shared.m_name);
                                                ItemDrop.ItemData newItem = prefab.GetComponent<ItemDrop>().m_itemData.Clone();
                                                int numToAdd = Mathf.RoundToInt(req.GetAmount(j) * returnResources.Value);
                                                Dbgl($"Returning {numToAdd}/{req.GetAmount(j)} {prefab.name}");
                                                while (numToAdd > 0) {
                                                    int stack = Mathf.Min(req.m_resItem.m_itemData.m_shared.m_maxStackSize, numToAdd);
                                                    numToAdd -= stack;


                                                    // added by cjayride
                                                    if ((prefab.name == "Coins" && recycleCoins.Value) || req.m_resItem.m_itemData.m_shared.m_itemType.ToString() == "Trophie" && recycleTrophy.Value || (prefab.name != "Coins" && req.m_resItem.m_itemData.m_shared.m_itemType.ToString() != "Trophie")) {

                                                        // original code
                                                        if (Player.m_localPlayer.GetInventory().AddItem(prefab.name, stack, req.m_resItem.m_itemData.m_quality, req.m_resItem.m_itemData.m_variant, 0, "") == null) {
                                                            ItemDrop component = Instantiate(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward + Player.m_localPlayer.transform.up, Player.m_localPlayer.transform.rotation).GetComponent<ItemDrop>();
                                                            component.m_itemData = newItem;
                                                            component.m_itemData.m_dropPrefab = prefab;
                                                            component.m_itemData.m_stack = stack;
                                                            Traverse.Create(component).Method("Save").GetValue();
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            // added by cjayride - check items were added before actually deleting held item. don't want to delete the base
                            if ((foundConsumable && recycleConsumables.Value || !foundConsumable) && (foundShards && recycleTrophy.Value || !foundShards)) {
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Recycling...");

                                if (___m_dragAmount == ___m_dragItem.m_stack) {
                                    Player.m_localPlayer.RemoveEquipAction(___m_dragItem);
                                    Player.m_localPlayer.UnequipItem(___m_dragItem, false);
                                    ___m_dragInventory.RemoveItem(___m_dragItem);
                                } else
                                    ___m_dragInventory.RemoveItem(___m_dragItem, ___m_dragAmount);
                                Destroy(___m_dragGo);
                                ___m_dragGo = null;
                                __instance.GetType().GetMethod("UpdateCraftingPanel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { false });
                            }
                        }
                    } else {
                        // added by cjayride
                        // Not really a full inventory, but there is no recipe for this item, so don't delete it
                        //fullInventory = true;
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Cannot be recycled or unknown recipe.");
                    }
                }

            }
        }
    }
}
