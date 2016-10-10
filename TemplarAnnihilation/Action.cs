using System;
using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using SharpDX;
using static TemplarAnnihilation.Members;

namespace TemplarAnnihilation
{
    internal class Action
    {
        private static bool IsEnable => Menu.Item("Enable").GetValue<bool>();
        private static bool IsRangeEnable => Menu.Item("Range.Enable").GetValue<bool>();

        public static Hero GlobalTarget;
        private static MultiSleeper _spellSleeper;
        private static EfeectMaster _rangeEfeectMaster;

        private static List<Hero> _enemyHeroes;
        private static IEnumerable<Creep> _enemyCreeps;
        private static List<Hero> _enemyPossibleHeroes;

        public static void Game_OnUpdate(EventArgs args)
        {
            if (!IsEnable)
                return;

            if (_rangeEfeectMaster == null)
                _rangeEfeectMaster = new EfeectMaster();

            if (_spellSleeper == null)
                _spellSleeper = new MultiSleeper();

            if (!MyHero.IsAlive)
                return;

            if (IsRangeEnable)
            {
                DrawPsiBladeStuff();
            }
        }

        public static void OnDrawing(EventArgs args)
        {
            if (!IsEnable)
                return;

            if (!MyHero.IsAlive)
                return;

            if (Menu.Item("Dev.Drawing.enable").GetValue<bool>())
            {
                DrawDebugger();
            }
        }

        private static void DrawDebugger()
        {
            /*if (enemyHeroes == null)
                return;
            if (enemyCreeps == null)
                return;
            if (enemyPossibleHeroes == null)
                return;*/
            var startPos = new Vector2(100, 100);
            var size = new Vector2(160, 60);
            Drawing.DrawRect(startPos, size, new Color(100, 100, 100, 100));
            var s1 = $"enemyHeroes: {_enemyHeroes?.Count}";
            var s2 = $"enemyCreeps: {_enemyCreeps?.Count()}";
            var s3 = $"enemyPossibleHeroes: {_enemyPossibleHeroes?.Count}";
            var textPos = startPos + new Vector2(5, 5);
            Drawing.DrawText(
                s1,
                textPos,
                new Vector2(15, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawText(
                s2,
                textPos+new Vector2(0,20),
                new Vector2(15, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
            Drawing.DrawText(
                s3,
                textPos + new Vector2(0, 40),
                new Vector2(15, 0),
                Color.White,
                FontFlags.AntiAlias | FontFlags.StrikeOut);
        }

        private static void DrawPsiBladeStuff()
        {
            var psiBlade = Abilities.FindAbility("templar_assassin_psi_blades");
            if (psiBlade==null || psiBlade.Level==0)
                return;
            _enemyHeroes =
                Heroes.All.Where(
                    x => x.Team != MyTeam && x.IsAlive && x.IsVisible && x.Distance2D(MyHero) <= MyHero.GetAttackRange() + x.HullRadius)
                    .ToList();
            _enemyCreeps =
                ObjectManager.GetEntitiesFast<Creep>()
                    .Where(
                        x =>
                            x.IsAlive && x.IsVisible && (x.Team != MyTeam || (float)x.Health / (float)x.MaximumHealth < 0.50) &&
                            x.Distance2D(MyHero) <= MyHero.GetAttackRange() + x.HullRadius);
            
            var extraRange = 550 + 40 * psiBlade.Level;
            _enemyPossibleHeroes =
                Heroes.All.Where(
                    x =>
                        x.Team != MyTeam && x.IsAlive && x.IsVisible &&
                        x.Distance2D(MyHero) <= MyHero.GetAttackRange() + x.HullRadius + extraRange)
                    .ToList();
            var myPos = MyHero.Position;
            foreach (var hero in _enemyHeroes)
            {
                var heroPos = hero.Position;
                var angle = MyHero.FindAngleBetween(heroPos, true);
                var point = new Vector3(
                    (float)
                        (heroPos.X +
                         extraRange *
                         Math.Cos(angle)),
                    (float)
                        (heroPos.Y +
                         extraRange *
                         Math.Sin(angle)),
                    heroPos.Z);
                var someOne = false;
                foreach (var possibleHero in _enemyPossibleHeroes)
                {
                    if (hero.Equals(possibleHero))
                        continue;
                    var posHeroPos = possibleHero.Position;
                    var pointer = new Point((int)posHeroPos.X, (int)posHeroPos.Y);
                    var masPoints = Helper.GetNeededPoinits(heroPos, point, 75);

                    var isIn = Helper.IsPointInsidePolygon(masPoints, pointer.X, pointer.Y);

                    //Printer.Print($"{possibleHero.GetRealName()}: {isIn}");
                    if (!someOne && isIn == 1)
                        someOne = true;
                }
                _rangeEfeectMaster.DrawEffect(hero, "materials/ensage_ui/particles/rectangle.vpcf", someOne);
            }
            foreach (var hero in _enemyCreeps)
            {
                var heroPos = hero.Position;
                var angle = MyHero.FindAngleBetween(heroPos, true);
                var point = new Vector3(
                    (float)
                        (heroPos.X +
                         extraRange *
                         Math.Cos(angle)),
                    (float)
                        (heroPos.Y +
                         extraRange *
                         Math.Sin(angle)),
                    heroPos.Z);
                var someOne = false;
                foreach (var possibleHero in _enemyPossibleHeroes)
                {

                    var posHeroPos = possibleHero.Position;
                    var pointer = new Point((int)posHeroPos.X, (int)posHeroPos.Y);
                    var masPoints = Helper.GetNeededPoinits(heroPos, point, 75);

                    var isIn = Helper.IsPointInsidePolygon(masPoints, pointer.X, pointer.Y);

                    //Printer.Print($"{possibleHero.GetRealName()}: {isIn}");
                    if (!someOne && isIn == 1)
                        someOne = true;
                }
                _rangeEfeectMaster.DrawEffect(hero, "materials/ensage_ui/particles/rectangle.vpcf", someOne);
            }

        }
    }
}