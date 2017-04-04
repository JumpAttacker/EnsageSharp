using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
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
using AbilityId = Ensage.Common.Enums.AbilityId;

namespace Auto_Disable
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static class Core
    {
        public static Hero Me;
        private static readonly MultiSleeper ComboSleeper = new MultiSleeper();
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Unit _fountain;
        public static void UpdateLogic(EventArgs args)
        {
            if (!Me.IsAlive || !Me.CanUseItems()) return;
            var myAllItems = Me.Inventory.Items.Where(x => Members.Items.Contains(x.StoredName()) && x.CanBeCasted());
            var myAllAbilities = Me.Spellbook.Spells.Where(x => Members.Spells.Contains(x.StoredName()) && x.CanBeCasted());
            foreach (var v in Heroes.GetByTeam(Me.GetEnemyTeam()).Where(x=>x.IsAlive && x.IsVisible && !ComboSleeper.Sleeping(x.StoredName())))
            {
                MenuManager.TryToInitNewHero(v);
                var myItems =
                    myAllItems.Where(
                        x =>
                            !ComboSleeper.Sleeping(x.StoredName() + v.StoredName()) && MenuManager.IsItemEnable(v, x.StoredName()) &&
                            (x.GetItemId() == ItemId.item_blink || x.CanHit(v) || x.IsShield()));
                var myAbilities =
                    myAllAbilities.Where(
                        x =>
                            !ComboSleeper.Sleeping(x.StoredName() + v.StoredName()) && MenuManager.IsAbilityEnable(v, x.StoredName()) &&
                            (x.CanHit(v) || x.IsShield()));
                //Printer.Print($"{v.GetRealName()}: 1 Ability: {myAbilities.Any()}");
                if (!myItems.Any() && !myAbilities.Any())
                    continue;
                var isInvul = v.IsInvul();
                //var magicImmnune = v.IsMagicImmune();
                var linkProt = v.IsLinkensProtected();
                var isStun = v.IsStunned();
                var isHex = v.IsHexed();
                var isSilence = v.IsSilenced();
                var angle = (float) Math.Max(
                    Math.Abs(v.RotationRad - Utils.DegreeToRadian(v.FindAngleBetween(Me.Position))) - 0.20, 0);
                var dpActivated =
                    v.HasModifiers(new[] {"modifier_slark_dark_pact", "modifier_slark_dark_pact_pulses"}, false);
                var blink = v.FindItem322(ItemId.item_blink);
                if (isHex || isSilence || isStun)
                    continue;
                var distance = Me.Distance2D(v);
                IEnumerable<Item> myItems2 = myItems.ToList();
                IEnumerable<Ability> myAbilities2 = myAbilities.ToList();
                if (MenuManager.IsAngryDisabler)
                {
                    if (!dpActivated && !linkProt && !isInvul &&
                        MenuManager.CheckForMoveItem(v, ref myItems2, ref myAbilities2, "Angry Disabler"))
                    {
                        if (TryToDisable(v, myItems2, myAbilities2))
                            continue;
                    }
                }
                myItems2 = myItems.ToList();
                myAbilities2 = myAbilities.ToList();
                if (distance <= 500 &&
                    ((v.HasModifier("modifier_item_forcestaff_active") &&
                      MenuManager.CheckForMoveItem(v, ref myItems2, ref myAbilities2, "item_force_staff")) ||
                     (blink != null && blink.Cooldown/blink.CooldownLength > 0.99 && blink.CooldownLength >= 5 &&
                      MenuManager.CheckForMoveItem(v, ref myItems2, ref myAbilities2, "item_blink"))) &&
                    v.HasDangAbility())
                {
                    if (dpActivated /*|| magicImmnune*/ || linkProt || isInvul)
                    {
                        if (TryToEscape(v, myItems2, myAbilities2))
                            continue;
                    }
                    else
                    {
                        if (TryToDisable(v, myItems2, myAbilities2))
                            continue;
                        if (TryToEscape(v, myItems2, myAbilities2))
                            continue;
                    }
                }
                if (v.IsChanneling() && MenuManager.IsDisableChannelGuys)
                {
                    if (TryToDisable(v, myItems, myAbilities))
                        continue;
                }
                AbilityId initAbility;
                if (Members.Initiators.TryGetValue(v.ClassID, out initAbility) && MenuManager.IsAntiInitiators)
                {
                    var initSpell = v.FindSpell322(initAbility);
                    if (initSpell != null && (initSpell.Cooldown / initSpell.CooldownLength > 0.99 || initSpell.IsInAbilityPhase))
                    {
                        if (TryToDisable(v, myItems, myAbilities))
                            continue;
                    }
                }
                /*if (Members.DangIltimate.Contains(v.ClassID))
                {
                    var ult =
                        v.Spellbook.Spells.Any(x => x.AbilityType == AbilityType.Ultimate && x.IsInAbilityPhase && x.CanHit(Me));
                    if (ult)
                    {
                        if (TryToEscape(v, myItems, myAbilities))
                            continue;
                    }
                }*/
                if (MenuManager.HelpAllyHeroes && TryToHelpAllyHeroes(v, myItems, myAbilities))
                    continue;
                if ((angle != 0 && !v.Spellbook.Spells.Any(
                    x =>
                        x.IsInAbilityPhase && (x.AbilityBehavior & AbilityBehavior.NoTarget) != 0 && x.IsDisable() &&
                        x.CanHit(Me) && x.CanBeCasted(Me))) || distance >= 1000)
                {
                    continue;
                }
                var anyDangAbility =
                    v.Spellbook.Spells.FirstOrDefault(
                        x =>
                            x.IsInAbilityPhase && ((x.IsDisable() && x.CanHit(Me)) || x.IsShield()) && x.CanBeCasted(Me));
                if (anyDangAbility!=null)
                {
                    if (dpActivated /*|| magicImmnune */|| linkProt || isInvul)
                    {
                        if (TryToEscape(v,
                            myItems.Where(x => MenuManager.IsItemEnable(v, x.StoredName(), anyDangAbility.StoredName())),
                            myAbilities.Where(
                                x => MenuManager.IsAbilityEnable(v, x.StoredName(), anyDangAbility.StoredName()))))
                            continue;
                    }
                    else
                    {
                        if (TryToDisable(v,
                            myItems.Where(x => MenuManager.IsItemEnable(v, x.StoredName(), anyDangAbility.StoredName())),
                            myAbilities.Where(
                                x => MenuManager.IsAbilityEnable(v, x.StoredName(), anyDangAbility.StoredName()))))
                            continue;
                        if (TryToEscape(v,
                            myItems.Where(x => MenuManager.IsItemEnable(v, x.StoredName(), anyDangAbility.StoredName())),
                            myAbilities.Where(
                                x => MenuManager.IsAbilityEnable(v, x.StoredName(), anyDangAbility.StoredName()))))
                            continue;
                    }
                }
            }
        }

        private static bool TryToHelpAllyHeroes(Hero v, IEnumerable<Item> myItems, IEnumerable<Ability> myAbilities)
        {
            var allyHeroes =
                Heroes.GetByTeam(Members.MyTeam)
                    .Where(x => x != null && x.IsValid && !x.Equals(Me) && x.IsAlive && x.Distance2D(v) <= 1000 && (float)
                Math.Max(Math.Abs(v.RotationRad - Utils.DegreeToRadian(v.FindAngleBetween(x.Position))) - 0.20, 0) == 0);
            foreach (var allyHero in from allyHero in allyHeroes let any = v.Spellbook.Spells.Any(
                x =>
                    x.IsInAbilityPhase && x.IsDisable() &&
                    x.CanHit(allyHero) && x.CanBeCasted(allyHero)) where any select allyHero)
            {
                Log.Debug($"Help {allyHero.GetRealName()} from {v.GetRealName()}");
                TryToDisable(v, myItems, myAbilities);
            }
            return false;
        }

        private static bool TryToDisable(Hero hero, IEnumerable<Item> myItems, IEnumerable<Ability> myAbilities)
        {
            if (MenuManager.IsNinjaMode && !Me.IsVisibleToEnemies)
                return false;
            myAbilities = myAbilities.Where(x => !Members.EscapeAbilityList.Contains(x.GetAbilityId()) &&  !x.IsShield() && !x.IsShield());
            myItems = myItems.Where(x => !Members.EscapeItemList.Contains(x.GetItemId()) && !x.IsShield() && !x.IsShield());
            if (myItems.Any())
            {
                var item = myItems.First();
                if ((item.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    item.UseAbility(hero.Position);
                }
                else if ((item.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    item.UseAbility(hero);
                }
                else
                {
                    item.UseAbility();
                }
                ComboSleeper.Sleep(350, item.StoredName() + hero.StoredName());
                Log.Debug($"item: {item.StoredName()}");
                if (MenuManager.IsUseOnlyOne)
                    ComboSleeper.Sleep(350, hero.StoredName());
                return MenuManager.IsUseOnlyOne;
            }
            if (myAbilities.Any())
            {
                var ability = myAbilities.First();
                if ((ability.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    if (ability.IsSkillShot())
                    {
                        ability.CastSkillShot(hero);
                    }
                    else
                        ability.UseAbility(hero.Position);
                }
                else if ((ability.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    ability.UseAbility(hero);
                }
                else
                {
                    ability.UseAbility();
                }
                ComboSleeper.Sleep(Helper.GetAbilityDelay(hero,ability), ability.StoredName() + hero.StoredName());
                Log.Debug($"ability: {ability.StoredName()} --> {hero.GetRealName()}");
                if (MenuManager.IsUseOnlyOne)
                    ComboSleeper.Sleep(350, hero.StoredName());
                return MenuManager.IsUseOnlyOne;
            }
            return false;
        }

        private static bool TryToEscape(Hero hero, IEnumerable<Item> myItems, IEnumerable<Ability> myAbilities)
        {
            myAbilities =
                myAbilities.Where(
                    x => Members.EscapeAbilityList.Contains(x.GetAbilityId()) || x.IsInvis() || x.IsShield())
                    .OrderByDescending(x => x.CastRange);
            myItems =
                myItems.Where(x => Members.EscapeItemList.Contains(x.GetItemId()) || x.IsInvis() || x.IsShield())
                    .OrderByDescending(x => x.CastRange);
            if (myItems.Any())
            {
                var item = myItems.First();
                if ((item.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    var castRange = item.GetCastRange() - 10;
                    var position = Me.Position;
                    var angle = _fountain.FindAngleBetween(position, true);
                    var point = new Vector3(
                        (float)
                            (position.X - castRange*Math.Cos(angle)),
                        (float)
                            (position.Y - castRange*Math.Sin(angle)),
                        0);
                    item.UseAbility(point);
                }
                else if ((item.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    item.UseAbility(Me);
                }
                else
                {
                    item.UseAbility();
                }
                Log.Debug($"item: {item.StoredName()} --> from {hero.GetRealName()}");
                ComboSleeper.Sleep(Helper.GetAbilityDelay(hero, item), item.StoredName() + hero.StoredName());
                if (MenuManager.IsUseOnlyOne)
                    ComboSleeper.Sleep(350, hero.StoredName());
                return MenuManager.IsUseOnlyOne;
            }
            if (myAbilities.Any())
            {
                var ability = myAbilities.First();
                if ((ability.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    var castRange = ability.GetCastRange() - 10;
                    var position = Me.Position;
                    var point = new Vector3(
                        (float)
                            (position.X - castRange*Math.Cos(Me.FindAngleBetween(position, true))),
                        (float)
                            (position.Y - castRange*Math.Sin(Me.FindAngleBetween(position, true))),
                        0);
                    ability.UseAbility(point);
                }
                else if ((ability.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    ability.UseAbility(Me);
                }
                else
                {
                    ability.UseAbility();
                }
                Log.Debug($"ability: {ability.StoredName()} --> from {hero.GetRealName()}");
                ComboSleeper.Sleep(350, ability.StoredName() + hero.StoredName());
                if (MenuManager.IsUseOnlyOne)
                    ComboSleeper.Sleep(350, hero.StoredName());
                return MenuManager.IsUseOnlyOne;
            }
            return false;
        }

        public static void Updater(EventArgs args)
        {
            if (!MenuManager.IsEnable)
                return;
            if (Members.Updater.Sleeping)
                return;
            if (_fountain == null || !_fountain.IsValid)
            {
                _fountain = ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == Me.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                if (_fountain != null)
                {
                    Log.Info($"[Init] fountain {_fountain.Team}");
                }
            }
            Members.Updater.Sleep(500);
            var inventory = Me.Inventory.Items;
            var enumerable = inventory as IList<Item> ?? inventory.ToList();
            var neededItems =
                enumerable.Where(
                    item =>
                        !Members.Items.Contains(item.StoredName()) &&
                        (item.IsDisable() || item.IsSilence() || item.IsShield() || item.IsInvis() ||
                         Members.WhiteList.Contains(item.GetItemId())));
            var spellsForUpdate =
                Me.Spellbook.Spells.Where(
                    x =>
                        !Members.Spells.Contains(x.StoredName()) &&
                        (x.IsDisable() || x.IsSilence() || x.IsInvis() || x.IsShield()));
            foreach (var spell in spellsForUpdate)
            {
                Members.Spells.Add(spell.StoredName());
                MenuManager.Menu.Item("abilityEnable")
                    .GetValue<AbilityToggler>().Add(spell.StoredName());
                MenuManager.UpdateAbility(spell.StoredName());
                Printer.Print($"[NewAbility]: {spell.StoredName()} shi: {spell.IsShield()} dis: {spell.IsDisable()}");
                Log.Debug($"[NewAbility]: {spell.StoredName()} shield: {spell.IsShield()} disable: {spell.IsDisable()}");
            }
            foreach (var item in neededItems)
            {
                Members.Items.Add(item.StoredName());
                MenuManager.Menu.Item("itemEnable")
                    .GetValue<AbilityToggler>().Add(item.StoredName());
                Printer.Print($"[NewItem]: {item.StoredName()}");
                Log.Debug($"[NewItem]: {item.StoredName()}");
                MenuManager.UpdateItem(item.StoredName());
            }
            var tempList = enumerable.Select(neededItem => neededItem.StoredName()).ToList();
            var removeList = new List<string>();
            foreach (var item in Members.Items.Where(x => !tempList.Contains(x)))
            {
                MenuManager.Menu.Item("itemEnable")
                    .GetValue<AbilityToggler>().Remove(item);
                MenuManager.RemoveItem(item);
                removeList.Add(item);
                Printer.Print($"[RemoveItem]: {item}");
            }
            foreach (var item in removeList)
            {
                Members.Items.Remove(item);
            }
        }
    }
}