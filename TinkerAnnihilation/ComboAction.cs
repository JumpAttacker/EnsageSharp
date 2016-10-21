using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace TinkerAnnihilation
{
    internal class ComboAction
    {
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool IsEnableKillSteal => Members.Menu.Item("KillSteal.Enable").GetValue<bool>();
        private static bool AutoAttack => Members.Menu.Item("AutoAttack.Enable").GetValue<bool>();
        private static bool UseOrbWalkker => Members.Menu.Item("OrbWalker.Enable").GetValue<bool>();
        private static bool IsComboHero => Members.Menu.Item("Combo.Enable").GetValue<KeyBind>().Active;
        private static int CloseRange => Members.Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value;
        private static int MinDistance => Members.Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value;
        private static int ExtraDistance => Members.Menu.Item("Dagger.ExtraDistance").GetValue<Slider>().Value;
        private static int DamageInderx => Members.Menu.Item("Drawing.EnableDamage").GetValue<StringList>().SelectedIndex;

        private enum DamageDrawing
        {
            OnlyForGlobalTarget,
            ForAll
        }

        private static Hero _globalTarget;
        private static Sleeper _attacker;
        private static Sleeper _ethereal;
        private static Sleeper _dmageSleeper;
        private static MultiSleeper _spellSleeper;
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!IsEnable)
                return;
            var closeToMouse = Helper.ClosestToMouse(Members.MyHero);
            if (DamageInderx == (int)DamageDrawing.OnlyForGlobalTarget && (closeToMouse == null || !closeToMouse.IsValid))
            {
                return;
            }
            var list = DamageInderx == (int) DamageDrawing.ForAll
                ? Heroes.GetByTeam(Members.MyHero.GetEnemyTeam())
                    .Where(x => x.IsAlive && x.IsVisible && !x.IsIllusion() && !x.IsMagicImmune()).ToList()
                : new List<Hero> {closeToMouse};
            foreach (var hero in list)
            {
                var pos = HUDInfo.GetHPbarPosition(hero);
                if (pos.IsZero)
                    continue;
                var size = HUDInfo.GetHpBarSizeY();
                var text = $"{(int) Helper.CalculateMyCurrentDamage(hero)} ({(int) Helper.CalculateMyDamage(hero)})";
                var textSize = Drawing.MeasureText(text, "Arial",
                    new Vector2((float)(size * 1.5), 500), FontFlags.AntiAlias);
                var textPos = pos + new Vector2(HUDInfo.GetHPBarSizeX() + 4, 0);
                Drawing.DrawText(
                    text,
                    textPos,
                    new Vector2(textSize.Y, 0),
                    new Color(255, 255, 255, 255),
                    FontFlags.AntiAlias | FontFlags.StrikeOut);
            }
            
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (!Members.MyHero.IsAlive)
                return;
            //CalculateDamage();
            if (_attacker == null)
                _attacker = new Sleeper();
            if (_spellSleeper == null)
                _spellSleeper = new MultiSleeper();
            if (_ethereal == null)
                _ethereal = new Sleeper();
            var laser = Abilities.FindAbility("tinker_laser");
            var rockets = Abilities.FindAbility("tinker_heat_seeking_missile");
            if (IsEnableKillSteal && !IsComboHero)
            {
                foreach (var x in Heroes.GetByTeam(Members.MyHero.GetEnemyTeam())
                    .Where(x => x.IsAlive && x.IsVisible && !x.IsIllusion() && !x.IsMagicImmune())
                    .Where(x => Helper.CalculateMyCurrentDamage(x, true) < 0))
                {
                    CastCombo(x, laser, rockets, true);
                }
            }
            
            if (!IsComboHero)
            {
                if (_globalTarget!=null)
                    Helper.UnHandleEffect(_globalTarget);
                _globalTarget = null;
                return;
            }
            if (_globalTarget == null || !_globalTarget.IsValid)
            {
                _globalTarget = Helper.ClosestToMouse(Members.MyHero);
                return;
            }
            Helper.HandleEffect(_globalTarget);
            if (Members.MyHero.IsChanneling() ||
                _spellSleeper.Sleeping(Abilities.FindAbility("tinker_rearm")) ||
                _globalTarget.Distance2D(Members.MyHero) > 2500)
            {
                return;
            }
            if (AutoAttack)
                if (UseOrbWalkker)
                {
                    Orbwalking.Orbwalk(_globalTarget, followTarget: true);
                }
                else if (!Members.MyHero.IsAttacking() && !_attacker.Sleeping && !_globalTarget.IsAttackImmune())
                {
                    Members.MyHero.Attack(_globalTarget);
                    _attacker.Sleep(250);
                }
            CastCombo(_globalTarget,laser, rockets);
            
        }

        private static void CastCombo(Hero globalTarget, Ability laser, Ability rockets, bool singleCombo = false)
        {
            var myInventory = Members.MyHero.Inventory.Items.ToList();
            var allItems = myInventory.Where(x =>
                Helper.IsItemEnable(x.StoredName()) && x.CanBeCasted()).ToList();
            var distance = Members.MyHero.Distance2D(globalTarget);
            var daggerCastRange = 1150 + (myInventory.Any(x => x.StoredName() == "item_aether_lens") ? 200 : 0);
            var inventory =
                allItems.Where(
                    x =>
                         ((x.StoredName() == "item_blink" && distance <= daggerCastRange + CloseRange) ||
                         (x.GetCastRange() > 0 && x.CanHit(globalTarget) /* && distance <= x.GetCastRange() + 150*/) ||
                         (x.GetCastRange() <= 0)) && Utils.SleepCheck($"{x.Handle}+item_usages"))
                    .ToList();

            if (laser != null && Helper.IsAbilityEnable(laser.StoredName()) && laser.CanBeCasted() &&
                laser.CanHit(globalTarget) && !_spellSleeper.Sleeping(laser))
            {
                laser.UseAbility(globalTarget);
                _spellSleeper.Sleep(500, laser);
            }
            var slarkMod = globalTarget.HasModifiers(new[] { "modifier_slark_dark_pact", "modifier_slark_dark_pact_pulses" }, false);
            foreach (var item in inventory.OrderByDescending(Helper.PriorityHelper))
            {
                var name = item.StoredName();
                if (name == "item_ethereal_blade")
                    _ethereal.Sleep(1000);
                if (name.Contains("dagon"))
                    if (_ethereal.Sleeping && !globalTarget.HasModifier("modifier_item_ethereal_blade_ethereal"))
                        continue;
                if (item.IsAbilityBehavior(AbilityBehavior.NoTarget))
                {
                    item.UseAbility();
                }
                else if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                    if (item.TargetTeamType == TargetTeamType.Enemy || item.TargetTeamType == TargetTeamType.All ||
                        item.TargetTeamType == TargetTeamType.Custom)
                    {
                        if (item.IsDisable())
                        {
                            if (!slarkMod && !globalTarget.IsLinkensProtected())
                                item.UseAbility(globalTarget);
                                /*if (item.CastStun(globalTarget))
                                {
                                    Utils.Sleep(250, $"{item.Handle}+item_usages");
                                    continue;
                                }*/
                        }
                        else if (item.IsSilence())
                        {
                            if (!slarkMod)
                                if (!globalTarget.IsSilenced())
                                {
                                    item.UseAbility(globalTarget);
                                }
                        }
                        else if ((item.StoredName().Contains("dagon") || item.StoredName() == "item_ethereal_blade") &&
                                 globalTarget.HasModifiers(
                                     new[]
                                     {
                                         /*"modifier_templar_assassin_refraction_absorb",
                                         "modifier_templar_assassin_refraction_absorb_stacks",*/
                                         "modifier_oracle_fates_edict",
                                         "modifier_abaddon_borrowed_time"
                                     }, false))
                        {
                            continue;
                        }
                        else
                        {
                            item.UseAbility(globalTarget);
                        }

                        /*item.UseAbility(target);
                        Print($"[Using]: {item.Name} (3)", print: false);*/
                    }
                    else
                    {
                        item.UseAbility(Members.MyHero);
                    }
                else
                {
                    if (name == "item_blink")
                    {
                        if (singleCombo)
                            continue;
                        if (distance > daggerCastRange+CloseRange)
                        {
                            var angle = Members.MyHero.FindAngleBetween(globalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (globalTarget.Position.X -
                                     daggerCastRange *
                                     Math.Cos(angle)),
                                (float)
                                    (globalTarget.Position.Y -
                                     daggerCastRange *
                                     Math.Sin(angle)),
                                globalTarget.Position.Z);
                            var dist = Members.MyHero.Distance2D(point);
                            if (dist >= MinDistance && dist <= daggerCastRange)
                            {
                                item.UseAbility(point);
                            }
                        }
                        else if (distance > MinDistance)
                        {
                            var angle = Members.MyHero.FindAngleBetween(globalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (globalTarget.Position.X -
                                     ExtraDistance *
                                     Math.Cos(angle)),
                                (float)
                                    (globalTarget.Position.Y -
                                     ExtraDistance *
                                     Math.Sin(angle)),
                                globalTarget.Position.Z);
                            item.UseAbility(point);
                        }
                    }
                    else
                    {
                        item.UseAbility(globalTarget.NetworkPosition);
                    }
                }
                Utils.Sleep(100, $"{item.Handle}+item_usages");

                #region 

                /*var name = item.StoredName();
                if (name == "item_ethereal_blade")
                    _ethereal.Sleep(500);
                if (name.Contains("dagon"))
                    if (_ethereal.Sleeping && !globalTarget.HasModifier("modifier_item_ethereal_blade_ethereal"))
                        continue;
                if (item.IsAbilityBehavior(AbilityBehavior.NoTarget))
                    item.UseAbility();
                else if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                    if (item.TargetTeamType == TargetTeamType.Enemy || item.TargetTeamType == TargetTeamType.All ||
                        item.TargetTeamType == TargetTeamType.Custom)
                        item.UseAbility(globalTarget);
                    else
                        item.UseAbility(Members.MyHero);
                else
                {
                    if (name == "item_blink")
                    {
                        if (singleCombo)
                            continue;
                        if (distance > 1150 + extraRange)
                        {
                            var angle = Members.MyHero.FindAngleBetween(globalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (globalTarget.Position.X -
                                     CloseRange*
                                     Math.Cos(angle)),
                                (float)
                                    (globalTarget.Position.Y -
                                     CloseRange*
                                     Math.Sin(angle)),
                                globalTarget.Position.Z);
                            var dist = Members.MyHero.Distance2D(point);
                            if (dist >= MinDistance && dist <= 1150)
                                item.UseAbility(point);
                        }
                        else if (distance > MinDistance)
                        {
                            var angle = Members.MyHero.FindAngleBetween(globalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (globalTarget.Position.X -
                                     ExtraDistance *
                                     Math.Cos(angle)),
                                (float)
                                    (globalTarget.Position.Y -
                                     ExtraDistance *
                                     Math.Sin(angle)),
                                globalTarget.Position.Z);
                            item.UseAbility(point);
                        }
                    }
                    else
                        item.UseAbility(globalTarget.NetworkPosition);
                }
                    
                Utils.Sleep(250, $"{item.Handle}+item_usages");*/

                #endregion

            }
            if (rockets != null && Helper.IsAbilityEnable(rockets.StoredName()) && rockets.CanBeCasted() &&
                distance <= 2500 && !_spellSleeper.Sleeping(rockets) && Helper.CanRockedHit(globalTarget))
            {
                rockets.UseAbility();
                _spellSleeper.Sleep(500, rockets);
                /*if (_ethereal.Sleeping && !globalTarget.HasModifier("modifier_item_ethereal_blade_ethereal"))
                {

                }
                else
                {
                    rockets.UseAbility();
                    _spellSleeper.Sleep(500, rockets);
                }*/
            }
            if (singleCombo)
                return;
            var rearm = Abilities.FindAbility("tinker_rearm");



            if (inventory.Count != 0 || (rockets.CanBeCasted() && Helper.CanRockedHit(globalTarget)) || (laser.CanBeCasted() && laser.CanHit(globalTarget)))
            {
                Printer.Print($"{inventory.Count}");
                return;
            }
            //Printer.Print($"{rearm != null}/{Helper.IsAbilityEnable(rearm.StoredName())}/{rearm.CanBeCasted()}/{!_spellSleeper.Sleeping(rearm)}/{!Members.MyHero.IsChanneling()}");
            if (rearm != null && Helper.IsAbilityEnable(rearm.StoredName()) && rearm.CanBeCasted() && !_spellSleeper.Sleeping(rearm) && !Members.MyHero.IsChanneling())
            {
                rearm.UseAbility();
                //_spellSleeper.Sleep(4000 - rearm.Level * 500, rearm);
                _spellSleeper.Sleep(750, rearm);
                //Printer.Print("use ream");
            }
        }


        /*private static void CalculateDamage()
        {
            if (_dmageSleeper == null)
                _dmageSleeper = new Sleeper();
            if (_dmageSleeper.Sleeping || _globalTarget == null || !_globalTarget.IsValid)
                return;
            _dmageSleeper.Sleep(100);
            _myCurrentDamage = Helper.CalculateMyCurrentDamage(_globalTarget);
            _myMaxDamage = Helper.CalculateMyDamage(_globalTarget);
        }*/
    }
}