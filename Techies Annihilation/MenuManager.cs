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
        public static bool LandMineIndicatorEnable => GetBool("Drawing.LandMineStatus.Enable");
        public static double GetLandMineIndicatorDigSize => GetSlider("Drawing.LandMineStatus.Dig.Size")/100f;
        public static float GetLandMineBarSize => GetSlider("Drawing.LandMineStatus.Bar.Size");
        public static bool LandMinesDrawDigs => GetBool("Drawing.LandMineStatus.Digs.Enable");
        public static double GetBombDelay => GetSlider("Settings.Delay")/1000f;

        public static bool IsEnableDelayBlow => GetBool("Settings.Delay.Enable");

        public static void Init()
        {
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            var delay = new Menu("Delay", "Delay");
            settings.AddItem(new MenuItem("Settings.Aegis.Enable", "Detonate in aegis").SetValue(true));
            settings.AddItem(new MenuItem("Settings.ForceStaff.Enable", "Enable ForceStaff").SetValue(true));
            delay.AddItem(new MenuItem("Settings.Delay.Enable", "Enable").SetValue(false));
            delay.AddItem(new MenuItem("Settings.Delay", "Delay bomb activation").SetValue(new Slider(150, 1, 500)));
            var draw = new Menu("Drawing", "Drawing");
            var landMineIndicator = new Menu("Indicator", "Indicator");
            landMineIndicator.AddItem(new MenuItem("Drawing.LandMineStatus.Enable", "Enable LandMine Indicator").SetValue(true));
            landMineIndicator.AddItem(new MenuItem("Drawing.LandMineStatus.Digs.Enable", "Draw [%]").SetValue(true));
            landMineIndicator.AddItem(
                new MenuItem("Drawing.LandMineStatus.Bar.Size", "Indicator size").SetValue(new Slider(13, 5, 30)));
            landMineIndicator.AddItem(new MenuItem("Drawing.LandMineStatus.Dig.Size", "Text size").SetValue(new Slider(100,50,150)));
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
            settings.AddSubMenu(delay);
            settings.AddSubMenu(draw);
            settings.AddSubMenu(perfomance);
            draw.AddSubMenu(topPanel);
            draw.AddSubMenu(landMineIndicator);
            
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