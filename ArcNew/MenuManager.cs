using System.Collections.Generic;
using System.Linq;
using ArcAnnihilation.OrderState;
using ArcAnnihilation.Panels;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Menu;
using SharpDX;
using AbilityId = Ensage.AbilityId;

namespace ArcAnnihilation
{
    internal class MenuManager
    {
        public static readonly Menu Menu = new Menu("Arc Annihilation", "Arc Annihilationv2.0", true,
            "npc_dota_hero_arc_warden", true).SetFontColor(Color.DarkOrange);

        public static MenuItem DefaultCombo;
        public static MenuItem TempestCombo;
        public static MenuItem SparkSpamCombo;
        public static MenuItem SparkSpamTempestOnlyCombo;
        public static MenuItem AutoPushingCombo;
        public static MenuItem SummonAndPushing;
        public static MenuItem SummonAndCombo;

        private static readonly List<string> AbilityList = new List<string>
        {
            "arc_warden_tempest_double",
            "arc_warden_spark_wraith",
            "arc_warden_magnetic_field",
            "arc_warden_flux"
        };

        public static readonly Dictionary<string, byte> Items = new Dictionary<string, byte>
        {
            {"item_hurricane_pike", 1},
            {"item_mask_of_madness", 7},
            {"item_ancient_janggo", 1},
            {"item_dagon", 2},
            {"item_dagon_2", 2},
            {"item_dagon_3", 2},
            {"item_dagon_4", 2},
            {"item_dagon_5", 2},
            {"item_blink", 5},
            {"item_orchid", 4},
            {"item_manta", 1},
            {AbilityId.item_abyssal_blade.ToString(), 5},
            {AbilityId.item_nullifier.ToString(), 5},
            {AbilityId.item_diffusal_blade.ToString(), 5},
            {AbilityId.item_black_king_bar.ToString(), 5},
            {AbilityId.item_silver_edge.ToString(), 8},
            {AbilityId.item_invis_sword.ToString(), 8},
            {AbilityId.item_rod_of_atos.ToString(), 1},
            {AbilityId.item_solar_crest.ToString(), 1},
            {AbilityId.item_medallion_of_courage.ToString(), 1},
            {AbilityId.item_lotus_orb.ToString(), 1},
            {AbilityId.item_phase_boots.ToString(), 1},
            {AbilityId.item_satanic.ToString(), 1},
            {"item_arcane_boots", 1},
            {"item_guardian_greaves", 1},
            {"item_shivas_guard", 1},
            {"item_ethereal_blade", 3},
            {"item_bloodthorn", 4},
            {"item_soul_ring", 4},
            {"item_blade_mail", 4},
            {"item_veil_of_discord", 4},
            {"item_heavens_halberd", 1},
            {"item_necronomicon", 2},
            {"item_necronomicon_2", 2},
            {"item_necronomicon_3", 2},
            {"item_mjollnir", 1},
            //{ "item_hurricane_pike",1},

            {"item_sheepstick", 5},
            {"item_urn_of_shadows", 5}

            /*{"item_dust", 4}*/
        };

        public static bool DebugInGame => Menu.Item("Dev.Text.enable").GetValue<bool>();
        public static bool DebugInConsole => Menu.Item("Dev.Text2.enable").GetValue<bool>();

        public static bool IsEnable => GetBool("Enable");
        public static bool DoCombo => GetKey("Combo.Key");
        public static bool CloneCombo => GetKey("Combo.Tempest.Key");
        public static bool SparkSpam => GetKey("Combo.Sparks.Key");
        public static bool OrbWalkType => GetBool("OrbWalking.Type");
        public static bool BlinkUseExtraRange => GetBool("Blink.ExtraRange.Use");

