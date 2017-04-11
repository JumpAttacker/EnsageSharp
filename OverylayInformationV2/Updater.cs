using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Ensage;
using Ensage.Abilities;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using Ensage.Heroes;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    internal abstract class Updater
    {
        public abstract class HeroList
        {
            private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            private static Sleeper _abilityUpdate = new Sleeper();
            //private static readonly Sleeper ItemUpdate = new Sleeper();
            private static Sleeper _heroUpdate = new Sleeper();
            private static Sleeper _updatePrediction = new Sleeper();
            private static Sleeper _courUpdater = new Sleeper();
            private static readonly List<string> IgnoreList=new List<string>
            {
                "npc_dota_beastmaster_boar_1",
                "npc_dota_beastmaster_boar_2",
                "npc_dota_beastmaster_boar_3",
                "npc_dota_beastmaster_boar_4",
            };

            public static void Update(EventArgs args)
            {

                if (!Checker.IsActive()) return;
                if (!_courUpdater.Sleeping)
                {
                    _courUpdater.Sleep(100);
                    Members.CourList =
                        ObjectManager.GetEntities<Courier>().Where(x => x.Team != Members.MyHero.Team).ToList();
                    foreach (var courier in Members.CourList)
                    {
                        if (Members.ItemDictionary.ContainsValue(
                            courier.Inventory.Items.Where(x => x.IsValid).ToList())) continue;
                        var items = courier.Inventory.Items.ToList();
                        Members.ItemDictionary.Remove(courier.Handle);
                        Members.ItemDictionary.Add(courier.Handle,
                            items.Where(x => x.IsValid).ToList());
                    }
                }
                if (!_heroUpdate.Sleeping)
                {
                    /*foreach (var enemyHero in Members.EnemyHeroes)
                    {
                        var worth = enemyHero.Inventory.Items.Aggregate<Item, long>(0, (current, item) => current + item.Cost);
                        Printer.Print(enemyHero.Name + " --> " + worth);
                    }*/
                    _heroUpdate.Sleep(2000);
                    if (Members.Heroes.Count < 10 + (Members.MeepoDivided != null ? Members.MeepoDivided.UnitCount - 1 : 0))
                    {
                        /*Members.Heroes =
                            Heroes.All.Where(
                                x =>
                                    x != null && x.IsValid && !x.IsIllusion && !IgnoreList.Contains(x.StoredName())).ToList();*/
                        Members.Heroes =
                            HeroesList.All.Where(
                                x =>
                                    x != null && x.IsValid && !x.IsIllusion && !IgnoreList.Contains(x.StoredName()))
                                .ToList();
                        /*Printer.Print($"---------------");
                        foreach (var source in Members.Heroes.Where(x=>x.ClassId == ClassId.CDOTA_Unit_Hero_Meepo))
                        {
                            var hero = source as Meepo;
                            var isIllusion = hero.IsIllusion();
                            if (hero != null) Printer.Print($"Hero({isIllusion}): {hero.Name}");
                        }*/
                        /*Members.Heroes =
                            ObjectManager.GetEntities<Hero>().Where(
                                x =>
                                    x != null && x.IsValid && !x.IsIllusion && !IgnoreList.Contains(x.StoredName())).ToList();*/

                        foreach (
                            var meepo in
                                Members.Heroes.Where(
                                    x =>
                                    {
                                        var dividedWeStand = x.Spellbook.SpellR as DividedWeStand;
                                        return dividedWeStand != null && !Members.MeepoIgnoreList.Contains(x) &&
                                               x.ClassId == ClassId.CDOTA_Unit_Hero_Meepo &&
                                               dividedWeStand.UnitIndex > 0;
                                    }))
                        {
                            Members.MeepoIgnoreList.Add(meepo);
                        }
                        Members.AllyHeroes = Members.Heroes.Where(x => x.Team == Members.MyHero.Team).ToList();
                        Members.EnemyHeroes =
                            Members.Heroes.Where(x => x.Team == Members.MyHero.GetEnemyTeam()).ToList();
                        //Printer.Print($"Heroes.All: {Heroes.All.Count}; Members.Heroes: {Members.Heroes.Count}");
                        //Printer.Print("STATUS:[all] " + Members.Heroes.Count+ " [enemy] " + Members.EnemyHeroes.Count + " [ally] " + Members.AllyHeroes.Count);
                        if (!Members.Apparition &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_AncientApparition))
                        {
                            Printer.Print("Apparition detected");
                            Members.Apparition = true;
                        }
                        if (Members.PAisHere == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_PhantomAssassin))
                        {
                            Printer.Print("PhantomAssss detected");
                            Members.PAisHere = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_PhantomAssassin);
                        }
                        if (!Members.BaraIsHere &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_SpiritBreaker))
                        {
                            Printer.Print("BaraIsHere detected");
                            Members.BaraIsHere = true;
                        }
                        if (Members.Mirana == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Mirana))
                        {
                            Printer.Print("Mirana detected");
                            Members.Mirana = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Mirana);
                        }
                        if (Members.Windrunner == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Windrunner))
                        {
                            Printer.Print("Windrunner detected");
                            Members.Windrunner = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Windrunner);
                        }
                        if (Members.Invoker == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Invoker))
                        {
                            Printer.Print("Invoker detected");
                            Members.Invoker = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Invoker);
                        }
                        if (Members.Kunkka == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Kunkka))
                        {
                            Printer.Print("Kunkka detected");
                            Members.Kunkka = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Kunkka);
                        }
                        if (Members.Lina == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Lina))
                        {
                            Printer.Print("Lina detected");
                            Members.Lina = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Lina);
                        }
                        if (Members.Leshrac == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Leshrac))
                        {
                            Printer.Print("Leshrac detected");
                            Members.Leshrac = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Leshrac);
                        }
                        if (Members.LifeStealer == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Life_Stealer))
                        {
                            Printer.Print("LifeStealer detected");
                            Members.LifeStealer = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Life_Stealer);
                        }

                        if (Members.Techies == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Techies))
                        {
                            Printer.Print("Techies detected");
                            Members.Techies = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Techies);
                        }
                        if (Members.Tinker == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Tinker))
                        {
                            Printer.Print("Tinker detected");
                            Members.Tinker = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Tinker);
                        }
                        if (Members.ArcWarden == null &&
                            Members.EnemyHeroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_ArcWarden))
                        {
                            Printer.Print("ArcWarden detected");
                            Members.ArcWarden = Members.EnemyHeroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_ArcWarden);
                        }
                        //DividedWeStand
                        if (Members.Meepo == null &&
                            Members.Heroes.Any(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Meepo))
                        {
                            Printer.Print("Meepo detected");
                            Members.Meepo = Members.Heroes.FirstOrDefault(x => x.ClassId == ClassId.CDOTA_Unit_Hero_Meepo);
                            Members.MeepoDivided = (DividedWeStand) Members.Meepo.Spellbook.SpellR;
                        }
                    }
                }
                if (!_updatePrediction.Sleeping /*&& Members.Menu.Item("lastPosition.Enable.Prediction").GetValue<bool>()*/)
                {
                    _updatePrediction.Sleep(1);
                    var time = Game.GameTime;
                    foreach (var v in Members.EnemyHeroes.Where(x=>x.IsAlive))
                    {
                        if (v.IsVisible)
                        {
                            if (Members.PredictionTimes.ContainsKey(v.StoredName()))
                                Members.PredictionTimes.Remove(v.StoredName());
                        }
                        else
                        {
                            float test;
                            if (!Members.PredictionTimes.TryGetValue(v.StoredName(), out test))
                            {
                                Members.PredictionTimes.Add(v.StoredName(), time);
                            }
                            /*else
                            {
                                Members.PredictionTimes[v.StoredName()] = time;
                            }*/

                        }
                    }
                }
                if (!_abilityUpdate.Sleeping)
                {
                    _abilityUpdate.Sleep(1000);
                    foreach (var hero in /*Members.Heroes */Manager.HeroManager.GetViableHeroes())
                    {
                        /*if ((hero.ClassId==ClassId.CDOTA_Unit_Hero_DoomBringer || hero.ClassId==ClassId.CDOTA_Unit_Hero_Rubick) && !hero.IsVisible)
                            continue;*/
                        try
                        {
                            if (!Members.AbilityDictionary.ContainsKey(hero.Handle))
                                Members.AbilityDictionary.Add(hero.Handle,
                                    hero.Spellbook.Spells.Where(
                                        x =>
                                            x!=null && x.IsValid && x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden &&
                                            x.AbilitySlot.ToString() != "-1")
                                        .ToList());
                            if (
                                !Members.AbilityDictionary.ContainsValue(
                                    hero.Spellbook.Spells.Where(x => x.AbilitySlot.ToString() != "-1").ToList()))
                            {
                                Members.AbilityDictionary.Remove(hero.Handle);
                                Members.AbilityDictionary.Add(hero.Handle, hero.Spellbook.Spells.Where(
                                    x =>
                                        x.IsValid && x.AbilityType != AbilityType.Attribute && x.AbilityType != AbilityType.Hidden &&
                                            x.AbilitySlot.ToString() != "-1")
                                        .ToList());

                            }
                            long worth = 0;
                            /*if ((hero.ClassId == ClassId.CDOTA_Unit_Hero_Meepo) &&
                                (hero.FindSpell("meepo_divided_we_stand") as DividedWeStand).UnitIndex > 0)
                            {
                                continue;
                            }*/
                            if (Members.MeepoIgnoreList.Contains(hero))
                                continue;
                            Members.NetWorthDictionary.Remove(hero.StoredName());
                            if (!Members.ItemDictionary.ContainsValue(
                                    hero.Inventory.Items.Where(x => x.IsValid).ToList()))
                            {
                                var items = hero.Inventory.Items.ToList();
                                Members.ItemDictionary.Remove(hero.Handle);
                                Members.ItemDictionary.Add(hero.Handle,
                                    items.Where(x => x.IsValid).ToList());
                                worth += items.Aggregate<Item, long>(0, (current, item) => current + item.Cost);
                            }
                            if ((Members.Menu.Item("itempanel.Stash.Enable").GetValue<bool>() || Members.Menu.Item("netWorth.Enable").GetValue<bool>()) &&
                                !Members.StashItemDictionary.ContainsValue(
                                    hero.Inventory.Stash.Where(x => x.IsValid).ToList()))
                            {
                                var items = hero.Inventory.Stash.ToList();
                                Members.StashItemDictionary.Remove(hero.Handle);
                                Members.StashItemDictionary.Add(hero.Handle,
                                    items.Where(x => x.IsValid).ToList());
                                worth += items.Aggregate<Item, long>(0, (current, item) => current + item.Cost);
                            }
                            Members.NetWorthDictionary.Add(hero.StoredName(), worth);
                        }
                        catch (Exception)
                        {
                            Printer.Print("[UPDATER.ITEMS/ABILITY: ] " + hero.StoredName());
                        }
                        
                    }
                }
            }

            public static void Flush()
            {
                _abilityUpdate = new Sleeper();
                _heroUpdate = new Sleeper();
                _updatePrediction = new Sleeper();
                _courUpdater=new Sleeper();
            }
        }

        public abstract class PlayerList
        {
            private static Sleeper _sleeper = new Sleeper();

            public static void Update(EventArgs args)
            {
                if (!Checker.IsActive()) return;
                if (_sleeper.Sleeping) return;
                _sleeper.Sleep(2000);
                if (Members.Players.Count(x => x != null && x.IsValid && x.Hero.IsValid) < 10)
                {
                    Members.Players = Players.All.Where(x => x != null && x.IsValid && x.Hero!=null && x.Hero.IsValid).ToList();
                    Members.AllyPlayers = Members.Players.Where(x => x.Team == Members.MyHero.Team).ToList();
                    Members.EnemyPlayers = Members.Players.Where(x => x.Team == Members.MyHero.GetEnemyTeam()).ToList();
                }
            }
            public static void Flush()
            {
                _sleeper = new Sleeper();
            }

        }

        public abstract class BaseList
        {
            private static Sleeper _sleeper = new Sleeper();

            public static void Update(EventArgs args)
            {
                if (!Checker.IsActive()) return;
                if (_sleeper.Sleeping) return;
                _sleeper.Sleep(100);
                Members.BaseList =
                    ObjectManager.GetEntities<Unit>()
                        .Where(x => x.ClassId == ClassId.CDOTA_BaseNPC && x.Team == Members.MyHero.GetEnemyTeam())
                        .ToList();
            }

            public static void Flush()
            {
                _sleeper = new Sleeper();
            }
        }
    }
}