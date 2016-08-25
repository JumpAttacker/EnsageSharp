using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;

namespace OverlayInformation
{
    public class HeroesList
    {
        #region Static Fields

        /// <summary>
        ///     The all.
        /// </summary>
        public static List<Hero> All;

        /// <summary>
        ///     The dire.
        /// </summary>
        public static List<Hero> Dire;

        /// <summary>
        ///     The radiant.
        /// </summary>
        public static List<Hero> Radiant;

        /// <summary>
        ///     The loaded.
        /// </summary>
        private static bool loaded;

        /// <summary>
        ///     The temp list.
        /// </summary>
        private static List<Hero> tempList;

        private static Sleeper Sleeper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Heroes" /> class.
        /// </summary>
        static HeroesList()
        {
            All = new List<Hero>();
            Dire = new List<Hero>();
            Radiant = new List<Hero>();
            Events.OnLoad += (sender, args) =>
            {
                if (loaded)
                {
                    return;
                }
                Load();
                loaded = true;
            };
            if (!loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
                loaded = true;
            }

            Events.OnClose += (sender, args) =>
            {
                All = new List<Hero>();
                Dire = new List<Hero>();
                Radiant = new List<Hero>();
                tempList = new List<Hero>();
                Events.OnUpdate -= Update;
                ObjectManager.OnAddEntity -= ObjectMgr_OnAddEntity;
                ObjectManager.OnRemoveEntity -= ObjectMgr_OnRemoveEntity;
                loaded = false;
            };
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get by team.
        /// </summary>
        /// <param name="team">
        ///     The team.
        /// </param>
        /// <returns>
        ///     The <see cref="List" />.
        /// </returns>
        public static List<Hero> GetByTeam(Team team)
        {
            return team == Team.Radiant ? Radiant : Dire;
        }

        /// <summary>
        ///     The update.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        public static void Update(EventArgs args)
        {
            if (!Game.IsInGame)
            {
                return;
            }
            //Printer.Print($"update#1 {!Utils.SleepCheck("Common.Heroes.Update2")} { All.Count(x => x.IsValid) >= 10}");
            if (Sleeper.Sleeping || All.Count(x => x.IsValid) >= 10)
            {
                return;
            }
            //Printer.Print("update#2");
            UpdateHeroes();
            Sleeper.Sleep(1000);
        }

        /// <summary>
        ///     The update heroes.
        /// </summary>
        public static void UpdateHeroes()
        {
            var herolist = new List<Hero>(All);
            var herolistRadiant = new List<Hero>(Radiant);
            var herolistDire = new List<Hero>(Dire);
            //Printer.Print("update#3");
            foreach (var hero in tempList)
            {
                if (!(hero != null && hero.IsValid))
                {
                    continue;
                }

                if (!All.Contains(hero))
                {
                    herolist.Add(hero);
                    //Printer.Print($"herolist.Add(hero): {hero.Name}");
                }

                if (!Radiant.Contains(hero) && hero.Team == Team.Radiant)
                {
                    herolistRadiant.Add(hero);
                    //Printer.Print($"herolist.Add(hero): {hero.Name}");
                }

                if (!Dire.Contains(hero) && hero.Team == Team.Dire)
                {
                    herolistDire.Add(hero);
                    //Printer.Print($"herolist.Add(hero): {hero.Name}");
                }
            }
            //Printer.Print("update#4");
            All = herolist;
            Radiant = herolistRadiant;
            Dire = herolistDire;

            //Printer.Print($"All: {All.Count}; Radiant: {Radiant.Count}; Dire: {Dire.Count};",type:MessageType.LogMessage);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The load.
        /// </summary>
        private static void Load()
        {
            All = new List<Hero> { ObjectManager.LocalHero };
            Dire = new List<Hero>();
            Radiant = new List<Hero>();
            if (ObjectManager.LocalHero.Team == Team.Dire)
            {
                Dire.Add(ObjectManager.LocalHero);
            }
            else
            {
                Radiant.Add(ObjectManager.LocalHero);
            }

            tempList = Players.All.Where(x => x.Hero != null && x.Hero.IsValid).Select(x => x.Hero).ToList();
            foreach (
                var hero in ObjectManager.GetEntities<Hero>().Where(hero => tempList.All(x => x.Handle != hero.Handle)))
            {
                //Printer.Print($"tempList.Add(hero): {hero.Name}");
                tempList.Add(hero);
            }

            UpdateHeroes();
            Sleeper = new Sleeper();
            Events.OnUpdate += Update;
            ObjectManager.OnAddEntity += ObjectMgr_OnAddEntity;
            ObjectManager.OnRemoveEntity += ObjectMgr_OnRemoveEntity;
        }

        /// <summary>
        ///     The object mgr_ on add entity.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void ObjectMgr_OnAddEntity(EntityEventArgs args)
        {
            DelayAction.Add(
                200,
                () =>
                {
                    var hero = args.Entity as Hero;
                    if (hero == null)
                    {
                        return;
                    }

                    tempList.Add(hero);
                    if (!All.Contains(hero))
                    {
                        //Printer.Print($"All.Add(hero): {hero.Name}");
                        All.Add(hero);
                    }

                    if (!Radiant.Contains(hero) && hero.Team == Team.Radiant)
                    {
                        //Printer.Print($"Radiant.Add(hero): {hero.Name}");
                        Radiant.Add(hero);
                        return;
                    }

                    if (!Dire.Contains(hero) && hero.Team == Team.Dire)
                    {
                        //Printer.Print($"Dire.Add(hero): {hero.Name}");
                        Dire.Add(hero);
                    }
                });
        }

        /// <summary>
        ///     The object mgr_ on remove entity.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void ObjectMgr_OnRemoveEntity(EntityEventArgs args)
        {
            var hero = args.Entity as Hero;
            if (hero == null)
            {
                return;
            }

            tempList.Remove(hero);
            if (All.Contains(hero))
            {
                All.Remove(hero);
                //Printer.Print($"All.Remove(hero): {hero.Name}");
            }

            if (Radiant.Contains(hero) && hero.Team == Team.Radiant)
            {
                //Printer.Print($"Radiant.Remove(hero): {hero.Name}");
                Radiant.Remove(hero);
                return;
            }

            if (Dire.Contains(hero) && hero.Team == Team.Dire)
            {
                //Printer.Print($"Dire.Remove(hero): {hero.Name}");
                Dire.Remove(hero);
            }
        }

        #endregion
    }
}