        public static bool MagneticField => GetBool("MagneticField.InFront");
        public static bool CustomComboPriorityHero => GetBool("customOrderHero");
        public static bool IsInfoPanelEnabled => GetBool("InfoPanel.Enable");
        public static bool CustomComboPriorityTempest => GetBool("customOrderTempest");
        public static bool SilverEdgeBlocker => GetBool("Blink.BlockSilver");
        public static float GetItemPanelSize => GetSlider("ItemPanelSize");
        public static bool AutoPushingTargetting => GetBool("AutoPushing.AutoTargetting");
        public static float GetInfoPanelSize => GetSlider("InfoPanel.Size");
        public static float GetPushLaneSelectorSize => GetSlider("PushLaneSelector.Size");
        public static bool UseTravels => GetBool("AutoPushing.Travels");
        public static bool IsAbilityEnabledTempest(AbilityId id) => GetToggle("spellTempest", id.ToString());
        public static bool IsAbilityEnabled(AbilityId id) => GetToggle("spellHero", id.ToString());

        /*public static bool IsItemEnabledTempest(ItemId id) =>
            GetToggle("itemTempestEnable",
                id == ItemId.item_necronomicon_2 || id == ItemId.item_necronomicon_3
                    ? ItemId.item_necronomicon.ToString()
                    : id == ItemId.item_dagon_2 || id == ItemId.item_dagon_3 || id == ItemId.item_dagon_4 ||
                      id == ItemId.item_dagon_5
                        ? ItemId.item_dagon.ToString()
                        : id == ItemId.item_diffusal_blade_2 ? ItemId.item_diffusal_blade.ToString() : id.ToString());*/

        public static bool IsItemEnabledTempest(AbilityId id) => GetToggle("itemTempestEnable", id.ToString());
        public static bool IsItemEnabled(AbilityId id) => GetToggle("itemHeroEnable", id.ToString());
        /*public static bool IsItemEnabled(ItemId id) =>
            GetToggle("itemHeroEnable",
                id == ItemId.item_necronomicon_2 || id == ItemId.item_necronomicon_3
                    ? ItemId.item_necronomicon.ToString()
                    : id == ItemId.item_dagon_2 || id == ItemId.item_dagon_3 || id == ItemId.item_dagon_4 ||
                      id == ItemId.item_dagon_5
                        ? ItemId.item_dagon.ToString()
                        : id == ItemId.item_diffusal_blade_2 ? ItemId.item_diffusal_blade.ToString() : id.ToString());*/

        /*public static uint GetItemOrderHero(ItemId id) => GetPriority("itemHero",
            id == ItemId.item_necronomicon_2 || id == ItemId.item_necronomicon_3
                ? ItemId.item_necronomicon
                : id == ItemId.item_dagon_2 || id == ItemId.item_dagon_3 || id == ItemId.item_dagon_4 ||
                  id == ItemId.item_dagon_5
                    ? ItemId.item_dagon
                    : id == ItemId.item_diffusal_blade_2 ? ItemId.item_diffusal_blade : id);*/

        public static uint GetItemOrderTempest(AbilityId id) => GetPriority("itemTempest", id);
        public static uint GetItemOrderHero(AbilityId id) => GetPriority("itemHero", id);
        /*public static uint GetItemOrderTempest(ItemId id) => GetPriority("itemTempest",
            id == ItemId.item_necronomicon_2 || id == ItemId.item_necronomicon_3
                ? ItemId.item_necronomicon
                : id == ItemId.item_dagon_2 || id == ItemId.item_dagon_3 || id == ItemId.item_dagon_4 ||
                  id == ItemId.item_dagon_5
                    ? ItemId.item_dagon
                    : id == ItemId.item_diffusal_blade_2 ? ItemId.item_diffusal_blade : id);*/

        public static void SetPushLanePanelPosition(int x, int y)
        {
            Menu.Item("PushLaneSelector.X").SetValue(new Slider(x, 0, 2000));
            Menu.Item("PushLaneSelector.Y").SetValue(new Slider(y, 0, 2000));
        }
        public static void SetInfoPanelPosition(int x, int y)
        {
            Menu.Item("InfoPanel.X").SetValue(new Slider(x, 0, 2000));
            Menu.Item("InfoPanel.Y").SetValue(new Slider(y, 0, 2000));
        }
        public static void SetItemPanelPosition(int x, int y)
        {
            Menu.Item("ItemPanel.X").SetValue(new Slider(x, 0, 2000));
            Menu.Item("ItemPanel.Y").SetValue(new Slider(y, 0, 2000));
        }



