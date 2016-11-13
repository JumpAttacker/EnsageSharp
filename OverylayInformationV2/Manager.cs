using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Objects;

namespace OverlayInformation
{
    internal static class Manager
    {
        internal static class HeroManager
        {
            public static List<Hero> GetHeroes()
            {
                return Members.Heroes;
            }

            public static List<Hero> GetViableHeroes()
            {
                try
                {
                    return Members.Heroes.Where(x => x.IsVisible && x.IsAlive).ToList();
                }
                catch (Exception e)
                {
                    Printer.Print("[GetViableHeroes]: " + e);
                    return new List<Hero>();
                }
                
            }

            public static List<Hero> GetAllyViableHeroes()
            {
                try
                {
                    return Members.AllyHeroes.Where(x => x.IsVisible && x.IsAlive).ToList();
                }
                catch (Exception e)
                {
                    Printer.Print("[GetAllyViableHeroes]: " + e);
                    return new List<Hero>();
                }
            }

            public static List<Hero> GetEnemyViableHeroes()
            {
                try
                {
                    return Members.EnemyHeroes.Where(x => x.IsVisible && x.IsAlive).ToList();
                }
                catch (Exception e)
                {
                    Printer.Print("[GetEnemyViableHeroes]: " + e);
                    return new List<Hero>();
                }
            }

            public static List<Item> GetItemList(Hero h)
            {
                List<Item> list;
                return Members.ItemDictionary.TryGetValue(h.Handle, out list) ? list : new List<Item>();
            }

            public static List<Ability> GetAbilityList(Hero h)
            {
                List<Ability> list;
                return Members.AbilityDictionary.TryGetValue(h.Handle, out list) ? list : new List<Ability>();
            }
        }
        internal static class PlayerManager
        {
            public static List<Player> GetPlayer()
            {
                return Members.Players;
            }
            public static List<Player> GetViablePlayers()
            {
                return Members.Players.Where(x => x.Hero.IsVisible && x.Hero.IsAlive).ToList();
            }
            public static List<Player> GetAllyViablePlayers()
            {
                return Members.AllyPlayers.Where(x => x.Hero.IsVisible && x.Hero.IsAlive).ToList();
            }
            public static List<Player> GetEnemyViablePlayers()
            {
                return Members.EnemyPlayers.Where(x => x.Hero.IsVisible && x.Hero.IsAlive).ToList();
            }
        }
        internal static class BaseManager
        {
            public static List<Unit> GetBaseList()
            {
                return Members.BaseList;
            }
            public static List<Courier> GetCouriersList()
            {
                return Members.CourList;
            }
            public static List<Courier> GetViableCouriersList()
            {
                try
                {
                    return Members.CourList.Where(x => x != null && x.IsValid && x.IsAlive && x.IsVisible).ToList();
                }
                catch (Exception)
                {
                    return new List<Courier>();
                }
            }
        }
    }
}