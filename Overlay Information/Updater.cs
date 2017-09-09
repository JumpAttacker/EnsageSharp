using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace OverlayInformation
{
    public class Updater
    {
        private OverlayInformation Main { get; }
        public List<HeroContainer> Heroes { get; }
        public List<HeroContainer> AllyHeroes { get; }
        public List<HeroContainer> EnemyHeroes { get; }
        public List<CourContainer> EnemyCouriers { get; set; }
        public List<CourContainer> AllyCouriers { get; set; }
        public List<CourContainer> Couriers { get; set; }
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Updater(OverlayInformation overlayInformation)
        {
            Main = overlayInformation;
            Heroes = new List<HeroContainer>();
            AllyHeroes = new List<HeroContainer>();
            EnemyHeroes = new List<HeroContainer>();

            Couriers = new List<CourContainer>();
            AllyCouriers = new List<CourContainer>();
            EnemyCouriers = new List<CourContainer>();

            foreach (var entity in ObjectManager.GetDormantEntities<Hero>())
            {
                OnNewHero(null, entity);
            }
            foreach (var entity in ObjectManager.GetEntities<Hero>())
            {
                OnNewHero(null, entity);
            }

            foreach (var entity in ObjectManager.GetDormantEntities<Courier>())
            {
                OnNewCour(null, entity);
            }
            foreach (var entity in ObjectManager.GetEntities<Courier>())
            {
                OnNewCour(null, entity);
            }

            EntityManager<Hero>.EntityAdded += OnNewHero;
            EntityManager<Courier>.EntityAdded += OnNewCour;

            EntityManager<Hero>.EntityRemoved += OnHeroRemoved;

            UpdateManager.Subscribe(() =>
            {
                foreach (var container in Heroes)
                {
                    var hero = container.Hero;
                    if (hero == null)
                    {
                        Log.Error(new string('-', Console.BufferWidth));
                        Log.Error($"---> ({container.Name}) is null | {container.Id}");
                        Log.Error(new string('-', Console.BufferWidth));
                    }
                    else if (!hero.IsValid)
                    {
                        Log.Error(new string('-', Console.BufferWidth));
                        Log.Error($"---> ({container.Name}) is not Valid | {container.Id}");
                        Log.Error(new string('-', Console.BufferWidth));
                    }
                }
            }, 5);
        }

        private void OnHeroRemoved(object sender, Hero e)
        {
            var founder = Heroes.Find(x => x.Hero.Equals(e));
            if (founder != null)
            {
                Heroes.Remove(founder);
                if (founder.IsAlly)
                    AllyHeroes.Remove(founder);
                else
                    EnemyHeroes.Remove(founder);
                Log.Error($"ON REMOVED -> {e.Name}");
            }
        }

        private void OnNewCour(object sender, Courier courier)
        {
            //if (courier.Team==Main.Owner.Team)
            //    return;

            if (Couriers.Any(x=>x.Cour.Equals(courier)))
            {
                Log.Error($"Cant init this Cour -> {courier.GetDisplayName()} [{courier.Handle}]");
                return;
            }
            var myTeam = Main.Context.Value.Owner.Team;
            var targetTeam = courier.Team;
            var isAlly = myTeam == targetTeam;
            var newHero = new CourContainer(courier, isAlly, Main);
            try
            {
                Couriers.Add(newHero);

                if (isAlly)
                {
                    AllyCouriers.Add(newHero);
                }
                else
                {
                    EnemyCouriers.Add(newHero);
                }

                Log.Info($"New courier -> {courier.GetDisplayName()} [{courier.Handle}] [{(isAlly ? "Ally" : "Enemy")}]");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnNewHero(object sender, Hero hero)
        {
            DelayAction.Add(350, () =>
            {
                if (hero == null || !hero.IsValid)
                    return;
                if (hero.IsIllusion)
                    return;
                if (Heroes.Any(x => x.Hero.Equals(hero)))
                {
                    Log.Error($"Cant init New Hero -> {hero.GetDisplayName()} [{hero.Handle}]");
                    return;
                }
                if (hero.ClassId == ClassId.CDOTA_Unit_Hero_MonkeyKing)
                {
                    if (!hero.Spellbook.Spells.Any())
                    {
                        Log.Error($"Monkey king bugisoft (ubishit) -> [{hero.Handle}]");
                        return;
                    }
                }
                var myTeam = Main.Context.Value.Owner.Team;
                var targetTeam = hero.Team;
                var isAlly = myTeam == targetTeam;
                var newHero = new HeroContainer(hero, isAlly, Main);
                try
                {
                    Heroes.Add(newHero);

                    if (isAlly)
                    {
                        AllyHeroes.Add(newHero);
                    }
                    else
                    {
                        EnemyHeroes.Add(newHero);
                    }

                    Log.Info($"New Hero -> {hero.GetDisplayName()} [{hero.Handle}] [{(isAlly ? "Ally" : "Enemy")}]");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        public void OnDeactivate()
        {
            foreach (var container in Heroes)
            {
                container.Flush();
            }
        }
    }
}