        public static bool PushLanePanelCanBeMovedByMouse => GetBool("PushLaneSelector.Moving");
        public static bool InfoPanelCanBeMovedByMouse => GetBool("InfoPanel.Moving");
        public static bool ItemPanelCanBeMovedByMouse => GetBool("ItemPanel.Moving");
        public static bool AutoMidas => GetBool("AutoMidas.Enable");
        public static bool PushLanePanelHide => GetBool("PushLaneSelector.Hide");
        public static Vector2 GetPushLanePanelPosition
            => new Vector2(GetSlider("PushLaneSelector.X"), GetSlider("PushLaneSelector.Y"));
        public static Vector2 GetInfoPanelPosition
            => new Vector2(GetSlider("InfoPanel.X"), GetSlider("InfoPanel.Y"));
        public static Vector2 GetItemPanelPosition
            => new Vector2(GetSlider("ItemPanel.X"), GetSlider("ItemPanel.Y"));

        public static float GetBlinkExtraRange => GetSlider("Blink.ExtraRange");
        public static float GetBlinkMinRange => GetSlider("Blink.MinRange");
        public static bool IsAutoPushPanelEnable => GetBool("AutoPushLaneSelector.Enable");
        public static bool DrawTargettingRange => GetBool("AutoPushing.DrawTargettingRange");
        public static bool IsItemPanelEnable => GetBool("itemPanel.Enable");
        public static bool SmartFlux => GetBool("FluxSettings.Smart");
        public static bool SmartSpark => GetBool("SparkSettings.Smart");
        public static bool CheckForCreeps => GetBool("AutoPushing.CheckForEnemyCreeps");
        public static float OrbWalkingRange => GetSlider("OrbWalking.Range");
        public static float AutoPushingTargettingRange => GetSlider("AutoPushing.AutoPushingTargettingRange");
        public static bool OrbWalkerGoBeyond => GetBool("OrbWalking.OrbWalkerGoBeyond");
        public static bool TowerPriority => GetBool("AutoPushing.TowerPriority");
        public static bool AutoSummonOnPusing => GetBool("AutoSummoning.Pushing");
        public static bool AutoSummonOnTempestCombog => GetBool("AutoSummoning.TempestCombo");
        public static bool IsSummmoningAndCombing => GetKey("Combo.SummonAndCombo.Key");
        public static bool IsSummmoningAndPushing => GetKey("Combo.SummonAndPushing.Key");
        public static float GetBlinkExtraDelay => GetSlider("Blink.ExtraDelay");
        public static bool InAnyCombo(ulong key)
            =>
                GetKeyId("Combo.Key") == key || GetKeyId("Combo.Tempest.Key") == key ||
                GetKeyId("Combo.Sparks.Tempest.Key") == key || GetKeyId("Combo.Sparks.Key") == key ||
                GetKeyId("Combo.AutoPushing.Key") == key;
            

