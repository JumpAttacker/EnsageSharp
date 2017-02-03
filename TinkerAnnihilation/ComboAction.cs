using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ensage;
using Ensage.Common;
using Ensage.Common.Enums;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;
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
        private static int DamageIndex => Members.Menu.Item("Drawing.EnableDamage").GetValue<StringList>().SelectedIndex;
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private enum DamageDrawing
        {
            OnlyForGlobalTarget,
            ForAll
        }

        private static Hero _globalTarget;
        private static Sleeper _attacker;
        private static Sleeper _ethereal;
        private static MultiSleeper _spellSleeper;
        private static Ability _laser;
        private static Ability _rockets;
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (_globalTarget!=null && _globalTarget.IsValid)
                Helper.HandleEffect(_globalTarget);
            var closeToMouse = Helper.ClosestToMouse(Members.MyHero);
            if (DamageIndex == (int)DamageDrawing.OnlyForGlobalTarget && (closeToMouse == null || !closeToMouse.IsValid))
            {
                return;
            }
            var list = DamageIndex == (int) DamageDrawing.ForAll
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
        private static CancellationTokenSource _tks;
        private static Task _testCombo;
        private static readonly int[] RearmTime = { 3000, 1500, 750 };
        private static int GetRearmTime(Ability s) => RearmTime[s.Level - 1];
        public static async void OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;
            if (!Members.MyHero.IsAlive)
                return;
            if (_attacker == null)
                _attacker = new Sleeper();
            if (_spellSleeper == null)
                _spellSleeper = new MultiSleeper();
            if (_ethereal == null)
                _ethereal = new Sleeper();
            if (_laser == null)
                _laser = Abilities.FindAbility("tinker_laser");
            if (_rockets == null)
                _rockets = Abilities.FindAbility("tinker_heat_seeking_missile");
            if (_spellSleeper.Sleeping(Abilities.FindAbility("tinker_rearm")))
                return;
            if (_testCombo != null && !_testCombo.IsCompleted)
            {
                return;
            }
            
            _tks = new CancellationTokenSource();
            _testCombo = Action(_tks.Token);
            try
            {
                await _testCombo;
                _testCombo = null;
            }
            catch (OperationCanceledException)
            {
                _testCombo = null;
            }
            if (IsEnableKillSteal && !IsComboHero)
            {
                foreach (var x in Heroes.GetByTeam(Members.MyHero.GetEnemyTeam())
                    .Where(x => x.IsAlive && x.IsVisible && !x.IsIllusion() && !x.IsMagicImmune())
                    .Where(x => Helper.CalculateMyCurrentDamage(x, true) < 0))
                {
                    await DoTrash(x, CancellationToken.None, true);
                }
            }
        }

        private static async Task Action(CancellationToken cancellationToken)
        {
            if (IsComboHero)
            {
                if (!IsComboHero)
                {
                    if (_globalTarget != null)
                        Helper.UnHandleEffect(_globalTarget);
                    _globalTarget = null;
                    return;
                }
                if (_globalTarget == null || !_globalTarget.IsValid)
                {
                    _globalTarget = Helper.ClosestToMouse(Members.MyHero);
                    return;
                }
                
                if (Members.MyHero.IsChanneling())
                {
                    return;
                }
                
                await DoTrash(_globalTarget, cancellationToken);
                await Task.Delay(50, cancellationToken);
                Printer.Print("rdy for rearm!");
                var rearm = Abilities.FindAbility("tinker_rearm");
                if (rearm.CanBeCasted())
                {
                    
                    rearm?.UseAbility();
                    var time = (int)(GetRearmTime(rearm) + Game.Ping + rearm.FindCastPoint() * 1000);
                    Printer.Print("rearm!");
                    Log.Debug("Rearm: "+time);
                    await Task.Delay(time, cancellationToken);
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
            }
            else
            {
                if (_globalTarget != null)
                    Helper.UnHandleEffect(_globalTarget);
                _globalTarget = null;
            }

            
            await Task.Delay(100, cancellationToken);
        }

        private static async Task DoTrash(Hero killStealTarget, CancellationToken cancellationToken, bool isKillSteal = false)
        {
            var target = killStealTarget;
            var myInventory = new List<Ability>(Members.MyHero.Inventory.Items) {_laser, _rockets};
            var allItems = myInventory.Where(x =>
                Helper.IsItemEnableNew(x.StoredName())).ToList();
            var blinkPos = new Vector3();
            var inventory =
                allItems.Where(
                    x =>
                        ((x.StoredName() == "item_blink" && x.CheckDaggerForUsable(target.Position,out blinkPos)/*&& distance <= daggerCastRange + CloseRange*/) ||
                         (x.GetCastRange() > 0 && x.CanHit(target) /* && distance <= x.GetCastRange() + 150*/ && x.StoredName()!="item_blink") ||
                         (x.GetCastRange() <= 0)) && Utils.SleepCheck($"{x.Handle}+item_usages"))
                    .OrderBy(Helper.PriorityHelper)
                    .ToList();
            var slarkMod = target.HasModifiers(new[] { "modifier_slark_dark_pact", "modifier_slark_dark_pact_pulses" }, false);
            if (inventory.Any(x => x.StoredName() == "item_ethereal_blade" && x.CanBeCasted() && x.CanHit(target)))
            {
                _ethereal.Sleep(1000);
            }
            if (!inventory.Any(x=>x.IsInAbilityPhase))
            foreach (var v in inventory)
            {
                if (v.CanBeCasted() && (v.TargetTeamType==TargetTeamType.None || (v.CanHit(target) && !v.Equals(_rockets)) ||(v.Equals(_rockets) && Helper.CanRockedHit(target))))
                {
                    var name = v.StoredName();
                    
                    if (v.DamageType == DamageType.Magical || v.StoredName().Contains("dagon"))
                        if (_ethereal.Sleeping && !target.HasModifier("modifier_item_ethereal_blade_ethereal"))
                        {
                            continue;
                        }
                    if (v.IsAbilityBehavior(AbilityBehavior.NoTarget))
                    {
                        await UseAbility(v);
                    }
                    else if (v.IsAbilityBehavior(AbilityBehavior.UnitTarget))
                        if (v.TargetTeamType == TargetTeamType.Enemy || v.TargetTeamType == TargetTeamType.All ||
                            v.TargetTeamType == TargetTeamType.Custom)
                        {
                            if (v.IsDisable())
                            {
                                if (!slarkMod && !target.IsLinkensProtected())
                                    await UseAbility(v, target);
                            }
                            else if (v.IsSilence())
                            {
                                if (!slarkMod)
                                    if (!target.IsSilenced())
                                    {
                                        await UseAbility(v, target);
                                    }
                            }
                            else if ((v.DamageType == DamageType.Magical || v.StoredName() == "item_ethereal_blade") &&
                                     target.HasModifiers(
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
                                await UseAbility(v, target);
                            }
                        }
                        else
                        {
                            await UseAbility(v, Members.MyHero);
                        }
                    else
                    {
                        if (name == "item_blink")
                        {
                            if (isKillSteal)
                                continue;
                            await UseAbility(v, point: blinkPos);
                            continue;
                        }
                        
                        await UseAbility(v, point: target.NetworkPosition);
                    }
                }
            }
            if (!isKillSteal &&
                inventory.Any(
                    v =>
                        v.CanBeCasted() &&
                        ((v.CanHit(target) && !v.Equals(_rockets)) ||
                         (v.Equals(_rockets) && Helper.CanRockedHit(target)))))
            {
                var count = inventory.Count(
                    v =>
                        v.CanBeCasted() &&
                        ((v.CanHit(target) && !v.Equals(_rockets)) ||
                         (v.Equals(_rockets) && Helper.CanRockedHit(target))));
                await Task.Delay(20, cancellationToken);
                await DoTrash(killStealTarget, cancellationToken);
            }
        }

        private static async Task UseAbility(Ability v,Hero target=null, Vector3 point=new Vector3())
        {
            if (!v.CanBeCasted())
                return;
            var castTime = target != null ? Helper.GetAbilityDelay(target, v) : Helper.GetAbilityDelay(point, v);
            if (v.StoredName() == "item_blink")
            {
                castTime += 80;
            }
            if (v.GetAbilityId() == AbilityId.tinker_laser)
            {
                castTime += 50;
            }
            if (target != null)
            {
                v.UseAbility(target);
            }
            if (!point.IsZero)
            {
                v.UseAbility(point);
            }
            else
                v.UseAbility();
            Log.Debug($"Use: {v.Name} -> {castTime}ms");
            //Printer.Print($"Global waiter: {v.Name}/{castTime}");
            await Task.Delay(castTime, _tks.Token);
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
            if (_spellSleeper.Sleeping(Abilities.FindAbility("tinker_rearm")))
                return;
            /*var data = _laser.AbilitySpecialData;
            var dmg = _laser.GetAbilityData("laser_damage", 1);
            Console.WriteLine($"dmg: {dmg}");
            foreach (var abilitySpecialData in data.Where(x=>x.IsSpellDamageValue))
            {
                Console.WriteLine($"{abilitySpecialData.Name}: {abilitySpecialData.GetValue(_laser.Level-1)}");
            }
            Console.WriteLine($"Laser: {_laser.GetDamage(0)}/{_laser.GetDamage(1)}/{_laser.GetDamage(2)}/{_laser.GetDamage(3)}");*/
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
            if (Members.MyHero.IsChanneling() || _globalTarget.Distance2D(Members.MyHero) > 2500)
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
                _spellSleeper.Sleep(1000, laser);
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
                        if (singleCombo || _spellSleeper.Sleeping("blink_fix"))
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
                                _spellSleeper.Sleep(250, "blink_fix");
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
                            _spellSleeper.Sleep(250,"blink_fix");
                        }
                    }
                    else
                    {
                        item.UseAbility(globalTarget.NetworkPosition);
                    }
                }
                Utils.Sleep(150, $"{item.Handle}+item_usages");

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
                    _rockets.UseAbility();
                    _spellSleeper.Sleep(500, _rockets);
                }*/
            }
            if (singleCombo)
                return;
            var rearm = Abilities.FindAbility("tinker_rearm");

            if (inventory.Count != 0 ||
                (rockets.CanBeCasted() && Helper.IsAbilityEnable(rockets.StoredName()) &&
                 Helper.CanRockedHit(globalTarget)) ||
                (laser.CanBeCasted() && Helper.IsAbilityEnable(laser.StoredName()) && laser.CanHit(globalTarget)) || _spellSleeper.Sleeping("blink_fix"))
            {
                Printer.Print($"{inventory.Count}");
                return;
            }
            //Printer.Print($"{rearm != null}/{Helper.IsAbilityEnable(rearm.StoredName())}/{rearm.CanBeCasted()}/{!_spellSleeper.Sleeping(rearm)}/{!Members.MyHero.IsChanneling()}");
            if (rearm != null && Helper.IsAbilityEnable(rearm.StoredName()) && rearm.CanBeCasted() && !_spellSleeper.Sleeping(rearm) && !Members.MyHero.IsChanneling())
            {
                rearm.UseAbility();
                //_spellSleeper.Sleep(4000 - rearm.Level * 500, rearm);
                _spellSleeper.Sleep(750+530, rearm);
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

        public static void OnValueChanged(object sender, OnValueChangeEventArgs args)
        {
            var oldOne = args.GetOldValue<KeyBind>().Active;
            var newOne = args.GetNewValue<KeyBind>().Active;
            if (oldOne == newOne) return;
            if (newOne)
            {
                Log.Debug("Start Combo");
                return;
            }
            try
            {
                Log.Debug("End Combo");
                _tks.Cancel();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}