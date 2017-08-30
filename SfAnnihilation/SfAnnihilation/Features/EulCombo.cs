using System;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Objects.UtilityObjects;
using SfAnnihilation.Utils;
using SharpDX;
using AbilityId = Ensage.AbilityId;

//using AbilityId = Ensage.Common.Enums.AbilityId;

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
        private static Item _eb;
        private static readonly Ability Ultimate = Me.GetAbilityById(AbilityId.nevermore_requiem);
        private static readonly Sleeper MoveSleeper = new Sleeper();
        private static readonly Sleeper EulSleeper = new Sleeper();
        private static readonly Sleeper UltimateSleeper = new Sleeper();
        private static readonly Sleeper BlinkSleeper = new Sleeper();
        private static readonly MultiSleeper ExtSleeper = new MultiSleeper();
        public static void OnUpdate(EventArgs args)
        {
            return;
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
            if (_eb == null || !_eb.IsValid)
                _eb = Me.GetItemById(ItemId.item_ethereal_blade);

            var distance = _target.NetworkPosition.Distance2D(Me.NetworkPosition);
            if (_eul != null && _eul.CanBeCasted() && !EulSleeper.Sleeping)
            {
                if (_eb != null && _eb.CanBeCasted() && !ExtSleeper.Sleeping(_eb) && _eb.CanHit(_target))
                {
                    _eb.UseAbility(_target);
                    ExtSleeper.Sleep(1000, _eb + "caster");
                    ExtSleeper.Sleep(500, _eb);
                }
                if (_eb != null)
                {
                    if (ExtSleeper.Sleeping(_eb + "caster") &&
                        !_target.HasModifier("modifier_item_ethereal_blade_ethereal"))
                        return;
                }
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
        private static CancellationTokenSource _tks;
        private static Task _testCombo;
        public static async void TestCombo(EventArgs args)
        {
            if (_testCombo != null && !_testCombo.IsCompleted)
            {
                if (!MenuManager.EulComboIsActive)
                {
                    _tks.Cancel();
                }
                return;
            }
            if (!MenuManager.EulComboIsActive)
                return;
            _tks = new CancellationTokenSource();
            _testCombo = ComboWombo(_tks.Token);
            try
            {
                await _testCombo;
                _testCombo = null;
            }
            catch (OperationCanceledException)
            {
                _testCombo = null;
                _target = null;
            }
        }

        private static async Task ComboWombo(CancellationToken token)
        {
            await Task.Delay(500,token);
            if (!Ultimate.CanBeCasted())
                return;

            await FindTarget(token);

            if (_target.IsLinkensProtected() || _target.HasModifier("modifier_item_lotus_orb_active"))
                return;

            UpdateItems();
            var targetBlinkPosition = new Vector3();
            if (_eul != null && _eul.CanBeCasted() && !EulSleeper.Sleeping)
            {
                if (_eb != null && _eb.CanBeCasted() && !ExtSleeper.Sleeping(_eb) && _eb.CanHit(_target))
                {
                    _eb.UseAbility(_target);
                    Printer.Log("eb");
                    ExtSleeper.Sleep(500, _eb);
                    await CheckingForBlade(token);
                }
                if (_veil != null && _veil.CanBeCasted() && !ExtSleeper.Sleeping(_veil) && _veil.CanHit(_target))
                {
                    Printer.Log("veil");
                    _veil.UseAbility(_target.Position);
                    ExtSleeper.Sleep(500, _veil);
                }
                await MoveToTarget(token);
                
                Printer.Log("eul");
                EulSleeper.Sleep(250);
                _eul.UseAbility(_target);
                
                Printer.Log("old position: " + targetBlinkPosition.PrintVector());
                targetBlinkPosition = _target.Position;
                Printer.Log("new position: " + targetBlinkPosition.PrintVector());
                await Task.Delay(100, token);
            }
            var modifier = _target.FindModifier("modifier_eul_cyclone");
            if (modifier == null)
            {
                Printer.Log("cant find modifier!");
                return;
            }
            var ping = Game.Ping / 1000;
            if (Ultimate.IsInAbilityPhase)
            {
                return;
            }
            
            if (_bkb != null && _bkb.CanBeCasted() && !ExtSleeper.Sleeping(_bkb) && MenuManager.BkbIsEulCombo)
            {
                Printer.Log("bkb");
                _bkb.UseAbility();
                await Task.Delay(25, token);
                ExtSleeper.Sleep(500, _bkb);
            }
            if (_blink != null && _blink.CanBeCasted() && !BlinkSleeper.Sleeping)
            {
                Printer.Log("blink to "+ targetBlinkPosition.PrintVector());
                if (targetBlinkPosition.IsZero)
                {
                    Printer.Log("blink position is ZERO");
                    targetBlinkPosition = _target.Position;
                    Printer.Log("new blink position: "+targetBlinkPosition.PrintVector());
                }
                BlinkSleeper.Sleep(250);
                _blink.UseAbility(targetBlinkPosition);
                await Task.Delay(_blink.GetAbilityDelay(targetBlinkPosition) +50, token);
            }
            await MoveToTarget(token, 60);
            var remTime = modifier.RemainingTime-ping-1.67;
            remTime *= 1000;
            remTime -= MenuManager.EulExtraTime;
            Printer.Log($"need to wait: [{modifier.RemainingTime}] [{modifier.RemainingTime - ping}] [{remTime}]");
            await Task.Delay((int) remTime, token);
            Printer.Log("ultimate!");
            UltimateSleeper.Sleep(250);
            Ultimate.UseAbility();
        }

        private static async Task MoveToTarget(CancellationToken token,float dist=1200)
        {
            Me.Move(_target.Position);
            await Task.Delay(100, token);
            /*while (Me.Distance2D(_target) >= dist)
            {
                Printer.Log($"{Me.Distance2D(_target)}>={dist} ==> move to target position");
                Me.Move(_target.Position);
                await Task.Delay(100,token);
            }*/
        }

        private static async Task CheckingForBlade(CancellationToken token)
        {
            Printer.Log("CheckingForBlade");
            var i = 0;
            while (i++ <= 100 && !_target.HasModifier("modifier_item_ethereal_blade_ethereal"))
            {
                if (!ExtSleeper.Sleeping("move_while_w"))
                {
                    Me.Move(_target.Position);
                    ExtSleeper.Sleep(150, "move_while_w");
                    Printer.Log("moving while waiting");
                }
                await Task.Delay(10, token);
            }
        }

        private static void UpdateItems()
        {
            if (_eul == null || !_eul.IsValid)
                _eul = Me.GetItemById(ItemId.item_cyclone);
            if (_veil == null || !_veil.IsValid)
                _veil = Me.GetItemById(ItemId.item_veil_of_discord);
            if (_bkb == null || !_bkb.IsValid)
                _bkb = Me.GetItemById(ItemId.item_black_king_bar);
            if (_blink == null || !_blink.IsValid)
                _blink = Me.GetItemById(ItemId.item_blink);
            if (_eb == null || !_eb.IsValid)
                _eb = Me.GetItemById(ItemId.item_ethereal_blade);
        }


        private static async Task<Hero> FindTarget(CancellationToken token)
        {
            while (_target == null || !_target.IsValid || !_target.IsAlive)
            {
                _target = TargetSelector.ClosestToMouse(Me);
                if (_target != null && _target.IsValid && _target.IsAlive)
                    return _target;
                await Task.Delay(250, token);
            }
            return null;
        }
    }
}