        public static void Init()
        {
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");

            DefaultCombo = new MenuItem("Combo.Key", "Main+Tempest Combo").SetValue(new KeyBind('0'));
            TempestCombo =
                new MenuItem("Combo.Tempest.Key", "Tempest Combo").SetValue(new KeyBind('0', KeyBindType.Toggle));
            SparkSpamCombo = new MenuItem("Combo.Sparks.Key", "Spark Spam").SetValue(new KeyBind('0'));
            SparkSpamTempestOnlyCombo =
                new MenuItem("Combo.Sparks.Tempest.Key", "[TempestOnly] Spark Spam").SetValue(new KeyBind('0',
                    KeyBindType.Toggle));
            AutoPushingCombo =
                new MenuItem("Combo.AutoPushing.Key", "Auto Pushing").SetValue(new KeyBind('0', KeyBindType.Toggle));

            SummonAndPushing =
                new MenuItem("Combo.SummonAndPushing.Key", "Ult+pushing near mouse").SetValue(new KeyBind('0'));

            SummonAndCombo =
                new MenuItem("Combo.SummonAndCombo.Key", "Ult+tempest combo near mouse").SetValue(new KeyBind('0'));


            var keys = new Menu("Hotkeys", "Hotkeys");
            keys.AddItem(DefaultCombo).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(TempestCombo).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(SparkSpamCombo).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(SparkSpamTempestOnlyCombo).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(AutoPushingCombo).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(SummonAndPushing).ValueChanged += Core.ComboStatusChanger;
            keys.AddItem(SummonAndCombo).ValueChanged += Core.ComboStatusChanger;
            var autoPushingKeys = new Menu("Auto Pushing Hotkey", "Auto Pushing Hotkey");
            var panels = new Menu("Panels", "Panels");
            settings.AddItem(new MenuItem("OrbWalking.Type", "[Orbwalking] move to target").SetValue(false))
                .SetTooltip("or to mouse");
            settings.AddItem(
                new MenuItem("OrbWalking.Range", "[Orbwalking] min range").SetValue(new Slider(50, 10, 1000)));
            settings.AddItem(
                    new MenuItem("OrbWalking.OrbWalkerGoBeyond", "[Orbwalking] Go beyond of selected range").SetValue(
                        true))
                .SetTooltip("only for Tempest");
            settings.AddItem(
                new MenuItem("AutoSummoning.Pushing", "Cast ultimate on autopusing").SetValue(false));
            settings.AddItem(
                new MenuItem("AutoSummoning.TempestCombo", "Cast ultimate on tempest combo").SetValue(false));
            var mf=settings.AddItem(
                new MenuItem("MagneticField.InFront", "Use Magnetic Field in front of ur hero").SetValue(true));
            var toggle = settings.AddItem(
                new MenuItem("MagneticField.InFront.ToggleKey", "Toggle for magnetic field setting").SetValue(
                    new KeyBind('0')));
            toggle.ValueChanged += (sender, args) =>
            {
                var newOne = args.GetNewValue<KeyBind>().Active;
                var oldOne = args.GetOldValue<KeyBind>().Active;
                if (newOne != oldOne && newOne)
                {
                    var newValue = !mf.GetValue<bool>();
                    mf.SetValue(newValue);
                    Game.PrintMessage($"MF: in front -> {newValue}");
                }
            };
            settings.AddItem(
                new MenuItem("AutoMidas.Enable", "Auto midas").SetValue(true));
            var usages = new Menu("Using in combo", "usages");
            var sparkSettings = new Menu("SparkSettings", "SparkSettings", false, AbilityId.arc_warden_spark_wraith.ToString());
            sparkSettings.AddItem(new MenuItem("SparkSettings.Smart", "Smart spark").SetValue(false))
                .SetTooltip("Use only if target is out of attack range");
            var fluxSettings = new Menu("FluxSettings", "FluxSettings", false, AbilityId.arc_warden_flux.ToString());
            fluxSettings.AddItem(new MenuItem("FluxSettings.Smart", "Smart flux").SetValue(false))
                .SetTooltip("Use only if there are no allies around the enemy");
            var itemPanel = new Menu("Item Panel", "ItemPanel");
            itemPanel.AddItem(new MenuItem("itemPanel.Enable", "Enable").SetValue(true)).ValueChanged +=
                ItemPanel.OnChange;
            itemPanel.AddItem(new MenuItem("ItemPanelSize", "Size").SetValue(new Slider(40, 20, 70)));
            itemPanel.AddItem(
                new MenuItem("ItemPanel.Moving", "Move by mouse").SetValue(false));
            itemPanel.AddItem(
                new MenuItem("ItemPanel.X", "Pos X").SetValue(new Slider(10, 0, 2000)));
            itemPanel.AddItem(
                new MenuItem("ItemPanel.Y", "Pos Y").SetValue(new Slider(350, 0, 2000)));

            var pushLaneSelectorPanel = new Menu("AutoPushLaneSelector Panel", "PushLaneSelector");
            pushLaneSelectorPanel.AddItem(new MenuItem("AutoPushLaneSelector.Enable", "Enable").SetValue(true))
                .ValueChanged += PushLaneSelector.OnChange;
            pushLaneSelectorPanel.AddItem(
                new MenuItem("PushLaneSelector.Size", "Text Size").SetValue(new Slider(30, 1, 70)));
            pushLaneSelectorPanel.AddItem(
                new MenuItem("PushLaneSelector.Hide", "Hide if current order isnt Push or Idle").SetValue(true));
            pushLaneSelectorPanel.AddItem(
                new MenuItem("PushLaneSelector.Moving", "Move by mouse").SetValue(false));
            pushLaneSelectorPanel.AddItem(
                new MenuItem("PushLaneSelector.X", "Pos X").SetValue(new Slider(10, 0, 2000)));
            pushLaneSelectorPanel.AddItem(
                new MenuItem("PushLaneSelector.Y", "Pos Y").SetValue(new Slider(350, 0, 2000)));
            var autoPushingSettings = new Menu("AutoPushing Settings", "AutoPushingSettings");
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.Travels", "Enable travel boots").SetValue(true));
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.TowerPriority", "Tower priority > creep priority").SetValue(true));
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.CheckForEnemyCreeps", "[Travels] Check for enemy creeps").SetValue(true));
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.AutoTargetting", "Do tempest combo").SetValue(true))
                .SetTooltip("if you find any target in attack range");
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.AutoPushingTargettingRange", "Auto Targetting range")
                .SetValue(new Slider(800, 200, 1600))).ValueChanged+= (sender, args) =>
            {
                if (OrderManager.CurrentOrder is AutoPushing && DrawTargettingRange)
                    OrderManager.Orders.AutoPushing.ParticleManager.DrawRange(Core.TempestHero.Hero, "targetting_range",
                        args.GetNewValue<Slider>().Value, Color.White);
            };
            autoPushingSettings.AddItem(new MenuItem("AutoPushing.DrawTargettingRange", "Draw Targetting range")
                .SetValue(false));

