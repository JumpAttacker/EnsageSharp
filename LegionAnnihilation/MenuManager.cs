using System;
using System.Collections.Generic;
using System.Linq;
using Ensage.Common;
using Ensage.Common.Menu;
using SharpDX;

namespace Legion_Annihilation
{
    public static class MenuManager
    {
        public static void Init()
        {
            Members.Menu.AddItem(new MenuItem("Enable", "Enable").SetValue(true));
            var settings = new Menu("Settings", "Settings");
            var orb = new Menu("Orbwalking", "Orbwalking");
            var invisibility = new Menu("Invisibility", "Invis", false, "item_silver_edge", true);
            orb.AddItem(new MenuItem("Orbwalking.Enable", "Enable").SetValue(true));
            orb.AddItem(new MenuItem("Orbwalking.FollowTarget", "Follow Target instead of mouse").SetValue(true));
            orb.AddItem(new MenuItem("Orbwalking.WhileTargetInStun", "Do Not Use orbwalking if target is stunned").SetValue(true));
            settings.AddItem(
                new MenuItem("Combo.Enable", "Combo key").SetValue(new KeyBind('D', KeyBindType.Press))).ValueChanged +=
                Core.OnValueChanged;
            var draw = new Menu("Drawing", "Drawing");
            draw.AddItem(new MenuItem("Drawing.DrawBkbStatus", "Draw bkb status").SetValue(true));
            draw.AddItem(new MenuItem("Drawing.DrawBkbStatus.X", "[Bkb] posX").SetValue(new Slider(0,0,1800)));
            draw.AddItem(new MenuItem("Drawing.DrawBkbStatus.Y", "[Bkb] posY").SetValue(new Slider(0,0,1800)));
            draw.AddItem(new MenuItem("Drawing.DrawBkbStatus.Size", "[Bkb] size").SetValue(new Slider(20,1,200)));
            draw.AddItem(new MenuItem("Range.Blink.Enable", "Draw range for Blink").SetValue(true)).ValueChanged +=
                (sender, args) =>
                {
                    if (args.GetNewValue<bool>())
                    {
                        Members.BlinkRange =
                            Members.MyHero.AddParticleEffect("materials/ensage_ui/particles/range_display_mod.vpcf");
                        var range = 1200;
                        Members.BlinkRange.SetControlPoint(1, new Vector3(range, 255, 0));
                        Members.BlinkRange.SetControlPoint(2, new Vector3(0, 155, 255));
                    }
                    else
                    {
                        if (Members.BlinkRange != null && Members.BlinkRange.IsValid && !Members.BlinkRange.IsDestroyed)
                            Members.BlinkRange.Dispose();
                    }
                };
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
            items.AddItem(
                new MenuItem("Bkb.Toggle", "BKB toggle").SetValue(new KeyBind('0', KeyBindType.Toggle))).ValueChanged +=
                Core.BkbToggler;
            invisibility.AddItem(new MenuItem("UseHealBeforeInvis.Enable", "Use heal before invis").SetValue(true));
            invisibility.AddItem(
                new MenuItem("InvisRange.value", "Min distance for cast invis->heal->items").SetValue(
                    new Slider(1500, 500, 3500)));
            var devolper = new Menu("Developer", "Developer");
            devolper.AddItem(new MenuItem("Dev.Text.enable", "Debug messages").SetValue(false));
            Members.Menu.AddSubMenu(settings);
            settings.AddSubMenu(items);
            items.AddSubMenu(invisibility);
            settings.AddSubMenu(orb);
            settings.AddSubMenu(draw);
            Members.Menu.AddSubMenu(devolper);
            Members.Menu.AddToMainMenu();
            DelayAction.Add(1000, () =>
            {
                try
                {
                    if (Members.Menu.Item("Range.Blink.Enable").GetValue<bool>())
                    {
                        Members.Menu.Item("Range.Blink.Enable").SetValue(false);
                        Members.Menu.Item("Range.Blink.Enable").SetValue(true);
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }
    }
}