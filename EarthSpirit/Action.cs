using System;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;

namespace EarthAn
{
    internal class Action
    {
        private static bool IsEnable => Members.Menu.Item("Enable").GetValue<bool>();
        private static bool IsEnableKillSteal => Members.Menu.Item("KillSteal.Enable").GetValue<bool>();
        private static bool AutoAttack => Members.Menu.Item("AutoAttack.Enable").GetValue<bool>();
        private static bool UseOrbWalkker => Members.Menu.Item("OrbWalker.Enable").GetValue<bool>();
        private static bool UsePrediction => Members.Menu.Item("Prediction.Enable").GetValue<bool>();
        private static bool IsComboHero => Members.Menu.Item("Combo.Enable").GetValue<KeyBind>().Active;
        private static int CloseRange => Members.Menu.Item("Dagger.CloseRange").GetValue<Slider>().Value;
        private static int MinDistance => Members.Menu.Item("Dagger.MinDistance").GetValue<Slider>().Value;
        private static int ExtraDistance => Members.Menu.Item("Dagger.ExtraDistance").GetValue<Slider>().Value;
        public static Hero GlobalTarget;
        private static Sleeper _attacker;
        private static Sleeper _ethereal;
        private static MultiSleeper _spellSleeper;
        public static void Game_OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;
            /*var rem = Helper.FindRemnant(Game.MousePosition);
            if (rem != null && rem.IsAlive)
            {
                Printer.Print("-----------");
                foreach (var modifier in rem.Modifiers)
                {
                    Printer.Print(modifier.Name);
                }
            }*/
            if (_attacker == null)
                _attacker = new Sleeper();
            if (_spellSleeper == null)
                _spellSleeper = new MultiSleeper();
            if (_ethereal == null)
                _ethereal = new Sleeper();
            if (!IsComboHero)
            {
                GlobalTarget = null;
                return;
            }
            if (GlobalTarget == null || !GlobalTarget.IsValid || !GlobalTarget.IsAlive)
            {
                GlobalTarget = Helper.ClosestToMouse(Members.MyHero);
                return;
            }
            if (AutoAttack)
                if (UseOrbWalkker)
                {
                    Orbwalking.Orbwalk(GlobalTarget, followTarget: true);
                }
                else if (!Members.MyHero.IsAttacking() && !_attacker.Sleeping && !GlobalTarget.IsAttackImmune())
                {
                    Members.MyHero.Attack(GlobalTarget);
                    _attacker.Sleep(250);
                }
            CastCombo();

        }

        private static void CastCombo()
        {
            var distance = Members.MyHero.Distance2D(GlobalTarget);
            if (CastItems(distance) && !_spellSleeper.Sleeping("daggerTime"))
                CastAbilities(distance);
        }