            var infoPanel = new Menu("Info Panel", "InfoPanel");
            infoPanel.AddItem(new MenuItem("InfoPanel.Enable", "Enable").SetValue(true)).ValueChanged +=
                InfoPanel.OnChange;
            infoPanel.AddItem(new MenuItem("InfoPanel.Size", "Text Size").SetValue(new Slider(30, 1, 70)));
            infoPanel.AddItem(
                new MenuItem("InfoPanel.Moving", "Move by mouse").SetValue(false));
            infoPanel.AddItem(
                new MenuItem("InfoPanel.X", "Pos X").SetValue(new Slider(10, 0, 2000)));
            infoPanel.AddItem(
                new MenuItem("InfoPanel.Y", "Pos Y").SetValue(new Slider(350, 0, 2000)));
            var mainHero = new Menu("For Main Hero", "mainHero");
            var spellHero = new Menu("Spells:", "HeroSpells");
            var itemHero = new Menu("Items:", "HeroItems");

            var blink = new Menu("Blink Settings", "Blink Settings", textureName: "item_blink",
                showTextWithTexture: true);
            blink.AddItem(
                    new MenuItem("Blink.ExtraRange", "Extra range").SetValue(new Slider(50, 0, 1200)))
                .SetTooltip("blink dist(1200 by def) + this value");
            blink.AddItem(
                    new MenuItem("Blink.ExtraRange.Use", "Use extra range").SetValue(true));
            blink.AddItem(
                    new MenuItem("Blink.BlockSilver", "Dont use silver edge if dagger can be casted").SetValue(true));
            blink.AddItem(
                new MenuItem("Blink.MinRange", "Min range for blink").SetValue(new Slider(400, 0, 1000)));
            blink.AddItem(
                new MenuItem("Blink.ExtraDelay", "Extra delay after blink").SetValue(new Slider(1, 1, 200)));

