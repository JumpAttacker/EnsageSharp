using System.Reflection;
using Ensage;
using Ensage.Common.Menu;

namespace StormAnnihilation
{
    public static class MenuManager
    {
        public static readonly string Ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly Menu Menu = new Menu("StormAnnihilation", "stormannihilation", true);
        public static float MinRange => Menu.Item("minrange").GetValue<Slider>().Value;
        public static float UltRate => Menu.Item("castUlt").GetValue<Slider>().Value;
        private static bool _init;
        public static bool IsEnable => Menu.Item("hotkey").GetValue<KeyBind>().Active;
        //public static bool Orbwalker => Menu.Item("orbwalker").GetValue<bool>();
        public static bool FollowTarget => Menu.Item("orbwalker.type").GetValue<StringList>().SelectedIndex == 0;
        public static void Init()
        {
            if (_init)
                return;
            _init = true;
            Menu.AddItem(
                new MenuItem("hotkey", "Combo hotkey").SetValue(new KeyBind('G', KeyBindType.Press))
                    .SetTooltip("just hold this key for combo"));
            /*Menu.AddItem(
                new MenuItem("orbwalker", "Enable orbwalking").SetValue(true));
            Menu.AddItem(
                new MenuItem("orbwalker.type", "Type:").SetValue(new StringList(new[] {"follow Target", "follow mouse"})));*/

            Menu.AddItem(
                new MenuItem("castUlt", "Ultimate rate").SetValue(new Slider(500, 300, 4000)).SetTooltip("in ms"));

            Menu.AddItem(
                new MenuItem("minrange", "Minimum range").SetValue(new Slider(0, 0, 1000))
                    .SetTooltip("Minimum range for casting ultimate"));
            Menu.AddToMainMenu();
        }
        public static void Handle()
        {
            Game.OnUpdate += Core.Game_OnUpdate;
        }
        public static void UnHandle()
        {
            if (!_init)
                return;
            Game.OnUpdate -= Core.Game_OnUpdate;
            Menu.RemoveFromMainMenu();
        }
    }
}