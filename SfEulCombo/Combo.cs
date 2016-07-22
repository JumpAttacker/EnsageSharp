using System;
using System.Globalization;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects.UtilityObjects;

namespace SfEulCombo
{
    public static class Combo
    {
        public static Sleeper EulSleeper;
        public static Sleeper BlinkSleeper;
        public static Sleeper MoveSleeper;
        public static Sleeper UltimateSleeper;

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!Members.Menu.Item("combo.Key").GetValue<KeyBind>().Active)
            {
                Members.Target = null;
                return;
            }

            if (Game.IsPaused)
                return;

            if (!Members.Ultimate.CanBeCasted())
                return;

            if (Members.Target == null || !Members.Target.IsValid || !Members.Target.IsAlive)
            {
                Members.Target = TargetSelector.ClosestToMouse(Members.MyHero);
                return;
            }

            if (Members.Eul == null || !Members.Eul.IsValid)
                Members.Eul = Members.MyHero.FindItem("item_cyclone");

            if (Members.Blink == null || !Members.Blink.IsValid)
                Members.Blink = Members.MyHero.FindItem("item_blink");

            var distance = Members.Target.NetworkPosition.Distance2D(Members.MyHero.NetworkPosition);
            if (Members.Eul != null && Members.Eul.CanBeCasted() && !EulSleeper.Sleeping)
            {
                if (distance >= 1200)
                {
                    if (MoveSleeper.Sleeping) return;
                    MoveSleeper.Sleep(250);
                    Members.MyHero.Move(Members.Target.Position);
                    return;
                }
                EulSleeper.Sleep(250);
                Members.Eul.UseAbility(Members.Target);
                return;
            }
            var modifier = Members.Target.FindModifier("modifier_eul_cyclone");
            var ping = Game.Ping/1000;
            if (Members.Ultimate.IsInAbilityPhase)
            {
                return;
            }
            if (modifier == null)
            {
                return;
            }
            if (modifier.RemainingTime - ping < 1.1)
            {
                return;
            }
            if (Members.Blink != null && Members.Blink.CanBeCasted() && !BlinkSleeper.Sleeping)
            {
                BlinkSleeper.Sleep(250);
                Members.Blink.UseAbility(Members.Target.NetworkPosition);
            }
            else
            {
                var extraTime = Members.Menu.Item("settings.ExtraTime").GetValue<Slider>().Value/1000;
                if (distance >= 60)
                {
                    if (MoveSleeper.Sleeping) return;
                    MoveSleeper.Sleep(250);
                    Members.MyHero.Move(Members.Target.NetworkPosition);
                }
                else if (modifier.RemainingTime - ping -
                         extraTime <= 1.67 && !UltimateSleeper.Sleeping)
                {
                    Printer.Print((modifier.RemainingTime - ping -
                         extraTime).ToString(CultureInfo.InvariantCulture));
                    UltimateSleeper.Sleep(250);
                    Members.Ultimate.UseAbility();
                }
            }
        }
    }
}