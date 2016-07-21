using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace SfEulCombo
{
    public class Initialization
    {
        public Initialization()
        {
            Combo.EulSleeper = new Sleeper();
            Combo.BlinkSleeper = new Sleeper();
            Combo.MoveSleeper = new Sleeper();
            Combo.UltimateSleeper = new Sleeper();
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            settings.AddItem(new MenuItem("combo.Key", "Key").SetValue(new KeyBind('F',KeyBindType.Press)));
            settings.AddItem(new MenuItem("settings.ExtraTime", "Extra Time").SetValue(new Slider(0,-50,50)));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            Members.Menu.AddSubMenu(devolper);
            Events.OnLoad += (sender, args) =>
            {
                Members.MyHero = ObjectManager.LocalHero;
                if (Members.MyHero == null || !Members.MyHero.IsValid ||
                    Members.MyHero.ClassID != ClassID.CDOTA_Unit_Hero_Nevermore)
                {
                    return;
                }
                Members.Ultimate = Members.MyHero.Spellbook.Spell6;
                Printer.Print(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Members.Menu.DisplayName +
                    " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    true, MessageType.LogMessage);
                Printer.PrintSuccess("> " + Members.Menu.DisplayName + " loaded v" +
                                     Assembly.GetExecutingAssembly().GetName().Version);
                Game.OnUpdate += Combo.Game_OnUpdate;
                Members.Menu.AddToMainMenu();
            };
            Events.OnClose += (sender, args) =>
            {
                Members.Eul = null;
                Members.Blink = null;
                Members.Ultimate = null;
                Game.OnUpdate -= Combo.Game_OnUpdate;
                Members.Menu.RemoveFromMainMenu();
            };
        }
    }
}