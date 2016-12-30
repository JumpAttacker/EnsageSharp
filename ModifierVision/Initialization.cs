using System.Collections.Generic;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;

namespace ModifierVision
{
    internal class Initialization
    {
        public Initialization()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            settings.AddItem(new MenuItem("Enable.Heroes", "Enable for heroes").SetValue(true));
            settings.AddItem(new MenuItem("Enable.Creeps", "Enable for creeps").SetValue(false));
            settings.AddItem(new MenuItem("Enable.Color", "Change color on low values").SetValue(false));
            /*settings.AddItem(
                new MenuItem("abilityToggle.!", "modifiers").SetValue(new AbilityToggler(Members.ModiferDictinary)));*/
            settings.AddItem(new MenuItem("Counter.Hero", "Max modifiers for each Hero").SetValue(new Slider(4, 1, 5)));
            settings.AddItem(new MenuItem("Counter.Creep", "Max modifiers for each Creep").SetValue(new Slider(1, 1, 5)));
            settings.AddItem(new MenuItem("Settings.IconSize", "Icon Size").SetValue(new Slider(25, 10, 100)));
            settings.AddItem(new MenuItem("Settings.TextSize", "Text Size").SetValue(new Slider(65, 1, 100)));
            settings.AddItem(new MenuItem("ExtraPos.X", "Extra Position X").SetValue(new Slider(0, -50, 50)));
            settings.AddItem(new MenuItem("ExtraPos.Y", "Extra Position Y").SetValue(new Slider(0, -50, 50)));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            Members.Menu.AddSubMenu(devolper);
            Events.OnLoad += (sender, args) =>
            {
                Members.MyHero = ObjectManager.LocalHero;
                if (Members.MyHero == null || !Members.MyHero.IsValid)
                {
                    return;
                }
                Printer.Print(
                    "<font face='Comic Sans MS, cursive'><font color='#00aaff'>" + Members.Menu.DisplayName +
                    " By Jumpering" +
                    " loaded!</font> <font color='#aa0000'>v" + Assembly.GetExecutingAssembly().GetName().Version,
                    true, MessageType.LogMessage);
                Printer.PrintSuccess("> " + Members.Menu.DisplayName + " loaded v" +
                                     Assembly.GetExecutingAssembly().GetName().Version);
                Members.System = new List<HeroModifier>();
                Drawing.OnDraw += Action.OnDraw;
                Unit.OnModifierAdded += Action.ModifierAdded;
                Unit.OnModifierRemoved += Action.ModifierRemoved;

                try
                {
                    Members.Menu.AddToMainMenu();
                }
                catch
                {
                    // ignored
                }
            };
            Events.OnClose += (sender, args) =>
            {
                if (Members.System != null)
                    Members.System.Clear();
                Drawing.OnDraw -= Action.OnDraw;
                Unit.OnModifierAdded -= Action.ModifierAdded;
                Unit.OnModifierRemoved -= Action.ModifierRemoved;
                try
                {
                    Members.Menu.RemoveFromMainMenu();
                }
                catch
                {
                    // ignored
                }
            };
        }
    }
}