        private static void CastAbilities(float distance)
        {
            if (!IsEnable)
                return;
            if (!IsComboHero)
                return;
            if (GlobalTarget==null || !GlobalTarget.IsValid || !GlobalTarget.IsVisible || GlobalTarget.IsMagicImmune() || !GlobalTarget.IsAlive)
                return;
            var smash = Abilities.FindAbility("earth_spirit_boulder_smash");
            var geomagneticGrip = Abilities.FindAbility("earth_spirit_geomagnetic_grip");
            var magnetize = Abilities.FindAbility("earth_spirit_magnetize");
            var rollingBoulder = Abilities.FindAbility("earth_spirit_rolling_boulder");
            var stoneCaller = Abilities.FindAbility("earth_spirit_stone_caller");
            if (distance >= 1000)
                return;
            var pos = GlobalTarget.NetworkPosition;
            
            if (Helper.IsAbilityEnable(smash.StoredName()) && smash.CanBeCasted())
            {
                if (smash.CanBeCasted() && !_spellSleeper.Sleeping("combo"+ smash))
                {
                    var remnant = Helper.FindRemnant();
                    if (stoneCaller.CanBeCasted() && remnant == null)
                        stoneCaller.UseAbility(Members.MyHero.InFront(75));
                    if (UsePrediction)
                    {
                        if (smash.CastSkillShot(GlobalTarget))
                        {
                            _spellSleeper.Sleep(500, "combo" + smash);
                            _spellSleeper.Sleep(distance/900*1000 + 1000, "w8" + smash);
                            Printer.Print($"Time: {distance/900*1000 + 500}ms");
                        }
                    }
                    else
                    {
                        smash.UseAbility(GlobalTarget.NetworkPosition);
                        _spellSleeper.Sleep(500, "combo" + smash);
                        _spellSleeper.Sleep(distance/900*1000+500,"w8"+ smash);
                        Printer.Print($"Time: {distance / 900 * 1000 + 1000}ms");
                    }
                }
            }
            if (Helper.IsAbilityEnable(geomagneticGrip.StoredName()) && geomagneticGrip.CanBeCasted() && !_spellSleeper.Sleeping("combo" + geomagneticGrip))
            {
                if (GlobalTarget.IsStunned())
                    Printer.Print("stunned!");
                if (_spellSleeper.Sleeping("w8" + smash) &&
                    !GlobalTarget.IsStunned())
                {
                }
                else
                {
                    var remnant = Helper.FindRemnant(pos,250);
                    if (stoneCaller.CanBeCasted() && remnant == null)
                    {
                        stoneCaller.UseAbility(pos);
                        geomagneticGrip.UseAbility(pos,true);
                        Printer.Print("new");
                    }
                    else if (remnant!=null)
                    {
                        Printer.Print("finded");
                        geomagneticGrip.UseAbility(remnant.NetworkPosition);
                    }
                    _spellSleeper.Sleep(250,"combo" + geomagneticGrip);
                }
            }
            else if (Helper.IsAbilityEnable(rollingBoulder.StoredName()) && rollingBoulder.CanBeCasted() && !_spellSleeper.Sleeping("combo" + rollingBoulder))
            {
                if (UsePrediction)
                {
                    if (rollingBoulder.CastSkillShot(GlobalTarget))
                    {
                        
                    }
                }
                else /*if (remnant!=null)*/
                {
                    rollingBoulder.UseAbility(GlobalTarget.NetworkPosition);
                }
                _spellSleeper.Sleep(250,"combo" + rollingBoulder);
            }
            else if (rollingBoulder.Cooldown>=3.5)
            {
                var myPos = Members.MyHero.Position;

                var angle = Members.MyHero.FindAngleBetween(pos, true);
                var point = new Vector3(
                    (float)
                        (myPos.X +
                         100 *
                         Math.Cos(angle)),
                    (float)
                        (myPos.Y +
                         100 *
                         Math.Sin(angle)),
                    0);
                var remnant = Helper.FindRemnant(point, 100) ??
                              Helper.FindRemnantWithModifier(pos, "modifier_earth_spirit_geomagnetic_grip");
                if (stoneCaller.CanBeCasted() && remnant == null &&
                    Members.MyHero.HasModifier("modifier_earth_spirit_rolling_boulder_caster"))
                {
                    if (!_spellSleeper.Sleeping("combo" + rollingBoulder + "Caller"))
                    {
                        Printer.Print($"cd: ({rollingBoulder.Cooldown}) | cant find remnant!");
                        //stoneCaller.UseAbility(point);
                        stoneCaller.UseAbility(Members.MyHero.Position);
                        _spellSleeper.Sleep(2000, "combo" + rollingBoulder + "Caller");
                    }
                }
            }
            if (Helper.IsAbilityEnable(magnetize.StoredName()) && magnetize.CanBeCasted() && !_spellSleeper.Sleeping("combo" + magnetize))
            {
                if (distance <= 300)
                {
                    magnetize.UseAbility();
                    _spellSleeper.Sleep(250, "combo" + magnetize);
                }
            }
            var mod = GlobalTarget.FindModifier("modifier_earth_spirit_magnetize");
            if (Helper.IsAbilityEnable(stoneCaller.StoredName()) && mod != null && mod.RemainingTime <= 0.2 &&
                !_spellSleeper.Sleeping("combo" + stoneCaller) && stoneCaller.CanBeCasted())
            {
                Printer.Print($"remTime: {mod.RemainingTime}");
                stoneCaller.UseAbility(pos);
                _spellSleeper.Sleep(1000, "combo" + stoneCaller);
            }
        }

