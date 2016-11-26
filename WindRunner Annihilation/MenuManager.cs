using System.Collections.Generic;
using System.Linq;
using Ensage.Common.Menu;
using SharpDX;

namespace WindRunner_Annihilation
{
    public static class MenuManager
    {
        public static void Init()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            var orb = new Menu("Orbwalking", "Orbwalking");
            orb.AddItem(new MenuItem("Orbwalking.Enable", "Enable").SetValue(true));
            orb.AddItem(new MenuItem("Orbwalking.FollowTarget", "Follow Target instead of mouse").SetValue(true));
            orb.AddItem(new MenuItem("Orbwalking.WhileTargetInStun", "Do Not Use orbwalking if target is stunned").SetValue(true));
            settings.AddItem(
                new MenuItem("Combo.Enable", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press))).ValueChanged +=
                Core.OnValueChanged;
            var draw = new Menu("Drawing", "Drawing");
            draw.AddItem(new MenuItem("Range.Shackle.Enable", "Draw range for Shackle").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    if (args.GetNewValue<bool>())
                    {
                        Members.ShacklRange =
                            Members.MyHero.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                        var range = 800;
                        Members.ShacklRange.SetControlPoint(1, new Vector3(range, 255, 0));
                        Members.ShacklRange.SetControlPoint(2, new Vector3(0, 155, 255));
                    }
                    else
                    {
                        if (Members.ShacklRange != null && Members.ShacklRange.IsValid && !Members.ShacklRange.IsDestroyed)
                            Members.ShacklRange.Dispose();
                    }
                };
            draw.AddItem(new MenuItem("Range.ShackleAoE.Enable", "Draw range for Shackle's AoE").SetValue(true));
            draw.AddItem(new MenuItem("Range.Lines.Enable", "Draw Lines").SetValue(true));

            var items = new Menu("Using", "Items");

            items.AddItem(
                new MenuItem("abilityEnable", "Abilities in combo:").SetValue(
                    new AbilityToggler(Members.AbilityList.ToDictionary(item => item, item => true))));
            items.AddItem(
                new MenuItem("itemEnable", "Items in combo:").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            items.AddItem(
                new MenuItem("itemEnableLinken", "Linken breaker:").SetValue(
                    new AbilityToggler(new Dictionary<string, bool>())));
            items.AddItem(new MenuItem("Blink.UltimateCheck", "Blink on ultimate range").SetValue(true));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            devolper.AddItem(new MenuItem("Dev.DebugLines.enable", "Draw lines").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            settings.AddSubMenu(items);
            settings.AddSubMenu(orb);
            settings.AddSubMenu(draw);
            Members.Menu.AddSubMenu(devolper);
            Members.Menu.AddToMainMenu();
            if (Members.Menu.Item("Range.Shackle.Enable").GetValue<bool>())
            {
                Members.Menu.Item("Range.Shackle.Enable").SetValue(false);
                Members.Menu.Item("Range.Shackle.Enable").SetValue(true);
            }
        }
    }
}