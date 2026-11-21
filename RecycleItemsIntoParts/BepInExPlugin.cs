using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace DiscardInventoryItem
{
    [BepInPlugin("cjayride.RecycleItemsIntoParts", "Recycle Items Into Parts", "1.8.11")]
    public class BepInExPlugin: BaseUnityPlugin
    {
        private static readonly bool isDebug = false;
        internal static ConfigSync configSync = new ConfigSync("cjayride.RecycleItemsIntoParts")
        {
            DisplayName = "Recycle Items Into Parts",
            CurrentVersion = "1.8.11",
            MinimumRequiredVersion = "1.7.6"
        };

        public static ConfigEntry<string> hotKey;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> serverEnforced;
        public static ConfigEntry<bool> returnUnknownResources;
        public static ConfigEntry<bool> returnEnchantedResources;
        public static ConfigEntry<float> returnResources;
        public static ConfigEntry<float> returnResourcesMagic;
        private static BepInExPlugin context;
        private static Assembly epicLootAssembly;

        public static ConfigEntry<bool> recycleCoins;
        public static ConfigEntry<bool> betterArchery;
        public static ConfigEntry<int> betterArcheryCount;
        public static ConfigEntry<bool> equipmentAndQuickSlots;
        public static ConfigEntry<int> equipmentAndQuickSlotsCount;
        public static ConfigEntry<int> inventoryRows;
        public static ConfigEntry<bool> recycleConsumables;
        public static ConfigEntry<bool> recycleTrophy;
        public static ConfigEntry<bool> recycleShards;
        public static ConfigEntry<bool> downgradeEnchantRarity;
        public static ConfigEntry<bool> magicDustSoftener;
        public static ConfigEntry<int> magicDustSoftenerAmount;
        public static ConfigEntry<bool> requireConfirm;

        private static readonly Dictionary<string, ReturnPreview> pendingReturns = new Dictionary<string, ReturnPreview>();
        private static KeyCode recycleKeyCode = KeyCode.Delete;
        private static InventoryGui pendingGui;
        private static ItemDrop.ItemData pendingItem;
        private static Inventory pendingInventory;
        private static int pendingAmount;
        private static GameObject pendingDragGo;
        private static bool popupOpen;
        private static bool popupCanConfirm;
        private static int suppressInventoryHideFrame = -1;
        private static GameObject confirmRoot;
        private static InventoryGrid pointerGrid;
        private static Vector2i pointerPos;
        private static ItemDrop.ItemData pointerItem;
        private static Component hoveredTooltip;
        private static readonly List<InventoryGrid> cachedGrids = new List<InventoryGrid>();
        private static readonly Vector3[] worldCorners = new Vector3[4];
        private static readonly List<RaycastResult> raycastHits = new List<RaycastResult>();
        private static FieldInfo dragItemField;
        private static FieldInfo dragInventoryField;
        private static FieldInfo dragAmountField;
        private static FieldInfo dragGoField;
        private static FieldInfo playerGridField;
        private static FieldInfo containerGridField;
        private static FieldInfo recipeNameField;
        private static FieldInfo playerNameField;
        private static FieldInfo elementsField;
        private static Action<InventoryGui, bool> updateCraftingPanel;
        private static Func<Vector3> getMousePosition;
        private static Func<InventoryGrid, GameObject, Vector2i> getButtonPos;
        private static Func<InventoryGrid, InventoryElement> getHoveredElement;
        private static Action<ItemDrop> saveItemDrop;
        private static MethodInfo epicIsMagic;
        private static MethodInfo epicGetRarity;
        private static MethodInfo epicGetEnchantCosts;
        private static ItemDrop cachedMagicDust;
        private static bool magicDustSearched;

        private ConfigEntry<T> BindConfig<T>(string group, string name, T value, string description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);
            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;
            return configEntry;
        }

        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        private void Awake()
        {
            context = this;

            serverEnforced = BindConfig("General", "ServerEnforced", true, "If true, clients must use the server/host recycle settings. If false, each player uses their own config.");
            configSync.AddLockingConfigEntry(serverEnforced);

            hotKey = BindConfig("General", "RecycleHotkey", "Delete", "Keyboard key name (Unity KeyCode). Use Delete, not delete.", false);
            hotKey.SettingChanged += (_, __) => RefreshRecycleKeyCode();
            RefreshRecycleKeyCode();
            modEnabled = BindConfig("General", "Enabled", true, "Enable this mod");
            returnUnknownResources = BindConfig("General", "ReturnUnknownResources", true, "Return resources if recipe is unknown");
            returnEnchantedResources = BindConfig("General", "ReturnEnchantedResources", true, "Return resources for Epic Loot enchantments");
            returnResources = BindConfig("General", "ReturnResources", 0.40f, "Fraction of craft materials to return (0.0 - 1.0). 0.40 means 1 wood returns 0, 3 wood returns 1.");
            returnResourcesMagic = BindConfig("General", "ReturnResourcesMagic", 0.40f, "Fraction of EpicLoot enchant materials to return after the rarity drop (0.0 - 1.0). 5 Magic Dust at 0.40 returns 2.");
            recycleCoins = BindConfig("General", "RecycleCoins", false, "Enable/disable coins on recycling items");
            recycleConsumables = BindConfig("General", "RecycleConsumables", true, "Enable/disable recycling of consumables (like food) (need to disable for cjaycraft ultimate modpack)");
            recycleTrophy = BindConfig("General", "RecycleTrophy", true, "Enable/disable recycling of Trophy items (need to disable for cjaycraft ultimate modpack)");
            recycleShards = BindConfig("General", "RecycleShards", true, "Enable/disable recycling of Shards (need to disable for cjaycraft ultimate modpack)");
            downgradeEnchantRarity = BindConfig("General", "DowngradeEnchantRarity", true, "Return EpicLoot enchant materials one rarity lower (Legendary -> Epic). Rare+ stay one tier down.");
            magicDustSoftener = BindConfig("General", "MagicDustSoftener", true, "If true, green/Magic items return a little Magic Dust instead of no enchant mats.");
            magicDustSoftenerAmount = BindConfig("General", "MagicDustSoftenerAmount", 1, "Magic Dust given by the green-item softener (1 or 2).");
            requireConfirm = BindConfig("General", "RequireConfirm", true, "Show a Yes/No popup with returned parts before recycling.");

            CacheGameApi();

            Harmony harmony = new Harmony(Info.Metadata.GUID);
            MethodInfo hide = AccessTools.Method(typeof(InventoryGui), "Hide");
            if (hide != null)
                harmony.Patch(hide,
                    prefix: new HarmonyMethod(typeof(Hide_Patch), nameof(Hide_Patch.Prefix)),
                    postfix: new HarmonyMethod(typeof(Hide_Patch), nameof(Hide_Patch.Postfix)));
            MethodInfo enterGui = AccessTools.Method(typeof(InventoryGui), "OnEnterElement");
            if (enterGui != null)
                harmony.Patch(enterGui, postfix: new HarmonyMethod(typeof(OnEnterElement_Patch), nameof(OnEnterElement_Patch.Postfix)));
            MethodInfo enterGrid = AccessTools.Method(typeof(InventoryGrid), "OnPointerEnter");
            if (enterGrid != null)
                harmony.Patch(enterGrid, postfix: new HarmonyMethod(typeof(OnPointerEnter_Patch), nameof(OnPointerEnter_Patch.Postfix)));
            PatchTooltipHover(harmony);
        }

        private static void CacheGameApi()
        {
            dragItemField = AccessTools.Field(typeof(InventoryGui), "m_dragItem");
            dragInventoryField = AccessTools.Field(typeof(InventoryGui), "m_dragInventory");
            dragAmountField = AccessTools.Field(typeof(InventoryGui), "m_dragAmount");
            dragGoField = AccessTools.Field(typeof(InventoryGui), "m_dragGo");
            playerGridField = AccessTools.Field(typeof(InventoryGui), "m_playerGrid");
            containerGridField = AccessTools.Field(typeof(InventoryGui), "m_containerGrid");
            recipeNameField = AccessTools.Field(typeof(InventoryGui), "m_recipeName");
            playerNameField = AccessTools.Field(typeof(InventoryGui), "m_playerName");
            elementsField = AccessTools.Field(typeof(InventoryGrid), "m_elements");

            updateCraftingPanel = CreateOpenDelegate<Action<InventoryGui, bool>>(AccessTools.Method(typeof(InventoryGui), "UpdateCraftingPanel", new Type[] { typeof(bool) }));
            MethodInfo mouse = AccessTools.DeclaredMethod(typeof(ZInput), "GetMousePosition");
            if (mouse != null && mouse.ReturnType == typeof(Vector3) && mouse.GetParameters().Length == 0)
                getMousePosition = CreateOpenDelegate<Func<Vector3>>(mouse);
            getButtonPos = CreateOpenDelegate<Func<InventoryGrid, GameObject, Vector2i>>(AccessTools.Method(typeof(InventoryGrid), "GetButtonPos", new Type[] { typeof(GameObject) }));
            getHoveredElement = CreateOpenDelegate<Func<InventoryGrid, InventoryElement>>(AccessTools.Method(typeof(InventoryGrid), "GetHoveredElement"));
            saveItemDrop = CreateOpenDelegate<Action<ItemDrop>>(AccessTools.Method(typeof(ItemDrop), "Save"));
        }

        private static T GetObj<T>(FieldInfo field, object instance) where T : class
        {
            return field != null && instance != null ? field.GetValue(instance) as T : null;
        }

        private static T CreateOpenDelegate<T>(MethodInfo method) where T : Delegate
        {
            if (method == null)
                return null;
            try
            {
                return (T)Delegate.CreateDelegate(typeof(T), method);
            }
            catch
            {
                return null;
            }
        }

        private static void PatchTooltipHover(Harmony harmony)
        {
            Type tooltipType = AccessTools.TypeByName("UITooltip");
            MethodInfo method = tooltipType != null ? AccessTools.Method(tooltipType, "OnPointerEnter") : null;
            if (method == null)
                return;
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(TooltipHover_Patch), nameof(TooltipHover_Patch.Postfix)));
        }
        private void Start()
        {
            if (Chainloader.PluginInfos.TryGetValue("randyknapp.mods.epicloot", out PluginInfo epic) && epic.Instance != null)
            {
                epicLootAssembly = epic.Instance.GetType().Assembly;
                CacheEpicLootApi();
            }
            DismissStuckUnifiedPopup();
        }

        private void Update()
        {
            TickRecycle(InventoryGui.instance);
        }

        private static void CacheEpicLootApi()
        {
            if (epicLootAssembly == null)
                return;
            Type ext = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions");
            Type helper = epicLootAssembly.GetType("EpicLoot.Crafting.EnchantHelper");
            if (ext != null)
            {
                epicIsMagic = ext.GetMethod("IsMagic", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ItemDrop.ItemData) }, null);
                epicGetRarity = ext.GetMethod("GetRarity", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(ItemDrop.ItemData) }, null);
            }
            if (helper != null)
                epicGetEnchantCosts = helper.GetMethod("GetEnchantCosts", BindingFlags.Public | BindingFlags.Static);
        }

        private static void TickRecycle(InventoryGui instance)
        {
            if (!modEnabled.Value)
                return;

            if (popupOpen)
            {
                TickPopupKeys();
                return;
            }

            if (!IsRecycleKeyDown())
                return;

            if (Player.m_localPlayer == null || instance == null || !InventoryGui.IsVisible())
                return;

            if (!TryGetTargetItem(instance, out ItemDrop.ItemData item, out Inventory inventory, out int amount, out GameObject dragGo))
                return;

            if (!TryRecycle(instance, item, inventory, amount, dragGo, commit: false))
                return;

            if (requireConfirm.Value)
            {
                ShowConfirmPopup(instance, item, inventory, amount, dragGo);
                return;
            }

            TryRecycle(instance, item, inventory, amount, dragGo, commit: true);
        }

        private static bool IsCancelKeyDown()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Mouse1);
        }

        private static bool IsConfirmKeyDown()
        {
            return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
        }

        private static void TickPopupKeys()
        {
            if (IsCancelKeyDown())
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    suppressInventoryHideFrame = Time.frameCount;
                OnConfirmNo();
                return;
            }

            if (!IsConfirmKeyDown())
                return;

            if (popupCanConfirm)
                OnConfirmYes();
            else
                OnConfirmNo();
        }

        static class Hide_Patch
        {
            public static bool Prefix()
            {
                if (Time.frameCount == suppressInventoryHideFrame)
                    return false;
                if (popupOpen && Input.GetKeyDown(KeyCode.Escape))
                {
                    suppressInventoryHideFrame = Time.frameCount;
                    OnConfirmNo();
                    return false;
                }
                return true;
            }

            public static void Postfix()
            {
                ClosePopup();
                pointerGrid = null;
                pointerItem = null;
                hoveredTooltip = null;
                cachedGrids.Clear();
            }
        }

        static class OnEnterElement_Patch
        {
            public static void Postfix(InventoryGrid __0, Vector2i __1)
            {
                SetPointerHover(__0, __1);
            }
        }

        static class OnPointerEnter_Patch
        {
            public static void Postfix(InventoryGrid __instance, UIInputHandler __0)
            {
                if (__instance == null || __0 == null)
                    return;
                if (getButtonPos == null)
                    return;
                SetPointerHover(__instance, getButtonPos(__instance, __0.gameObject));
            }
        }

        static class TooltipHover_Patch
        {
            public static void Postfix(object __instance)
            {
                hoveredTooltip = __instance as Component;
            }
        }

        private static void SetPointerHover(InventoryGrid grid, Vector2i pos)
        {
            pointerGrid = grid;
            pointerPos = pos;
            pointerItem = grid != null ? grid.GetItem(pos) : null;
        }

        private static void ClosePopup()
        {
            DismissStuckUnifiedPopup();
            ClearPending();
        }

        private static void DismissStuckUnifiedPopup()
        {
            try
            {
                for (int i = 0; i < 8 && UnifiedPopup.IsVisible(); i++)
                    UnifiedPopup.Pop();
            }
            catch
            {
            }
        }

        private static void DestroyConfirmUi()
        {
            if (confirmRoot != null)
            {
                UnityEngine.Object.Destroy(confirmRoot);
                confirmRoot = null;
            }
        }

        private static void ClearPending()
        {
            DestroyConfirmUi();
            popupOpen = false;
            popupCanConfirm = false;
            pendingGui = null;
            pendingItem = null;
            pendingInventory = null;
            pendingAmount = 0;
            pendingDragGo = null;
        }

        private static void ShowConfirmPopup(InventoryGui gui, ItemDrop.ItemData item, Inventory inventory, int amount, GameObject dragGo)
        {
            pendingGui = gui;
            pendingItem = item;
            pendingInventory = inventory;
            pendingAmount = amount;
            pendingDragGo = dragGo;
            popupOpen = true;

            string name = ItemDisplayName(item);
            string title = "Recycle";
            string body = "Recycle " + name;
            if (amount > 1)
                body += " x" + amount;
            body += "?";

            ShowInventoryConfirm(gui, title, body, true);
        }

        private static void ShowBlockedPopup(string title, string body)
        {
            popupOpen = true;
            InventoryGui gui = InventoryGui.instance;
            if (gui == null)
                return;
            ShowInventoryConfirm(gui, title, body, false);
        }

        private static void ShowInventoryConfirm(InventoryGui gui, string title, string body, bool canConfirm)
        {
            DestroyConfirmUi();
            DismissStuckUnifiedPopup();
            ShowFallbackConfirm(gui, title, body, canConfirm);
        }

        private static void ShowFallbackConfirm(InventoryGui gui, string title, string body, bool canConfirm)
        {
            Canvas parentCanvas = gui.GetComponentInParent<Canvas>();
            Transform parent = parentCanvas != null ? parentCanvas.transform : gui.transform;

            popupCanConfirm = canConfirm;
            confirmRoot = new GameObject("RecycleConfirm", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
            confirmRoot.transform.SetParent(parent, false);
            Canvas canvas = confirmRoot.GetComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9999;
            if (parentCanvas != null)
            {
                canvas.renderMode = parentCanvas.renderMode;
                canvas.worldCamera = parentCanvas.worldCamera;
            }
            else
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            RectTransform rootRt = confirmRoot.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            GameObject dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(confirmRoot.transform, false);
            Image dimImg = dim.GetComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.45f);
            dimImg.raycastTarget = true;
            RectTransform dimRt = dim.GetComponent<RectTransform>();
            dimRt.anchorMin = Vector2.zero;
            dimRt.anchorMax = Vector2.one;
            dimRt.offsetMin = Vector2.zero;
            dimRt.offsetMax = Vector2.zero;

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(confirmRoot.transform, false);
            Image panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0.08f, 0.08f, 0.08f, 0.96f);
            panelImg.raycastTarget = true;
            RectTransform panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            int rowCount = canConfirm ? pendingReturns.Count : 0;
            float panelHeight = canConfirm ? Mathf.Clamp(360f + rowCount * 38f, 420f, 620f) : 320f;
            panelRt.sizeDelta = new Vector2(560f, panelHeight);
            panelRt.anchoredPosition = Vector2.zero;

            TMP_Text textSource = GetObj<TMP_Text>(recipeNameField, gui);
            if (textSource == null)
                textSource = GetObj<TMP_Text>(playerNameField, gui);

            float top = panelHeight * 0.5f - 40f;
            CreateLabel(panel.transform, textSource, title, new Vector2(0f, top), new Vector2(500f, 40f), 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            CreateLabel(panel.transform, textSource, body, new Vector2(0f, top - 42f), new Vector2(500f, 40f), 18f, FontStyles.Normal, TextAlignmentOptions.Center);

            if (canConfirm)
            {
                CreateLabel(panel.transform, textSource, "You will get:", new Vector2(0f, top - 84f), new Vector2(500f, 28f), 18f, FontStyles.Bold, TextAlignmentOptions.Left);
                float y = top - 118f;
                if (pendingReturns.Count == 0)
                {
                    CreateLabel(panel.transform, textSource, "Nothing (return rounded to 0).", new Vector2(20f, y), new Vector2(480f, 32f), 16f, FontStyles.Normal, TextAlignmentOptions.Left);
                }
                else
                {
                    foreach (KeyValuePair<string, ReturnPreview> pair in pendingReturns)
                    {
                        CreateReturnRow(panel.transform, textSource, pair.Value.icon, pair.Value.amount + "x " + pair.Key, y);
                        y -= 38f;
                    }
                }
            }

            float buttonY = -panelHeight * 0.5f + 50f;
            if (canConfirm)
            {
                CreatePlainButton(panel.transform, textSource, "Yes", new Vector2(-120f, buttonY), new Color(0.22f, 0.45f, 0.18f, 1f), new UnityAction(OnConfirmYes));
                CreatePlainButton(panel.transform, textSource, "No", new Vector2(120f, buttonY), new Color(0.5f, 0.16f, 0.14f, 1f), new UnityAction(OnConfirmNo));
            }
            else
                CreatePlainButton(panel.transform, textSource, "OK", new Vector2(0f, buttonY), new Color(0.35f, 0.28f, 0.16f, 1f), new UnityAction(OnConfirmNo));
        }

        private static void CreateReturnRow(Transform parent, TMP_Text textSource, Sprite icon, string text, float y)
        {
            GameObject row = new GameObject("ReturnRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);
            row.transform.localScale = Vector3.one;
            RectTransform rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.5f, 0.5f);
            rowRt.anchorMax = new Vector2(0.5f, 0.5f);
            rowRt.sizeDelta = new Vector2(500f, 36f);
            rowRt.anchoredPosition = new Vector2(0f, y);

            GameObject iconGo = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconGo.transform.SetParent(row.transform, false);
            Image image = iconGo.GetComponent<Image>();
            image.sprite = icon;
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.color = icon != null ? Color.white : new Color(1f, 1f, 1f, 0.15f);
            RectTransform iconRt = iconGo.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(0f, 0.5f);
            iconRt.pivot = new Vector2(0f, 0.5f);
            iconRt.sizeDelta = new Vector2(32f, 32f);
            iconRt.anchoredPosition = new Vector2(8f, 0f);

            CreateLabel(row.transform, textSource, text, new Vector2(40f, 0f), new Vector2(440f, 32f), 18f, FontStyles.Normal, TextAlignmentOptions.Left);
            RectTransform labelRt = row.transform.GetChild(row.transform.childCount - 1) as RectTransform;
            if (labelRt != null)
            {
                labelRt.anchorMin = new Vector2(0f, 0.5f);
                labelRt.anchorMax = new Vector2(0f, 0.5f);
                labelRt.pivot = new Vector2(0f, 0.5f);
                labelRt.anchoredPosition = new Vector2(48f, 0f);
            }
        }

        private static void CreateLabel(Transform parent, TMP_Text source, string text, Vector2 pos, Vector2 size, float fontSize, FontStyles style)
        {
            CreateLabel(parent, source, text, pos, size, fontSize, style, TextAlignmentOptions.Center);
        }

        private static void CreateLabel(Transform parent, TMP_Text source, string text, Vector2 pos, Vector2 size, float fontSize, FontStyles style, TextAlignmentOptions align)
        {
            GameObject go;
            TMP_Text label;
            if (source != null)
            {
                go = UnityEngine.Object.Instantiate(source.gameObject, parent);
                go.name = "Label";
                label = go.GetComponent<TMP_Text>();
            }
            else
            {
                go = new GameObject("Label", typeof(RectTransform));
                go.transform.SetParent(parent, false);
                label = go.AddComponent<TextMeshProUGUI>();
            }

            foreach (Behaviour extra in go.GetComponents<Behaviour>())
            {
                if (extra != null && extra != label && !(extra is RectTransform) && extra.GetType().Name != "ContentSizeFitter")
                    extra.enabled = false;
            }

            foreach (Graphic graphic in go.GetComponentsInChildren<Graphic>(true))
                graphic.raycastTarget = false;

            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = align;
            label.enableWordWrapping = true;
            label.raycastTarget = false;
            go.transform.localScale = Vector3.one;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
        }

        private static void CreatePlainButton(Transform parent, TMP_Text textSource, string text, Vector2 pos, Color color, UnityAction click)
        {
            GameObject go = new GameObject(text, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one;

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(click);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(180f, 48f);
            rt.anchoredPosition = pos;

            CreateLabel(go.transform, textSource, text, Vector2.zero, new Vector2(170f, 40f), 20f, FontStyles.Bold);
        }

        private static void OnConfirmYes()
        {
            InventoryGui gui = pendingGui;
            ItemDrop.ItemData item = pendingItem;
            Inventory inventory = pendingInventory;
            int amount = pendingAmount;
            GameObject dragGo = pendingDragGo;
            ClearPending();

            if (gui == null || item == null || inventory == null || !inventory.ContainsItem(item))
            {
                Dbgl("Confirm yes but item is no longer in inventory");
                return;
            }

            TryRecycle(gui, item, inventory, amount, dragGo, commit: true);
        }

        private static void OnConfirmNo()
        {
            ClearPending();
        }

        private static void RefreshRecycleKeyCode()
        {
            string raw = hotKey != null ? hotKey.Value : "Delete";
            if (string.IsNullOrEmpty(raw))
            {
                recycleKeyCode = KeyCode.Delete;
                return;
            }

            string token = raw.Split('+')[0].Trim();
            if (string.Equals(token, "del", StringComparison.OrdinalIgnoreCase)
                || string.Equals(token, "delete", StringComparison.OrdinalIgnoreCase))
            {
                recycleKeyCode = KeyCode.Delete;
                return;
            }

            if (Enum.TryParse(token, true, out KeyCode code) && code != KeyCode.None)
            {
                recycleKeyCode = code;
                return;
            }

            recycleKeyCode = KeyCode.Delete;
            Dbgl("Unknown RecycleHotkey '" + raw + "', using Delete");
        }

        private static bool IsRecycleKeyDown()
        {
            return Input.GetKeyDown(recycleKeyCode);
        }

        private static bool TryGetTargetItem(InventoryGui gui, out ItemDrop.ItemData item, out Inventory inventory, out int amount, out GameObject dragGo)
        {
            item = GetObj<ItemDrop.ItemData>(dragItemField, gui);
            inventory = GetObj<Inventory>(dragInventoryField, gui);
            amount = dragAmountField != null ? (int)dragAmountField.GetValue(gui) : 0;
            dragGo = GetObj<GameObject>(dragGoField, gui);
            if (item != null && inventory != null && inventory.ContainsItem(item) && amount > 0)
                return true;

            item = null;
            inventory = null;
            amount = 0;
            dragGo = null;

            if (TryGetItemFromPointer(gui, out item, out inventory, out amount))
                return true;

            return false;
        }

        private static List<InventoryGrid> GetGrids(InventoryGui gui)
        {
            if (cachedGrids.Count > 0)
                return cachedGrids;

            AddGrid(cachedGrids, GetObj<InventoryGrid>(playerGridField, gui));
            AddGrid(cachedGrids, GetObj<InventoryGrid>(containerGridField, gui));
            AddGrid(cachedGrids, pointerGrid);
            InventoryGrid[] found = UnityEngine.Object.FindObjectsByType<InventoryGrid>(FindObjectsSortMode.None);
            foreach (InventoryGrid grid in found)
                AddGrid(cachedGrids, grid);
            return cachedGrids;
        }

        private static void AddGrid(List<InventoryGrid> grids, InventoryGrid grid)
        {
            if (grid == null || grids.Contains(grid))
                return;
            if (!grid.gameObject.activeInHierarchy)
                return;
            grids.Add(grid);
        }

        private static bool TryGetItemFromPointer(InventoryGui gui, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;

            if (pointerItem != null && pointerGrid != null)
            {
                Inventory hoverInv = pointerGrid.GetInventory();
                ItemDrop.ItemData current = GetItemFromSlot(pointerGrid, pointerPos);
                if (hoverInv != null && current != null && hoverInv.ContainsItem(current))
                {
                    item = current;
                    inventory = hoverInv;
                    amount = current.m_stack;
                    return true;
                }
            }

            List<InventoryGrid> grids = GetGrids(gui);
            foreach (InventoryGrid grid in grids)
            {
                if (TryGetHoveredGridItem(grid, out item, out inventory, out amount))
                    return true;
            }

            if (TryGetItemUnderMouse(grids, out item, out inventory, out amount))
                return true;

            if (TryGetItemFromTooltip(grids, out item, out inventory, out amount))
                return true;

            return TryGetItemFromUiRaycast(grids, out item, out inventory, out amount);
        }

        private static bool TryGetItemUnderMouse(List<InventoryGrid> grids, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;
            Vector2 mouse = GetMouseScreenPos();
            foreach (InventoryGrid grid in grids)
            {
                Inventory gridInv = grid.GetInventory();
                if (gridInv == null)
                    continue;

                IList elements = GetGridElements(grid);
                if (elements != null)
                {
                    foreach (object raw in elements)
                    {
                        InventoryElement element = raw as InventoryElement;
                        if (element == null)
                            continue;
                        if (!IsMouseOverElement(element, mouse))
                            continue;
                        ItemDrop.ItemData hoveredItem = GetItemFromSlot(grid, element.Position);
                        if (hoveredItem == null || !gridInv.ContainsItem(hoveredItem))
                            continue;
                        item = hoveredItem;
                        inventory = gridInv;
                        amount = hoveredItem.m_stack;
                        return true;
                    }
                }

            }
            return false;
        }

        private static bool IsMouseOverElement(InventoryElement element, Vector2 mouse)
        {
            RectTransform[] rects = new RectTransform[]
            {
                element.m_button != null ? element.m_button.transform as RectTransform : null,
                element.m_touchRect,
                element.GetElementRectTransform()
            };
            foreach (RectTransform rect in rects)
            {
                if (IsMouseOverRect(rect, mouse))
                    return true;
            }
            return false;
        }

        private static bool IsMouseOverRect(RectTransform rect, Vector2 mouse)
        {
            if (rect == null || !rect.gameObject.activeInHierarchy)
                return false;

            Canvas canvas = rect.GetComponentInParent<Canvas>();
            Camera cam = null;
            if (canvas != null)
            {
                Canvas root = canvas.rootCanvas;
                if (root != null && root.renderMode != RenderMode.ScreenSpaceOverlay)
                    cam = root.worldCamera;
            }

            rect.GetWorldCorners(worldCorners);
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < 4; i++)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, worldCorners[i]);
                min = Vector2.Min(min, screen);
                max = Vector2.Max(max, screen);
            }
            return mouse.x >= min.x && mouse.x <= max.x && mouse.y >= min.y && mouse.y <= max.y;
        }

        private static Vector2 GetMouseScreenPos()
        {
            if (getMousePosition != null)
                return getMousePosition();
            return Input.mousePosition;
        }

        private static ItemDrop.ItemData GetItemFromSlot(InventoryGrid grid, Vector2i pos)
        {
            if (grid == null || pos.x < 0 || pos.y < 0)
                return null;
            ItemDrop.ItemData item = grid.GetItem(pos);
            if (item != null)
                return item;
            Inventory inv = grid.GetInventory();
            if (inv == null)
                return null;
            return inv.GetItemAt(pos.x, pos.y);
        }

        private static bool TryGetItemFromTooltip(List<InventoryGrid> grids, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;
            if (hoveredTooltip == null)
                return false;

            GameObject tipGo = hoveredTooltip.gameObject;
            foreach (InventoryGrid grid in grids)
            {
                IList elements = GetGridElements(grid);
                if (elements == null)
                    continue;
                Inventory gridInv = grid.GetInventory();
                if (gridInv == null)
                    continue;
                foreach (object raw in elements)
                {
                    InventoryElement element = raw as InventoryElement;
                    if (element == null)
                        continue;
                    Component tip = element.m_tooltip;
                    if (tip == null)
                        continue;
                    if (tip != hoveredTooltip && tip.gameObject != tipGo)
                        continue;
                    ItemDrop.ItemData hoveredItem = grid.GetItem(element.Position);
                    if (hoveredItem == null || !gridInv.ContainsItem(hoveredItem))
                        continue;
                    item = hoveredItem;
                    inventory = gridInv;
                    amount = hoveredItem.m_stack;
                    return true;
                }
            }
            return false;
        }

        private static bool TryGetItemFromUiRaycast(List<InventoryGrid> grids, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

            PointerEventData pointer = new PointerEventData(eventSystem);
            pointer.position = GetMouseScreenPos();
            raycastHits.Clear();
            eventSystem.RaycastAll(pointer, raycastHits);
            if (raycastHits.Count == 0)
                return false;

            foreach (RaycastResult hit in raycastHits)
            {
                if (hit.gameObject == null)
                    continue;
                if (confirmRoot != null && hit.gameObject.transform.IsChildOf(confirmRoot.transform))
                    continue;

                foreach (InventoryGrid grid in grids)
                {
                    if (TryMatchHitToGridItem(grid, hit.gameObject, out item, out inventory, out amount))
                        return true;
                }
            }
            return false;
        }

        private static bool TryMatchHitToGridItem(InventoryGrid grid, GameObject hit, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;
            if (grid == null || hit == null)
                return false;

            IList elements = GetGridElements(grid);
            if (elements == null)
                return false;

            foreach (object raw in elements)
            {
                InventoryElement element = raw as InventoryElement;
                if (element == null)
                    continue;
                if (!HitIsOnElement(hit, element))
                    continue;

                Inventory gridInv = grid.GetInventory();
                ItemDrop.ItemData hoveredItem = grid.GetItem(element.Position);
                if (hoveredItem == null || gridInv == null || !gridInv.ContainsItem(hoveredItem))
                    continue;

                item = hoveredItem;
                inventory = gridInv;
                amount = hoveredItem.m_stack;
                return true;
            }
            return false;
        }

        private static bool HitIsOnElement(GameObject hit, InventoryElement element)
        {
            Transform[] roots = new Transform[]
            {
                element.m_button != null ? element.m_button.transform : null,
                element.m_touchRect,
                element.GetElementRectTransform()
            };
            foreach (Transform root in roots)
            {
                if (root == null)
                    continue;
                if (hit == root.gameObject || hit.transform.IsChildOf(root))
                    return true;
            }
            return false;
        }

        private static IList GetGridElements(InventoryGrid grid)
        {
            if (grid == null || elementsField == null)
                return null;
            return elementsField.GetValue(grid) as IList;
        }

        private static bool TryGetHoveredGridItem(InventoryGrid grid, out ItemDrop.ItemData item, out Inventory inventory, out int amount)
        {
            item = null;
            inventory = null;
            amount = 0;
            if (grid == null)
                return false;

            Inventory gridInv = grid.GetInventory();
            if (gridInv == null)
                return false;

            InventoryElement hovered = getHoveredElement != null ? getHoveredElement(grid) : null;
            if (hovered != null)
            {
                ItemDrop.ItemData hoveredItem = grid.GetItem(hovered.Position);
                if (hoveredItem != null && gridInv.ContainsItem(hoveredItem))
                {
                    item = hoveredItem;
                    inventory = gridInv;
                    amount = hoveredItem.m_stack;
                    return true;
                }
            }

            if (TryGetItemUnderMouse(grid, gridInv, out item, out amount))
            {
                inventory = gridInv;
                return true;
            }

            return false;
        }

        private static bool TryGetItemUnderMouse(InventoryGrid grid, Inventory gridInv, out ItemDrop.ItemData item, out int amount)
        {
            item = null;
            amount = 0;
            IList elements = GetGridElements(grid);
            if (elements == null)
                return false;

            Vector2 mouse = Input.mousePosition;
            foreach (object raw in elements)
            {
                InventoryElement element = raw as InventoryElement;
                if (element == null)
                    continue;

                RectTransform rect = null;
                if (element.m_button != null)
                    rect = element.m_button.transform as RectTransform;
                if (rect == null)
                    rect = element.m_touchRect;
                if (rect == null)
                    rect = element.GetElementRectTransform();
                if (rect == null)
                    continue;

                Canvas canvas = rect.GetComponentInParent<Canvas>();
                Camera cam = null;
                if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                    cam = canvas.worldCamera != null ? canvas.worldCamera : canvas.rootCanvas.worldCamera;

                bool inside = RectTransformUtility.RectangleContainsScreenPoint(rect, mouse, cam)
                    || RectTransformUtility.RectangleContainsScreenPoint(rect, mouse, null);
                if (!inside)
                    continue;

                ItemDrop.ItemData hoveredItem = grid.GetItem(element.Position);
                if (hoveredItem == null || !gridInv.ContainsItem(hoveredItem))
                    continue;

                item = hoveredItem;
                amount = hoveredItem.m_stack;
                return true;
            }
            return false;
        }

        private static bool TryRecycle(InventoryGui instance, ItemDrop.ItemData dragItem, Inventory dragInventory, int dragAmount, GameObject dragGo, bool commit)
        {
            pendingReturns.Clear();
            string recycledName = ItemDisplayName(dragItem);
            Dbgl($"Discarding {dragAmount}/{dragItem.m_stack} {dragItem.m_dropPrefab?.name}");

            Recipe recipe = ObjectDB.instance.GetRecipe(dragItem);
            bool craftEnabled = recipe != null && recipe.m_enabled;
            if (craftEnabled && !returnUnknownResources.Value && !Player.m_localPlayer.IsRecipeKnown(dragItem.m_shared.m_name))
            {
                if (commit)
                    Dbgl("Cannot recycle " + recycledName + " (unknown recipe).");
                else
                    ShowBlockedPopup("Cannot recycle", recycledName + " has no known recipe.");
                return false;
            }

            string itemSharedName = dragItem.m_shared?.m_name ?? "";
            bool foundShards = itemSharedName.IndexOf("shard", StringComparison.OrdinalIgnoreCase) >= 0
                || recipe?.m_item?.m_itemData?.m_shared?.m_name == "Magic $mod_epicloot_assets_shard";
            bool foundConsumable = dragItem.m_shared != null && dragItem.m_shared.m_itemType.ToString() == "Consumable";
            if (foundShards && !recycleShards.Value)
            {
                ShowBlockedPopup("Cannot recycle", "Shards cannot be recycled.");
                return false;
            }
            if (foundConsumable && !recycleConsumables.Value)
            {
                ShowBlockedPopup("Cannot recycle", "Consumables cannot be recycled.");
                return false;
            }

            bool isMagic = false;
            if (epicIsMagic != null && returnEnchantedResources.Value)
                isMagic = (bool)epicIsMagic.Invoke(null, new object[] { dragItem });

            if (!craftEnabled && !isMagic)
            {
                if (commit)
                    Dbgl("Cannot recycle " + recycledName + " (no enabled recipe).");
                else
                    ShowBlockedPopup("Cannot recycle", recycledName + " has no crafting recipe.");
                return false;
            }

            Piece.Requirement[] reqs = craftEnabled ? recipe.m_resources : Array.Empty<Piece.Requirement>();
            List<KeyValuePair<ItemDrop, int>> magicReqs = null;
            bool scaleEnchantReturns = false;
            if (isMagic)
            {
                int rarity = epicGetRarity != null ? (int)epicGetRarity.Invoke(null, new object[] { dragItem }) : 0;
                int returnRarity = rarity;
                if (downgradeEnchantRarity.Value)
                    returnRarity = rarity <= 0 ? -1 : rarity - 1;
                if (returnRarity >= 0)
                {
                    magicReqs = epicGetEnchantCosts != null
                        ? (List<KeyValuePair<ItemDrop, int>>)epicGetEnchantCosts.Invoke(null, new object[] { dragItem, returnRarity })
                        : null;
                    scaleEnchantReturns = true;
                }
                else if (magicDustSoftener.Value)
                    magicReqs = GetMagicDustSoftenerReqs();
            }

            int copies = 1;
            if (craftEnabled && recipe.m_amount > 0)
                copies = dragAmount / recipe.m_amount;
            else
                copies = Math.Max(1, dragAmount);
            if (copies <= 0)
            {
                if (commit)
                    Dbgl("Cannot recycle " + recycledName + " (recipe amount).");
                else
                    ShowBlockedPopup("Cannot recycle", recycledName + " has no crafting recipe.");
                return false;
            }

            for (int i = 0; i < copies; i++)
            {
                if (craftEnabled)
                {
                    foreach (Piece.Requirement req in reqs)
                    {
                        if (ShouldSkipReturnedResource(req))
                            continue;
                        int quality = dragItem.m_quality;
                        for (int j = quality; j > 0; j--)
                            QueueRequirement(req, Mathf.RoundToInt(req.GetAmount(j) * returnResources.Value), commit);
                    }
                }
                if (magicReqs != null)
                {
                    foreach (var kvp in magicReqs)
                    {
                        if (kvp.Key?.m_itemData?.m_shared == null)
                            continue;
                        if (kvp.Key.m_itemData.m_shared.m_name == "$item_coins" && !recycleCoins.Value)
                            continue;
                        int give = scaleEnchantReturns
                            ? Mathf.RoundToInt(kvp.Value * returnResourcesMagic.Value)
                            : kvp.Value;
                        QueueRequirement(new Piece.Requirement() { m_amount = kvp.Value, m_resItem = kvp.Key }, give, commit);
                    }
                }
            }

            if (!commit)
                return true;

            if (dragAmount == dragItem.m_stack)
            {
                Player.m_localPlayer.RemoveEquipAction(dragItem);
                Player.m_localPlayer.UnequipItem(dragItem, false);
                dragInventory.RemoveItem(dragItem);
            }
            else
                dragInventory.RemoveItem(dragItem, dragAmount);

            if (dragGo != null)
                UnityEngine.Object.Destroy(dragGo);

            if (updateCraftingPanel != null)
                updateCraftingPanel(instance, false);
            return true;
        }

        private static List<KeyValuePair<ItemDrop, int>> GetMagicDustSoftenerReqs()
        {
            int amount = magicDustSoftenerAmount.Value;
            if (amount < 1)
                amount = 1;
            if (amount > 2)
                amount = 2;

            ItemDrop dust = FindMagicDust();
            if (dust == null)
                return null;

            return new List<KeyValuePair<ItemDrop, int>> { new KeyValuePair<ItemDrop, int>(dust, amount) };
        }

        private static ItemDrop FindMagicDust()
        {
            if (magicDustSearched)
                return cachedMagicDust;
            if (ObjectDB.instance == null)
                return null;
            magicDustSearched = true;

            string[] names = { "DustMagic", "MagicDust", "dust_magic" };
            foreach (string name in names)
            {
                GameObject prefab = ObjectDB.instance.GetItemPrefab(name);
                if (prefab == null)
                    continue;
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                if (drop != null)
                    return cachedMagicDust = drop;
            }

            foreach (GameObject prefab in ObjectDB.instance.m_items)
            {
                if (prefab == null)
                    continue;
                ItemDrop drop = prefab.GetComponent<ItemDrop>();
                string shared = drop?.m_itemData?.m_shared?.m_name ?? "";
                string prefabName = prefab.name ?? "";
                if (shared.IndexOf("dust", StringComparison.OrdinalIgnoreCase) >= 0
                    && (shared.IndexOf("magic", StringComparison.OrdinalIgnoreCase) >= 0 || prefabName.IndexOf("DustMagic", StringComparison.OrdinalIgnoreCase) >= 0))
                    return cachedMagicDust = drop;
                if (prefabName.Equals("DustMagic", StringComparison.OrdinalIgnoreCase))
                    return cachedMagicDust = drop;
            }
            return null;
        }

        private static void QueueRequirement(Piece.Requirement req, int numToAdd, bool commit)
        {
            if (numToAdd <= 0 || req?.m_resItem?.m_itemData?.m_shared == null)
                return;

            GameObject prefab = GetRequirementPrefab(req);
            if (prefab == null)
                return;
            ItemDrop prefabDrop = prefab.GetComponent<ItemDrop>();
            if (prefabDrop == null)
                return;

            ItemDrop.ItemData.ItemType itemType = req.m_resItem.m_itemData.m_shared.m_itemType;
            if (!((prefab.name == "Coins" && recycleCoins.Value) || (itemType == ItemDrop.ItemData.ItemType.Trophy && recycleTrophy.Value) || (prefab.name != "Coins" && itemType != ItemDrop.ItemData.ItemType.Trophy)))
                return;

            Sprite icon = GetItemIcon(req.m_resItem.m_itemData);
            if (icon == null)
                icon = GetItemIcon(prefabDrop.m_itemData);
            TrackReturn(ItemDisplayName(req.m_resItem.m_itemData), numToAdd, icon);
            if (!commit)
                return;

            ItemDrop.ItemData newItem = prefabDrop.m_itemData.Clone();
            Dbgl($"Returning {numToAdd} {prefab.name}");
            while (numToAdd > 0)
            {
                int stack = Mathf.Min(req.m_resItem.m_itemData.m_shared.m_maxStackSize, numToAdd);
                numToAdd -= stack;
                if (Player.m_localPlayer.GetInventory().AddItem(prefab.name, stack, req.m_resItem.m_itemData.m_quality, req.m_resItem.m_itemData.m_variant, 0L, "", false, true) == null)
                {
                    ItemDrop component = UnityEngine.Object.Instantiate(prefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward + Player.m_localPlayer.transform.up, Player.m_localPlayer.transform.rotation).GetComponent<ItemDrop>();
                    component.m_itemData = newItem;
                    component.m_itemData.m_dropPrefab = prefab;
                    component.m_itemData.m_stack = stack;
                    if (saveItemDrop != null)
                        saveItemDrop(component);
                }
            }
        }

        private class ReturnPreview
        {
            public int amount;
            public Sprite icon;
        }

        private static void TrackReturn(string name, int amount, Sprite icon)
        {
            if (string.IsNullOrEmpty(name) || amount <= 0)
                return;
            ReturnPreview existing;
            if (pendingReturns.TryGetValue(name, out existing))
            {
                existing.amount += amount;
                if (existing.icon == null)
                    existing.icon = icon;
            }
            else
                pendingReturns[name] = new ReturnPreview() { amount = amount, icon = icon };
        }

        private static Sprite GetItemIcon(ItemDrop.ItemData item)
        {
            if (item == null)
                return null;
            try
            {
                Sprite icon = item.GetIcon();
                if (icon != null)
                    return icon;
            }
            catch
            {
            }
            if (item.m_shared?.m_icons != null && item.m_shared.m_icons.Length > 0)
                return item.m_shared.m_icons[0];
            return null;
        }

        private static string ItemDisplayName(ItemDrop.ItemData item)
        {
            if (item?.m_shared == null)
                return "item";
            string name = item.m_shared.m_name;
            if (Localization.instance != null)
                return Localization.instance.Localize(name);
            return name;
        }

        private static bool ShouldSkipReturnedResource(Piece.Requirement req)
        {
            if (req?.m_resItem?.m_itemData?.m_shared == null)
                return true;
            if (IsUpgradeOnlyResource(req))
                return true;
            if (req.m_resItem.m_itemData.m_shared.m_name == "$item_coins" && !recycleCoins.Value)
                return true;
            if (req.m_resItem.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Trophy && !recycleTrophy.Value)
                return true;
            return false;
        }

        private static bool IsUpgradeOnlyResource(Piece.Requirement req)
        {
            if (req == null)
                return true;
            if (req.m_upgraderResource)
                return true;

            string prefabName = req.m_resItem != null ? req.m_resItem.name : "";
            string sharedName = req.m_resItem?.m_itemData?.m_shared?.m_name ?? "";
            return prefabName.IndexOf("Idol", StringComparison.OrdinalIgnoreCase) >= 0
                || sharedName.IndexOf("idol", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static GameObject GetRequirementPrefab(Piece.Requirement req)
        {
            if (req?.m_resItem?.m_itemData?.m_shared == null)
                return null;

            GameObject prefab = ObjectDB.instance.GetItemPrefab(req.m_resItem.m_itemData.m_shared);
            if (prefab == null)
                prefab = ObjectDB.instance.GetItemPrefab(req.m_resItem.name);
            if (prefab == null)
                prefab = req.m_resItem.gameObject;
            return prefab;
        }
    }
}
