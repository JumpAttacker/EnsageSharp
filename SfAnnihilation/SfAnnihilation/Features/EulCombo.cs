using System;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Utils;
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace SfAnnihilation.Features
{
    internal class EulCombo
    {
        private static readonly Hero Me = ObjectManager.LocalHero;
        private static Hero _target;
        private static Item _eul;
        private static Item _blink;
        private static Item _veil;
        private static Item _bkb;
        private static readonly Ability Ultimate = Me.GetAbilityById(AbilityId.nevermore_requiem);
        private static readonly Sleeper MoveSleeper = new Sleeper();
        private static readonly Sleeper EulSleeper = new Sleeper();
        private static readonly Sleeper UltimateSleeper = new Sleeper();
        private static readonly Sleeper BlinkSleeper = new Sleeper();
        private static readonly MultiSleeper ExtSleeper = new MultiSleeper();
        public static void OnUpdate(EventArgs args)
        {
            if (!MenuManager.EulComboIsActive)
            {
                _target = null;
                return;
            }

            if (Game.IsPaused)
                return;
            
            if (!Ultimate.CanBeCasted())
                return;

            if (_target == null || !_target.IsValid || !_target.IsAlive)
            {
                _target = TargetSelector.ClosestToMouse(Me);
                return;
            }
            if (_target.IsLinkensProtected() || _target.HasModifier("modifier_item_lotus_orb_active"))
                return;
            if (_eul == null || !_eul.IsValid)
                _eul = Me.GetItemById(ItemId.item_cyclone);
            if (_veil == null || !_veil.IsValid)
                _veil = Me.GetItemById(ItemId.item_veil_of_discord);
            if (_bkb == null || !_bkb.IsValid)
                _bkb = Me.GetItemById(ItemId.item_black_king_bar);
            if (_blink == null || !_blink.IsValid)
                _blink = Me.GetItemById(ItemId.item_blink);

            var distance = _target.NetworkPosition.Distance2D(Me.NetworkPosition);
            if (_eul != null && _eul.CanBeCasted() && !EulSleeper.Sleeping)
            {
                if (_veil!=null && _veil.CanBeCasted() && !ExtSleeper.Sleeping(_veil) && _veil.CanHit(_target))
                {
                    _veil.UseAbility(_target.Position);
                    ExtSleeper.Sleep(500, _veil);
                }
                if (distance >= 1200)
                {
                    if (MoveSleeper.Sleeping) return;
                    MoveSleeper.Sleep(250);
                    Me.Move(_target.Position);
                    return;
                }
                EulSleeper.Sleep(250);
                _eul.UseAbility(_target);
                return;
            }
            var modifier = _target.FindModifier("modifier_eul_cyclone");
            var ping = Game.Ping / 1000;
            if (Ultimate.IsInAbilityPhase)
            {
                return;
            }
            if (modifier == null)
            {
                return;
            }
            if (_bkb != null && _bkb.CanBeCasted() && !ExtSleeper.Sleeping(_bkb) && MenuManager.BkbIsEulCombo)
            {
                _bkb.UseAbility();
                ExtSleeper.Sleep(500, _bkb);
            }
            if (modifier.RemainingTime - ping < 1.1)
            {
                return;
            }
            if (_blink != null && _blink.CanBeCasted() && !BlinkSleeper.Sleeping)
            {
                BlinkSleeper.Sleep(250);
                _blink.UseAbility(_target.NetworkPosition);
            }
            else
            {
                var extraTime = MenuManager.EulExtraTime / 1000;
                if (distance >= 60)
                {
                    if (MoveSleeper.Sleeping) return;
                    MoveSleeper.Sleep(250);
                    Me.Move(_target.NetworkPosition);
                }
                else if (modifier.RemainingTime - ping -
                         extraTime <= 1.67 && !UltimateSleeper.Sleeping)
                {
                    UltimateSleeper.Sleep(250);
                    Ultimate.UseAbility();
                }
            }
        }
    }
}