            var tempest = new Menu("Tempest", "tempest");
            var spellTempest = new Menu("Spells:", "TempestSpells");
            var itemTempest = new Menu("Items:", "TempestItems");
            var dict = AbilityList.ToDictionary(item => item, item => true);
            var dict2 = AbilityList.ToDictionary(item => item, item => true);
            var itemListHero = Items.Keys.ToList().ToDictionary(item => item, item => true);
            var itemListTempest = Items.Keys.ToList().ToDictionary(item => item, item => true);
            itemHero.AddItem(
                new MenuItem("itemHeroEnable", "").SetValue(new AbilityToggler(new Dictionary<string, bool>())));
                //new MenuItem("itemHeroEnable", "").SetValue(new AbilityToggler(itemListHero)));
            itemHero.AddItem(new MenuItem("customOrderHero", "Use Custom Order").SetValue(false));
            itemHero.AddItem(new MenuItem("itemHero", "").SetValue(new PriorityChanger(new List<string>())));
            //itemHero.AddItem(new MenuItem("itemHero", "").SetValue(new PriorityChanger(Items.Keys.ToList())));
            itemTempest.AddItem(
                new MenuItem("itemTempestEnable", "").SetValue(new AbilityToggler(new Dictionary<string, bool>())));
                //new MenuItem("itemTempestEnable", "").SetValue(new AbilityToggler(itemListTempest)));
            itemTempest.AddItem(new MenuItem("customOrderTempest", "Use Custom Order").SetValue(false));
            itemTempest.AddItem(
                new MenuItem("itemTempest", "").SetValue(new PriorityChanger(new List<string>())));
                //new MenuItem("itemTempest", "").SetValue(new PriorityChanger(Items.Keys.ToList())));

            spellHero.AddItem(new MenuItem("spellHero", "").SetValue(new AbilityToggler(dict)));
            spellTempest.AddItem(new MenuItem("spellTempest", "").SetValue(new AbilityToggler(dict2)));

            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages ingame").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Text2.enable", "Debug messages in console").SetValue(false));


            settings.AddSubMenu(usages);
            settings.AddSubMenu(keys);
            settings.AddSubMenu(panels);
            settings.AddSubMenu(autoPushingSettings);


            usages.AddSubMenu(mainHero);
            usages.AddSubMenu(tempest);
            usages.AddSubMenu(blink);
            usages.AddSubMenu(fluxSettings);
            usages.AddSubMenu(sparkSettings);

            mainHero.AddSubMenu(spellHero);
            mainHero.AddSubMenu(itemHero);

            tempest.AddSubMenu(spellTempest);
            tempest.AddSubMenu(itemTempest);


            panels.AddSubMenu(itemPanel);
            panels.AddSubMenu(infoPanel);
            panels.AddSubMenu(pushLaneSelectorPanel);


            Menu.AddSubMenu(settings);
            Menu.AddSubMenu(devolper);
            Menu.AddToMainMenu();
        }

        public static void AddNewItem(AbilityId item)
        {
            var name = item.ToString();
            Menu.Item("itemHeroEnable").GetValue<AbilityToggler>().Add(name);
            Menu.Item("itemTempestEnable").GetValue<AbilityToggler>().Add(name);
            Menu.Item("itemHero").GetValue<PriorityChanger>().Add(name);
            Menu.Item("itemTempest").GetValue<PriorityChanger>().Add(name);
            Printer.Log($"Add new item -> {item}");
        }
        public static void RemoveOldItem(AbilityId item)
        {
            var name = item.ToString();
            Menu.Item("itemHeroEnable").GetValue<AbilityToggler>().Remove(name);
            Menu.Item("itemTempestEnable").GetValue<AbilityToggler>().Remove(name);
            Menu.Item("itemHero").GetValue<PriorityChanger>().Remove(name);
            Menu.Item("itemTempest").GetValue<PriorityChanger>().Remove(name);
            Printer.Log($"Remove old item -> {item}");
        }

        private static float GetSlider(string item)
        {
            return Menu.Item(item).GetValue<Slider>().Value;
        }

        private static bool GetKey(string item)
        {
            return Menu.Item(item).GetValue<KeyBind>().Active;
        }
        private static uint GetKeyId(string item)
        {
            return Menu.Item(item).GetValue<KeyBind>().Key;
        }

        private static bool GetBool(string item)
        {
            return Menu.Item(item).GetValue<bool>();
        }

        private static bool GetToggle(string name, string item)
        {
            return Menu.Item(name).GetValue<AbilityToggler>().IsEnabled(item);
        }

        private static bool GetToggle(string name, AbilityId item)
        {
            return Menu.Item(name).GetValue<AbilityToggler>().IsEnabled(item.ToString());
        }

        private static uint GetPriority(string name, AbilityId item)
        {
            return Menu.Item(name).GetValue<PriorityChanger>().GetPriority(item.ToString());
        }
    }
}