        private static bool CastItems(float distance)
        {
            var myInventory = Members.MyHero.Inventory.Items.ToList();
            var allItems = myInventory.Where(x =>
                Helper.IsItemEnable(x.StoredName()) && x.CanBeCasted()).ToList();
            
            var extraRange = myInventory.Any(x => x.StoredName() == "item_aether_lens") ? 200 : 0;
            var inventory =
                allItems.Where(
                    x =>
                        ((x.StoredName() == "item_blink" && distance <= 1800) || (x.GetCastRange() > 0 && distance <= x.GetCastRange() + 150) ||
                         (x.GetCastRange() <= 0)) && Utils.SleepCheck($"{x.Handle}+item_usages"))
                    .ToList();
            var copy = inventory.ToList();
            foreach (var item in inventory.OrderByDescending(Helper.PriorityHelper))
            {
                var name = item.StoredName();
                if (name == "item_ethereal_blade")
                    _ethereal.Sleep(500);
                if (name == "item_dagon" || name == "item_dagon_2" || name == "item_dagon_3" || name == "item_dagon_4" || name == "item_dagon_5")
                    if (_ethereal.Sleeping && !GlobalTarget.HasModifier("modifier_item_ethereal_blade_ethereal"))
                    {
                        copy.Remove(item);
                        continue;
                    }
                if (item.IsAbilityBehavior(AbilityBehavior.NoTarget))
                    item.UseAbility();
                else if (item.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                    if (item.TargetTeamType == TargetTeamType.Enemy || item.TargetTeamType == TargetTeamType.All ||
                        item.TargetTeamType == TargetTeamType.Custom)
                        item.UseAbility(GlobalTarget);
                    else
                        item.UseAbility(Members.MyHero);
                else
                {
                    if (name == "item_blink")
                    {
                        if (distance > 1150 + extraRange)
                        {
                            var angle = Members.MyHero.FindAngleBetween(GlobalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (GlobalTarget.Position.X -
                                     CloseRange*
                                     Math.Cos(angle)),
                                (float)
                                    (GlobalTarget.Position.Y -
                                     CloseRange*
                                     Math.Sin(angle)),
                                GlobalTarget.Position.Z);
                            var dist = Members.MyHero.Distance2D(point);
                            if (dist >= MinDistance && dist <= 1150)
                            {

                                _spellSleeper.Sleep(250,"daggerTime");
                                item.UseAbility(point);
                            }
                            else
                                copy.Remove(item);
                        }
                        else if (distance > MinDistance)
                        {
                            var angle = Members.MyHero.FindAngleBetween(GlobalTarget.Position, true);
                            var point = new Vector3(
                                (float)
                                    (GlobalTarget.Position.X -
                                     ExtraDistance *
                                     Math.Cos(angle)),
                                (float)
                                    (GlobalTarget.Position.Y -
                                     ExtraDistance *
                                     Math.Sin(angle)),
                                GlobalTarget.Position.Z);
                            item.UseAbility(point);
                            _spellSleeper.Sleep(250, "daggerTime");
                        }
                    }
                    else
                        item.UseAbility(GlobalTarget.NetworkPosition);
                }

                Utils.Sleep(250, $"{item.Handle}+item_usages");
            }
            return !copy.Any();
        }
    }
}