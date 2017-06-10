using System;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using log4net;
using PlaySharp.Toolkit.Logging;
using AbilityId = Ensage.AbilityId;

namespace StormAnnihilation
{
    public static class Core
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static Hero Me, Target;
        private static int _totalDamage;
        private static float _remainingMana;
        public static void Game_OnUpdate(EventArgs args)
        {
            /*Console.WriteLine(new string('-', Console.BufferWidth));
            foreach (var data in Me.Spellbook.Spell4.AbilitySpecialData)
            {
                Log.Debug($"{data.Name} -> {data.Value} -> {data.Count}");
            }*/
            if (!MenuManager.IsEnable)
            {
                Target = null;
                return;
            }

            if (Target == null || !Target.IsValid)
            {
                Target = TargetSelector.ClosestToMouse(Me, 150);
            }
            if (Target == null || !Target.IsValid || !Target.IsAlive || Me.IsStunned() || !Me.IsAlive) return;

            var zip = Me.GetAbilityById(AbilityId.storm_spirit_ball_lightning);
            
            if (zip == null || zip.Level == 0) return;
            var inUltimate = Me.HasModifier("modifier_storm_spirit_ball_lightning") || zip.IsInAbilityPhase;
            var inPassve = Me.HasModifier("modifier_storm_spirit_overload");
            var zipLevel = zip.Level;
            var distance = Me.Distance2D(Target);

            var travelSpeed = zip.GetAbilityData("ball_lightning_move_speed", zipLevel - 1);
            var damage = zip.GetAbilityData("#AbilityDamage", zipLevel - 1)/100f;
            var damageRadius = zip.GetAbilityData("ball_lightning_aoe", zipLevel - 1);//50 + 75 * zipLevel;
            var startManaCost = zip.GetAbilityData("ball_lightning_initial_mana_base") +
                                Me.MaximumMana/100*zip.GetAbilityData("ball_lightning_initial_mana_percentage");
            var costPerUnit = (12 + Me.MaximumMana * 0.007) / 100.0;

            var totalCost = (int)(startManaCost + costPerUnit * (int)Math.Floor((decimal)distance / 100) * 100);

            var travelTime = distance / travelSpeed;

            var enemyHealth = Target.Health;
            _remainingMana = Me.Mana - totalCost + (Me.ManaRegeneration * (travelTime + 1));

            var hpPerc = Me.Health / (float)Me.MaximumHealth;
            var mpPerc = Me.Mana / Me.MaximumMana;
            if (Utils.SleepCheck("mana_items"))
            {
                var soulring = Me.GetItemById(ItemId.item_soul_ring);
                if (soulring != null && soulring.CanBeCasted() && hpPerc >= .1 && mpPerc <= .7)
                {
                    soulring.UseAbility();
                    Utils.Sleep(200, "mana_items");
                }
                var stick = Me.GetItemById(ItemId.item_magic_stick) ?? Me.GetItemById(ItemId.item_magic_wand);
                if (stick != null && stick.CanBeCasted() && stick.CurrentCharges != 0 && (mpPerc <= .5 || hpPerc <= .5))
                {
                    stick.UseAbility();
                    Utils.Sleep(200, "mana_items");
                }
            }

            #region Ultimate and Attack

            if (!inUltimate && Utils.SleepCheck("castUlt"))
            {
                if (inPassve && distance < Me.AttackRange)
                {

                    /*if (MenuManager.Orbwalker)
                    {
                        Helper.Orbwalk(Me, Target);
                    }
                    else */if (Utils.SleepCheck("Attacking") && !Me.IsAttacking())
                    {
                        Me.Attack(Target);
                        Utils.Sleep(Me.AttackSpeedValue, "Attacking");
                        Helper.Print("flush passive");
                    }
                }
                else if (_remainingMana > 0 && Utils.SleepCheck("Attacking"))
                {
                    if (distance >= MenuManager.MinRange)
                    {
                        if (distance >= damageRadius)
                        {
                            zip.UseAbility(Prediction.SkillShotXYZ(Me, Target, (float)zip.FindCastPoint(),
                                travelSpeed,
                                damageRadius)); //TODO mb more accurate
                            /*if (MenuManager.Orbwalker)
                            {
                                Helper.Orbwalk(Me, Target);
                            }
                            else*/
                            Me.Attack(Target, true);
                            Helper.Print("[attack][ult] out of range");
                        }
                        else
                        {
                            var remnant = Me.GetAbilityById(AbilityId.ember_spirit_fire_remnant);
                            if (!remnant.CanBeCasted())
                            {
                                zip.UseAbility(Me.Position);
                            }
                            /*if (MenuManager.Orbwalker)
                            {
                                Helper.Orbwalk(Me, Target);
                            }
                            else*/
                            Me.Attack(Target, true);
                            Helper.Print("[attack][ult] in range range");
                        }
                        Utils.Sleep(MenuManager.UltRate, "castUlt");
                    }
                    else /*if (Utils.SleepCheck("Attacking"))*/
                    {
                        /*if (MenuManager.Orbwalker)
                        {
                            Helper.Orbwalk(Me, Target);
                        }
                        else*/
                        {
                            Me.Attack(Target);
                            Utils.Sleep(750, "Attacking");
                            Helper.Print("[attack] just ->"+ Utils.SleepCheck("Attacking"));
                        }
                    }
                }
                else
                {
                    Helper.Orbwalk(Me, Target);
                    Helper.Print("[orbwalking] without mana");
                }
            }

            #endregion

            _totalDamage = (int)(damage * distance);
            _totalDamage = (int)Target.DamageTaken(_totalDamage, DamageType.Magical, Me);

            if (enemyHealth < _totalDamage) return; //Target ll die only from ultimate

            #region items

            if (Utils.SleepCheck("items"))
            {
                var eul = Me.GetItemById(ItemId.item_cyclone);
                if (eul != null && eul.CanBeCasted() && Target.IsLinkensProtected())
                {
                    eul.UseAbility(Target);
                    Utils.Sleep(250, "items");
                }
                var hex = Me.GetItemById(ItemId.item_sheepstick);
                var orchid = Me.GetItemById(ItemId.item_orchid)?? Me.GetItemById(ItemId.item_bloodthorn);
                if (hex != null && hex.CanBeCasted(Target) && !Target.IsHexed() && !Target.IsStunned() && !Target.IsLinkensProtected())
                {
                    hex.UseAbility(Target);
                    Utils.Sleep(250, "items");
                }
                if (orchid != null && orchid.CanBeCasted(Target) && !Target.IsHexed() && !Target.IsSilenced() &&
                    !Target.IsStunned() && !Target.IsLinkensProtected())
                {
                    orchid.UseAbility(Target);
                    Utils.Sleep(250, "items");
                }
                var shiva = Me.GetItemById(ItemId.item_shivas_guard);
                if (shiva != null && shiva.CanBeCasted() && distance <= 900)
                {
                    shiva.UseAbility();
                    Utils.Sleep(250, "items");
                }
                var lotus = Me.GetItemById(ItemId.item_lotus_orb);
                if (lotus != null && lotus.CanBeCasted() && distance <= 500 && !inUltimate)
                {
                    lotus.UseAbility(Me);
                    Utils.Sleep(250, "items");
                }
                var dagon = Me.GetDagon();
                if (dagon != null && dagon.CanBeCasted(Target) && distance <= dagon.CastRange)
                {
                    dagon.UseAbility(Target);
                    Utils.Sleep(250, "items");
                }
            }

            #endregion

            #region Spells

            if (Utils.SleepCheck("spells"))
            {
                var remnant = Me.GetAbilityById(AbilityId.storm_spirit_static_remnant);
                var vortex = Me.GetAbilityById(AbilityId.storm_spirit_electric_vortex);
                if (remnant != null && remnant.CanBeCasted() && !inPassve && !inUltimate)
                {
                    remnant.UseAbility();
                    Utils.Sleep(250, "spells");
                }
                else if (vortex != null && vortex.CanBeCasted(Target) && vortex.CanHit(Target) && !Target.IsHexed() &&
                         !Target.IsStunned())
                {
                    vortex.UseAbility(Target);
                    Utils.Sleep(250, "spells");
                }
            }

            #endregion
        }
    }
}