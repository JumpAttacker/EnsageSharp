using Ensage.Common.Menu;
using SharpDX;
using SharpDX.Direct2D1;

namespace Techies_Annihilation
{
    internal class MenuManager
    {
        public static readonly Menu Menu = new Menu("Techies Annihilation", "Techies Annihilation", true,
            "npc_dota_hero_techies", true);
        public static bool DebugInGame => Menu.Item("Dev.Text.enable").GetValue<bool>();
        public static bool DebugInConsole => Menu.Item("Dev.Text2.enable").GetValue<bool>();
        public static float GetUpdateSpeed => Menu.Item("Performance.tickRate").GetValue<Slider>().Value;

        public static Vector2 GetExtraPosForTopPanel
            =>
                new Vector2(Menu.Item("TopPanel.Extra.X").GetValue<Slider>().Value,
                    Menu.Item("TopPanel.Extra.Y").GetValue<Slider>().Value);

        public static Vector2 GetTopPanelExtraSize
            => new Vector2(Menu.Item("TopPanel.Size").GetValue<Slider>().Value);

        public static bool CheckForAegis => Menu.Item("Settings.Aegis.Enable").GetValue<bool>();

        public static bool IsEnableForceStaff => GetBool("Settings.ForceStaff.Enable");

        public static void Init()
        {
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            settings.AddItem(new MenuItem("Settings.Aegis.Enable", "Detonate in aegis").SetValue(true));
            settings.AddItem(new MenuItem("Settings.ForceStaff.Enable", "Enable ForceStaff").SetValue(true));
            var draw = new Menu("Drawing", "Drawing");
            /*draw.AddItem(new MenuItem("Drawing.Range.LandMine", "Range for LandMine").SetValue(true));
            draw.AddItem(new MenuItem("Drawing.Range.StaticTrap", "Range for StaticTrap").SetValue(true));
            draw.AddItem(new MenuItem("Drawing.Range.RemoteMine", "Range for RemoteMine").SetValue(true));*/
            var topPanel = new Menu("TopPanel", "TopPanel");
            topPanel.AddItem(
                new MenuItem("TopPanel.Extra.X", "Extra Position X").SetValue(new Slider(0, -150, 150)));
            topPanel.AddItem(
                new MenuItem("TopPanel.Extra.Y", "Extra Position Y").SetValue(new Slider(0, -150, 150)));
            topPanel.AddItem(
                new MenuItem("TopPanel.Size", "Size").SetValue(new Slider(0, -50, 50)));
            var perfomance = new Menu("Performance", "Performance");
            perfomance.AddItem(
                new MenuItem("Performance.tickRate", "Damage Update Rate (Drawing)").SetValue(new Slider(500, 50, 1000))).SetTooltip("in ms");
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages ingame").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Text2.enable", "Debug messages in console").SetValue(false));
            Menu.AddSubMenu(settings);
            settings.AddSubMenu(draw);
            settings.AddSubMenu(perfomance);
            draw.AddSubMenu(topPanel);
            
            Menu.AddSubMenu(devolper);
            Menu.AddToMainMenu();
        }

        private static float GetSlider(string item)
        {
            return Menu.Item(item).GetValue<Slider>().Value;
        }
        private static bool GetKey(string item)
        {
            return Menu.Item(item).GetValue<KeyBind>().Active;
        }
        private static bool GetBool(string item)
        {
            return Menu.Item(item).GetValue<bool>();
        }
    }
}