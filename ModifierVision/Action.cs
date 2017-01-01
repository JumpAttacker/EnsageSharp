using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace ModifierVision
{
    internal class Action
    {
        private static bool Checker => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool ChangeColor => Members.Menu.Item("Enable.Color").GetValue<bool>();

        public static void OnDraw(EventArgs args)
        {
            if (!Checker)
                return;
            var refresh = Members.System.ToList();
            foreach (var heroModifier in refresh.Where(x=> x.Owner == null || !x.Owner.IsValid || !x.Owner.IsAlive))
            {
                Members.System.Remove(heroModifier);
            }
            foreach (var heroModifier in Members.System)
            {
                var target = heroModifier.Owner;
                var modList = heroModifier.Modifiers;
                var isHero = heroModifier.IsHero;
                var maxCounter = isHero
                    ? Members.Menu.Item("Counter.Hero").GetValue<Slider>().Value
                    : Members.Menu.Item("Counter.Creep").GetValue<Slider>().Value;
                if (isHero)
                {
                    if (!Members.Menu.Item("Enable.Heroes").GetValue<bool>())
                        continue;
                }
                else
                {
                    if (!Members.Menu.Item("Enable.Creeps").GetValue<bool>())
                        continue;
                }
                var counter = 0;
                var extra = isHero ? 0 : 5;
                var hudPos = HUDInfo.GetHPbarPosition(target);
                if (hudPos.IsZero)
                    continue;
                var startPos = hudPos + new Vector2(0, HUDInfo.GetHpBarSizeY()*2+extra) +
                               new Vector2(Members.Menu.Item("ExtraPos.X").GetValue<Slider>().Value,
                                   Members.Menu.Item("ExtraPos.Y").GetValue<Slider>().Value);
                var size = new Vector2(Members.Menu.Item("Settings.IconSize").GetValue<Slider>().Value,
                    Members.Menu.Item("Settings.IconSize").GetValue<Slider>().Value);
                foreach (var modifier in modList.Where(x=>x!=null && x.IsValid && !Members.BlackList.Contains(x.Name)))
                {
                    if (counter >= maxCounter)
                        continue;
                    var remTime = modifier.RemainingTime;
                    /*if (remTime<=1)
                        continue;*/
                    var itemPos = startPos + new Vector2(-2 + size.X*counter, 2);
                    Drawing.DrawRect(itemPos, size,
                        Textures.GetTexture($"materials/ensage_ui/modifier_textures/{modifier.TextureName}.vmat"));
                    Drawing.DrawRect(itemPos, size,
                        Color.Black,true);
                    var timer = Math.Min(remTime+0.1, 99).ToString("0.0");
                    var textSize = Drawing.MeasureText(timer, "Arial",
                        new Vector2(
                            (float) (size.Y*Members.Menu.Item("Settings.TextSize").GetValue<Slider>().Value/100),
                            size.Y/2), FontFlags.AntiAlias);
                    var textPos = itemPos + new Vector2(0, size.Y - textSize.Y);
                    Drawing.DrawRect(textPos - new Vector2(0, 0),
                        new Vector2(textSize.X, textSize.Y),
                        new Color(0, 0, 0, 200));
                    var clr = remTime >= 1 ? Color.White : ChangeColor ? Color.Red : Color.White;
                    Drawing.DrawText(
                        timer,
                        textPos,
                        new Vector2(textSize.Y, 0),
                        clr,
                        FontFlags.AntiAlias | FontFlags.StrikeOut);
                    counter++;
                }
            }
        }

        private static bool AddToSystem(Unit target, Modifier mod)
        {
            var finder = Members.System.Find(modifier => modifier.Owner.Equals(target));
            if (finder == null)
            {
                Members.System.Add(new HeroModifier(target, mod));
                return true;
            }
            if (finder.Modifiers!=null && finder.Modifiers.Any(x => x!=null && x.IsValid && x.Name == mod.Name))
                return false;
            finder.Add(mod);
            return true;
        }
        private static bool RemoveFromSystem(Unit target, Modifier mod)
        {
            var finder = Members.System.Find(modifier => modifier.Owner.Equals(target));
            if (finder == null)
            {
                return false;
            }
            finder.Remove(mod);
            return true;
        }

        public static void ModifierAdded(Unit sender, ModifierChangedEventArgs args)
        {
            if (!Checker)
                return;
            if (sender == null || !sender.IsValid || args.Modifier == null || !args.Modifier.IsValid)
                return;
            var modifier = args.Modifier;
            if (Members.BlackList.Contains(modifier.Name))
                return;
            if (modifier.RemainingTime<=1 && !Members.WhiteList.Contains(modifier.Name)) 
                return;
            /*var name = modifier.Name.Substring(9);
            if (Members.Menu.Item("abilityToggle.!").GetValue<AbilityToggler>().Dictionary.ContainsKey(name))
            {
                if (!Members.Menu.Item("abilityToggle.!").GetValue<AbilityToggler>().IsEnabled(name))
                {
                    return;
                }
            }
            else
            {
                Members.Menu.Item("abilityToggle.!").GetValue<AbilityToggler>().Add(name);
                if (!Members.Menu.Item("abilityToggle.!").GetValue<AbilityToggler>().IsEnabled(name))
                {
                    return;
                }
            }*/
            if (sender is Hero)
            {
                if (Members.Menu.Item("Enable.Heroes").GetValue<bool>())
                {
                    var a = AddToSystem(sender, modifier);
                    Printer.Print($"[Add]Hero: {sender.Name} [{a}] --> {modifier.Name}");
                }
            }
            else if (Members.Menu.Item("Enable.Creeps").GetValue<bool>())
            {
                var a = AddToSystem(sender, modifier);
                Printer.Print("[Add]Creep: " + a);
            }
        }

        public static void ModifierRemoved(Unit sender, ModifierChangedEventArgs args)
        {
            if (sender == null || !sender.IsValid || args.Modifier == null || !args.Modifier.IsValid)
                return;
            if (RemoveFromSystem(sender, args.Modifier))
                Printer.Print("[Remove]: " + sender.StoredName());
        }
    }
}