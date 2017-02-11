using System.Collections.Generic;
using Ensage;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace MorphlingAnnihilation
{
    public static class MenuManager
    {
        public static Menu Menu;
        public static int GetSize => Menu.Item("drawing.GetSize").GetValue<Slider>().Value;

        public static Vector2 GetPosition
            =>
                new Vector2(Menu.Item("drawing.pos.X").GetValue<Slider>().Value,
                    Menu.Item("drawing.pos.Y").GetValue<Slider>().Value);
        public static bool IsEnable => Menu.Item("Menu.Enable").GetValue<bool>();
        public static bool IsEnableDebugger => Menu.Item("Dev.Text.enable").GetValue<bool>();
        public static bool SafeTp => Menu.Item("safetp").GetValue<bool>();
        public static int MinHpForSafeTp => Menu.Item("minHpForSafeTp").GetValue<Slider>().Value;
        public static bool LockTarget => Menu.Item("LockTarget").GetValue<bool>();
        public static bool AutoBalance => Menu.Item("AutoBalance").GetValue<bool>();
        public static int MorphGetMinHealth => Menu.Item("minHp").GetValue<Slider>().Value;
        public static int MorphGetMinHealthPercent => Menu.Item("minHpPerc").GetValue<Slider>().Value;
        public static int MorphGetMinMana => Menu.Item("minMp").GetValue<Slider>().Value;
        public static bool AllComboKey => Menu.Item("all.hotkey").GetValue<KeyBind>().Active;
        public static bool HeroComboKey => Menu.Item("hero.hotkey").GetValue<KeyBind>().Active;
        public static bool ReplicateComboKey => Menu.Item("replicate.hotkey").GetValue<KeyBind>().Active;
        public static bool HybridComboKey => Menu.Item("hybrid.hotkey").GetValue<KeyBind>().Active;
        public static int GetComboBehavior => Menu.Item("ComboBehavior").GetValue<StringList>().SelectedIndex;

        public static bool IsItemEnable(string item)
            =>
                Menu.Item("itemEnable")
                    .GetValue<AbilityToggler>()
                    .IsEnabled(item.Contains("dagon") ? "item_dagon" : item);

        public static bool IsAbilityEnable(string item)
            =>
                Menu.Item("abilityEnable").GetValue<AbilityToggler>().IsEnabled(item);

        public static void Init()
        {
            Menu = new Menu("Morphling Annihilation", "morph", true, "npc_dota_hero_morphling", true);
            var settings = new Menu("Settings", "Settings");
            var combo = new Menu("Combo Settings", "Combo Settings");
            var hero = new Menu("Hero", "Hero");
            var replicate = new Menu("Replicate", "Replicate");
            var hybrid = new Menu("Hybrid", "Hybrid");
            var usage = new Menu("Using", "Using");
            var draw = new Menu("Drawing", "Drawing");
            draw.AddItem(new MenuItem("draw.Enable", "Enable").SetValue(true));
            draw.AddItem(new MenuItem("drawing.pos.X", "Pos X").SetValue(new Slider(14, 0, 2000)));
            draw.AddItem(new MenuItem("drawing.pos.Y", "Pos Y").SetValue(new Slider(130, 0, 2000)));
            draw.AddItem(new MenuItem("drawing.GetSize", "Size").SetValue(new Slider(30, 5, 30)));
            Menu.AddItem(new MenuItem("Menu.Enable", "Enable").SetValue(true));
            combo.AddItem(new MenuItem("all.hotkey", "Global Combo Key").SetValue(new KeyBind('F', KeyBindType.Press)))
                .SetTooltip("for hero, hybrid & replicate").ValueChanged += Core.OnValueChanged;
            combo.AddItem(new MenuItem("LockTarget", "LockTarget").SetValue(true));
            combo.AddItem(
                new MenuItem("ComboBehavior", "Combo Behavior").SetValue(
                    new StringList(new[] {"follow target", "follow mouse"})));
            hero.AddItem(new MenuItem("hero.hotkey", "HotKey").SetValue(new KeyBind('0', KeyBindType.Press)));
            replicate.AddItem(new MenuItem("replicate.hotkey", "HotKey").SetValue(new KeyBind('Z', KeyBindType.Toggle)))
                .ValueChanged += Core.OnValueChanged;
            hybrid.AddItem(new MenuItem("hybrid.hotkey", "HotKey").SetValue(new KeyBind('X', KeyBindType.Toggle)))
                .ValueChanged += Core.OnValueChanged;

            var dick = new Dictionary<string, bool>
            {
                {"item_ethereal_blade",true},
                {"morphling_adaptive_strike",true},
                {"item_dagon",true},
                {"morphling_waveform",true}
            };
            usage.AddItem(new MenuItem("itemEnable", "Abilities in combo:").SetValue(new AbilityToggler(dick)));

            var autoBalance = new Menu("Auto balance", "Auto balance", false, "morphling_morph_agi", true);
            autoBalance.AddItem(new MenuItem("AutoBalance", "Auto balance").SetValue(true));
            autoBalance.AddItem(new MenuItem("minHp", "Minimum Health").SetValue(new Slider(100, 100, 5000)));
            autoBalance.AddItem(new MenuItem("minHpPerc", "Minimum Health percent").SetValue(new Slider(0)));
            autoBalance.AddItem(new MenuItem("minMp", "Minimum Mana percent").SetValue(new Slider(0)));
            

            var safetp = new Menu("Safe Tp", "Safetpout", false, "morphling_morph_replicate", true);
            safetp.AddItem(new MenuItem("safetp", "Use replicate on low hp").SetValue(true));
            safetp.AddItem(new MenuItem("minHpForSafeTp", "Minimum HP").SetValue(new Slider(100, 100, 5000)));

            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));



            settings.AddSubMenu(usage);
            settings.AddSubMenu(combo);


            combo.AddSubMenu(hero);
            combo.AddSubMenu(replicate);
            combo.AddSubMenu(hybrid);

            settings.AddSubMenu(autoBalance);
            settings.AddSubMenu(safetp);
            settings.AddSubMenu(draw);


            Menu.AddSubMenu(settings);
            Menu.AddSubMenu(devolper);
        }

        

        public static void Handle()
        {
            Menu.AddToMainMenu();
            Members.Updater = new Sleeper();
            Members.MainHero = ObjectManager.LocalHero;
            Members.MainPlayer = ObjectManager.LocalPlayer;
            Members.MyTeam = ObjectManager.LocalPlayer.Team;
            Game.OnUpdate += Core.Updater;
            Game.OnUpdate += Core.UpdateLogic;
            Drawing.OnDraw += Draw.OnDrawing;
            //Drawing.OnEndScene += Draw.OnDrawingEndScene;
        }
        public static void UnHandle()
        {
            Game.OnUpdate -= Core.Updater;
            Game.OnUpdate -= Core.UpdateLogic;
            Drawing.OnDraw -= Draw.OnDrawing;
            Menu.RemoveFromMainMenu();
        }
    }
}