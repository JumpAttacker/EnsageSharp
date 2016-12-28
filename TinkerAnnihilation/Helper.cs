using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.AbilityInfo;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.Damage;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace TinkerAnnihilation
{
    public static class Helper
    {

        public static Hero ClosestToMouse(Hero source, float range = 600)
        {
            var mousePosition = Game.MousePosition;
            var enemyHeroes = ObjectManager.GetEntities<Hero>()
                .Where(
                    x =>
                        x.Team == source.GetEnemyTeam() && !x.IsIllusion && x.IsAlive && x.IsVisible &&
                        x.Distance2D(mousePosition) <= range /*&& !x.IsMagicImmune()*/)
                .OrderBy(x => x.Distance2D(mousePosition));
            return enemyHeroes.FirstOrDefault();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit">face</param>
        /// <param name="target">where</param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector3 InFront(Unit unit,Unit target, float distance)
        {
            var v = target.Position + (unit.Vector3FromPolarAngle() * distance);
            return new Vector3(v.X, v.Y, 0);
        }
        public static bool IsItemEnable(string name)
        {
            return Members.Menu.Item("itemEnable").GetValue<PriorityChanger>().AbilityToggler.IsEnabled(name);
        }
        public static bool IsItemEnableNew(string name)
        {
            return Members.Menu.Item("itemEnable").GetValue<PriorityChanger>().AbilityToggler.IsEnabled(name);
        }
        public static bool IsAbilityEnable(string name)
        {
            return Members.Menu.Item("abilityEnable").GetValue<AbilityToggler>().IsEnabled(name);
        }

        public static uint PriorityHelper(Item item)
        {
            return Members.Menu.Item("itemEnable").GetValue<PriorityChanger>().GetPriority(item.StoredName());
        }
        public static uint PriorityHelper(Ability item)
        {
            return Members.Menu.Item("itemEnable").GetValue<PriorityChanger>().GetPriority(item.StoredName());
        }

        public static bool CanRockedHit(Hero globalTarget)
        {
            var allHeroes =
                        Heroes.GetByTeam(Members.MyHero.GetEnemyTeam())
                            .Where(x => x.IsAlive && x.IsVisible && !x.IsMagicImmune())
                            .OrderBy(y => y.Distance2D(Members.MyHero)).ToList();
            var range = allHeroes.GetRange(0, Math.Min(allHeroes.Count, Members.MyHero.AghanimState() ? 4 : 2));
            return range.Contains(globalTarget);
        }

        public static float CalculateMyDamage(Hero globalTarget)
        {
            var mana = Members.MyHero.MaximumMana;
            var rearm = Abilities.FindAbility("tinker_rearm");
            var laser = Abilities.FindAbility("tinker_laser");
            var rocked = Abilities.FindAbility("tinker_heat_seeking_missile");
            var allItems = new List<Ability>();
            var allItems2 = new List<Ability>(Members.MyHero.Inventory.Items.Where(x =>
                IsItemEnableNew(x.StoredName()) && (x.ManaCost > 0 || x.StoredName() == "item_soul_ring"))
                .OrderBy(PriorityHelper)
                .ToList());
            
            allItems.AddRange(allItems2);
            
            if (rocked.Level > 0 && IsItemEnableNew(rocked.StoredName()))
                allItems.Add(rocked);
            if (laser.Level > 0 && IsItemEnableNew(laser.StoredName()))
                allItems.Add(laser);
            if (rearm.Level > 0)
                allItems.Add(rearm);
            var haveEb = allItems.Any(x => x.StoredName() == "item_ethereal_blade" && x.CanBeCasted());
            var haveVeil = allItems.Any(x => x.StoredName() == "item_veil_of_discord" && x.CanBeCasted());
            var myDmg = 0f;
            var extraDamage = haveEb && !globalTarget.HasModifier("modifier_item_ethereal_blade_ethereal") ? 40 : 0;
            extraDamage += haveVeil && !globalTarget.HasModifier("modifier_item_veil_of_discord_debuff") ? 25 : 0;
            if (allItems.Count == 0 || (rearm.Level == 0))
            {
                myDmg=allItems.Sum(
                    x =>
                        AbilityDamage.CalculateDamage(x, Members.MyHero, globalTarget,
                            minusMagicResistancePerc: extraDamage));
                return (int) (globalTarget.Health - myDmg);
            }
            
            //Printer.Print($"[mana]: init");
            Printer.ConsolePrint($"[mana]: Init. [count]: {allItems.Count}. [possible]: {allItems.Count(x => mana > x.ManaCost)}");
            var wasRearmed = true;
            var templarStacks = globalTarget.FindModifier("modifier_templar_assassin_refraction_absorb_stacks");
            var stacks = templarStacks?.StackCount ?? 0;
            var hasRainDrop = globalTarget.FindItem("item_infused_raindrop", true)?.Cooldown <= 0;
            var wasNama = mana;
            var linkDef = globalTarget.IsLinkensProtected();
            while (mana>5 && allItems.Count(x=>mana>x.ManaCost)>0 && wasRearmed && wasNama>=mana)
            {
                wasRearmed = false;
                foreach (var x in allItems)
                {
                    if (mana > x.ManaCost)
                    {
                        //Printer.ConsolePrint($"[mana]: {x.StoredName()} -> {x.ManaCost}/{mana}");
                        if (x.StoredName() == "item_soul_ring")
                        {
                            mana += 150;
                            Printer.ConsolePrint($"[mana]: {mana} (+150) soul ring");
                            continue;
                        }

                        var mCost = x.ManaCost;
                        if (!(mana - mCost > 0)) break;
                        mana -= mCost;

                        var dmgFromSpell = AbilityDamage.CalculateDamage(x, Members.MyHero, globalTarget,
                            minusMagicResistancePerc: extraDamage);
                        if (x.IsAbilityBehavior(AbilityBehavior.UnitTarget) && linkDef && !x.IsDisable())
                        {
                            dmgFromSpell = 0;
                            linkDef = false;
                        }
                        if (stacks > 0)
                        {
                            stacks--;
                            myDmg += 0;
                        }
                        else
                        {
                            if (AbilityDamage.GetDamageType(x) == DamageType.Magical && hasRainDrop && dmgFromSpell > 50)
                            {
                                hasRainDrop = false;
                                dmgFromSpell -= Math.Min(120, dmgFromSpell);
                                dmgFromSpell = Math.Max(dmgFromSpell, 0);
                            }
                            myDmg += dmgFromSpell;
                        }
                        //Printer.Print($"[mana]: {mana} (-{mCost}) {x.StoredName()} -> damage: {myDmg}");

                        if (x.StoredName() == rearm.StoredName())
                        {
                            Printer.ConsolePrint($"[mana]: {mana} (-{mCost}) {x.StoredName()}");
                            wasRearmed = true;
                            continue;
                        }
                        Printer.ConsolePrint($"[mana]: {mana} (-{mCost}) {x.StoredName()} -> damage: {dmgFromSpell} || total:{myDmg}");
                    }
                    else
                    {
                        Printer.ConsolePrint($"[mana]: {x.StoredName()} -> {x.ManaCost}/{mana} cant cast this!");
                    }
                }
                /*Printer.ConsolePrint($"[mana]: {rearm.StoredName()} -> {rearm.ManaCost}/{mana}");
                mana -= rearm.ManaCost;*/
            }
            var healthAfterShit = (int) (globalTarget.Health - myDmg);
            
            return healthAfterShit;
            
        }
        public static float CalculateMyCurrentDamage(Hero globalTarget,bool checkForRange=false)
        {
            var mana = Members.MyHero.Mana;
            var laser = Abilities.FindAbility("tinker_laser");
            var rocked = Abilities.FindAbility("tinker_heat_seeking_missile");
            var allItems = new List<Ability>();
            var allItems2 = new List<Ability>(Members.MyHero.Inventory.Items.Where(x =>
                IsItemEnableNew(x.StoredName()) && (x.ManaCost > 0 || x.StoredName() == "item_soul_ring"))
                .OrderBy(PriorityHelper)
                .ToList());

            allItems.AddRange(allItems2);

            if (laser.Level > 0 && IsItemEnableNew(laser.StoredName()))
                allItems.Add(laser);

            if (rocked.Level > 0 && IsItemEnableNew(rocked.StoredName()))
                allItems.Add(rocked);
            
            var haveEb =
                allItems.Any(
                    x =>
                        x.StoredName() == "item_ethereal_blade" && x.CanBeCasted() &&
                        (x.CanHit(globalTarget) || !checkForRange));
            var haveVeil =
                allItems.Any(
                    x =>
                        x.StoredName() == "item_veil_of_discord" && x.CanBeCasted() &&
                        (x.CanHit(globalTarget) || !checkForRange));
            var myDmg = 0f;
            var extraDamage = haveEb && !globalTarget.HasModifier("modifier_item_ethereal_blade_ethereal") ? 40 : 0;
            extraDamage += haveVeil && !globalTarget.HasModifier("modifier_item_veil_of_discord_debuff") ? 25 : 0;
            var ignoreList=new List<Ability>();
            var linkDef = globalTarget.IsLinkensProtected();
            foreach (
                var x in
                    allItems.Where(x => x.CanBeCasted() && !ignoreList.Contains(x))
                        .Where(x => !checkForRange || x.CanHit(globalTarget)))
            {
                if (x.Equals(rocked))
                    if (!CanRockedHit(globalTarget))
                    {
                        ignoreList.Add(rocked);
                        continue;
                    }
                if (mana > x.ManaCost)
                {
                    if (x.StoredName() == "item_soul_ring")
                    {
                        mana += 150;
                        continue;
                    }
                    var mCost = x.ManaCost;
                    if (!(mana - mCost > 0)) break;
                    mana -= mCost;
                    var dmgFromSpell = AbilityDamage.CalculateDamage(x, Members.MyHero, globalTarget,
                        minusMagicResistancePerc: extraDamage);
                    if (x.IsAbilityBehavior(AbilityBehavior.UnitTarget) && linkDef && !x.IsDisable())
                    {
                        dmgFromSpell = 0;
                        linkDef = false;
                    }
                    myDmg += dmgFromSpell;
                }
            }
            return (int) (globalTarget.Health - myDmg);
        }

        private static Dictionary<uint, ParticleEffect> _effects;
        public static void HandleEffect(Hero target)
        {
            if (_effects==null)
                _effects=new Dictionary<uint, ParticleEffect>();
            var handle = target.Handle;
            ParticleEffect effect;
            if (!_effects.TryGetValue(handle, out effect))
            {
                effect = Members.MyHero.AddParticleEffect("materials/ensage_ui/particles/target.vpcf");
                /*
                 *  2 Control Point (Position X, Y, Z)
                    5 Control Point (Color X, Y, Z)
                    6 Control Point (Alpha !!!(1)!!! X)
                    7 Control Point (Position X, Y, Z) 
                 * */
                effect.SetControlPoint(2, Members.MyHero.Position);
                effect.SetControlPoint(5, new Vector3(Members.TargetR, Members.TargetG, Members.TargetB));
                effect.SetControlPoint(6, new Vector3(255));
                effect.SetControlPoint(7, target.Position);
                _effects.Add(handle,effect);
            }
            else
            {
                effect.SetControlPoint(2, Members.MyHero.Position);
                effect.SetControlPoint(7, target.Position);
            }
        }
        public static void UnHandleEffect(Hero target)
        {
            var handle = target.Handle;
            ParticleEffect effect;
            if (_effects.TryGetValue(handle, out effect))
            {
                effect.Dispose();
                _effects.Remove(handle);
            }
        }
    }
}