using Ensage.Common.Menu;
using SfAnnihilation.DrawingStuff;
using SharpDX;

namespace SfAnnihilation
{
    internal class MenuManager
    {
        public static readonly Menu Menu = new Menu("Shadow Fiend Annihilation", "SfAnnihilation", true, "npc_dota_hero_nevermore", true);
        public static int EulExtraTime => Menu.Item("eul.ExtraTime").GetValue<Slider>().Value;
        public static bool EulComboIsActive => Menu.Item("eul.Key").GetValue<KeyBind>().Active;
        public static bool ComboIsActive => Menu.Item("combo.Key").GetValue<KeyBind>().Active;
        public static bool AimIsActive => Menu.Item("aim.Key").GetValue<KeyBind>().Active;
        public static bool ShadowBladeKey => Menu.Item("sb.Key").GetValue<KeyBind>().Active;
        public static bool BkbIsEulCombo => Menu.Item("eul.bkb.Key").GetValue<KeyBind>().Active;
        public static bool DebugInGame => Menu.Item("Dev.Text.enable").GetValue<bool>();
        public static bool DebugInConsole => Menu.Item("Dev.Text2.enable").GetValue<bool>();
        //public static bool OrbWalkType => Menu.Item("combo.orbwalking.followTarget").GetValue<bool>();
        public static bool AimKillStealOnly => Menu.Item("aim.OnlyKillSteal").GetValue<bool>();
        public static bool UseRazeInCombo => Menu.Item("combo.Raze").GetValue<bool>();
        public static bool DrawRazeRange => Menu.Item("Drawing.RazeRange.Enable").GetValue<bool>();
        public static bool DrawBkbStatus => Menu.Item("Drawing.BkbStatusInEulCombo.Enable").GetValue<bool>();
        public static float DrawBkbStatusSize => Menu.Item("Drawing.BkbStatusInEulCombo.Size").GetValue<Slider>().Value;
        public static float InvisRange => Menu.Item("sb.range").GetValue<Slider>().Value;

        public static Vector2 DrawBkbStatusPosition
            =>
                new Vector2(Menu.Item("Drawing.BkbStatusInEulCombo.Position.X").GetValue<Slider>().Value,
                    Menu.Item("Drawing.BkbStatusInEulCombo.Position.Y").GetValue<Slider>().Value);
        public static bool DrawAimStatus => Menu.Item("Drawing.Aim.Enable").GetValue<bool>();
        public static float DrawAimStatusSize => Menu.Item("Drawing.Aim.Size").GetValue<Slider>().Value;

        public static Vector2 DrawAimStatusPosition
            =>
                new Vector2(Menu.Item("Drawing.Aim.Position.X").GetValue<Slider>().Value,
                    Menu.Item("Drawing.Aim.Position.Y").GetValue<Slider>().Value);

        public static void Init()
        {
            Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            var combo = new Menu("Combo", "Combo");
            combo.AddItem(new MenuItem("combo.Key", "Combo Key").SetValue(new KeyBind('F')));
            combo.AddItem(new MenuItem("combo.Raze", "Use ShadowRaze in combo").SetValue(true));
            /*combo.AddItem(new MenuItem("combo.orbwalking.followTarget", "OrbWalking: follow Target?").SetValue(true))
                .SetTooltip("or mouse");*/
            var aim = new Menu("Raze Aim Settings", "Raze Aim Settings");
            aim.AddItem(new MenuItem("aim.Key", "aim Key").SetValue(new KeyBind('Q', KeyBindType.Toggle)));
            aim.AddItem(new MenuItem("aim.OnlyKillSteal", "Only for KillStealing").SetValue(true));
            var shadowBladeCombo = new Menu("ShadowBlade combo", "ShadowBlade combo");
            shadowBladeCombo.AddItem(new MenuItem("sb.Key", "SB Key").SetValue(new KeyBind('0')));
            shadowBladeCombo.AddItem(
                new MenuItem("sb.range", "Safe range for ultimate").SetValue(new Slider(200, 1, 500)));
            var eulsettings = new Menu("Eul Settings", "Eul Settings");
            eulsettings.AddItem(new MenuItem("eul.Key", "Eul Key").SetValue(new KeyBind('G')));
            eulsettings.AddItem(new MenuItem("eul.bkb.Key", "Eul Bkb toggle Key").SetValue(new KeyBind('H', KeyBindType.Toggle)));
            eulsettings.AddItem(new MenuItem("eul.ExtraTime", "Extra Time").SetValue(new Slider(-40, -100, 10)));
            var drawing = new Menu("Drawing", "Drawing");
            var drawingBkb = new Menu("Bkb", "Bkb");
            var drawingAim = new Menu("Aim", "Aim");
            drawing.AddItem(new MenuItem("Drawing.RazeRange.Enable", "Draw Razes range").SetValue(true)).ValueChanged+=RazeDrawing.OnChange;
            drawingBkb.AddItem(new MenuItem("Drawing.BkbStatusInEulCombo.Enable", "Draw Bkb Status in Eul Combo").SetValue(true));
            drawingBkb.AddItem(
                new MenuItem("Drawing.BkbStatusInEulCombo.Position.X", "[Bkb Status]: position X").SetValue(new Slider(
                    100, 1, 2000))).ValueChanged += InfoDrawing.BkbChanger;
            drawingBkb.AddItem(
                new MenuItem("Drawing.BkbStatusInEulCombo.Position.Y", "[Bkb Status]: position Y").SetValue(new Slider(
                    100, 1, 2000))).ValueChanged += InfoDrawing.BkbChanger;
            drawingBkb.AddItem(
                new MenuItem("Drawing.BkbStatusInEulCombo.Size", "[Bkb Status]: Size").SetValue(new Slider(
                    20, 20, 150))).ValueChanged += InfoDrawing.BkbChanger;
            drawingAim.AddItem(new MenuItem("Drawing.Aim.Enable", "Draw Aim Status").SetValue(true));
            drawingAim.AddItem(
                new MenuItem("Drawing.Aim.Position.X", "[Aim Status]: position X").SetValue(new Slider(
                    100, 1, 2000))).ValueChanged += InfoDrawing.AimChanger;
            drawingAim.AddItem(
                new MenuItem("Drawing.Aim.Position.Y", "[Aim Status]: position Y").SetValue(new Slider(
                    100, 1, 2000))).ValueChanged += InfoDrawing.AimChanger;
            drawingAim.AddItem(
                new MenuItem("Drawing.Aim.Size", "[Aim Status]: Size").SetValue(new Slider(
                    20, 20, 150))).ValueChanged += InfoDrawing.AimChanger;
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages ingame").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.Text2.enable", "Debug messages in console").SetValue(false));
            Menu.AddSubMenu(settings);
            settings.AddSubMenu(combo);
            settings.AddSubMenu(eulsettings);
            settings.AddSubMenu(shadowBladeCombo);
            settings.AddSubMenu(aim);
            settings.AddSubMenu(drawing);
            drawing.AddSubMenu(drawingBkb);
            drawing.AddSubMenu(drawingAim);
            Menu.AddSubMenu(devolper);
            Menu.AddToMainMenu();
        }
    }
}