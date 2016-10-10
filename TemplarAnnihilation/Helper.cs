using System;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.SharpDX;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace TemplarAnnihilation
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

        public static int IsPointInsidePolygon(Point[] p, int x, int y)
        {
            int n;
            var flag = 0;
            var maxCount = p.Length;
            for (n = 0; n < maxCount; n++)
            {
                flag = 0;
                var i1 = n < maxCount - 1 ? n + 1 : 0;
                while (flag == 0)
                {
                    var i2 = i1 + 1;
                    if (i2 >= maxCount)
                        i2 = 0;
                    if (i2 == (n < maxCount - 1 ? n + 1 : 0))
                        break;
                    var s = Math.Abs(p[i1].X * (p[i2].Y - p[n].Y) +
                                     p[i2].X * (p[n].Y - p[i1].Y) +
                                     p[n].X * (p[i1].Y - p[i2].Y));
                    var s1 = Math.Abs(p[i1].X * (p[i2].Y - y) +
                                      p[i2].X * (y - p[i1].Y) +
                                      x * (p[i1].Y - p[i2].Y));
                    var s2 = Math.Abs(p[n].X * (p[i2].Y - y) +
                                      p[i2].X * (y - p[n].Y) +
                                      x * (p[n].Y - p[i2].Y));
                    var s3 = Math.Abs(p[i1].X * (p[n].Y - y) +
                                      p[n].X * (y - p[i1].Y) +
                                      x * (p[i1].Y - p[n].Y));
                    if (s == s1 + s2 + s3)
                    {
                        flag = 1;
                        break;
                    }
                    i1 = i1 + 1;
                    if (i1 >= maxCount)
                        i1 = 0;
                }
                if (flag == 0)
                    break;
            }
            return flag;
        }

        public static Point[] GetNeededPoinits(
            Vector3 startPosition,
            Vector3 endPosition,
            float startWidth,
            float endWidth = 0)
        {
            if (endWidth <= 0)
            {
                endWidth = startWidth;
            }

            endPosition = startPosition.Extend(endPosition, startPosition.Distance2D(endPosition) + endWidth / 2);

            var difference = startPosition - endPosition;
            var rotation = difference.Rotated(MathUtil.DegreesToRadians(90));
            rotation.Normalize();

            var start = rotation * startWidth;
            var end = rotation * endWidth;

            var rightStartPosition = startPosition + start;
            var leftStartPosition = startPosition - start;
            var rightEndPosition = endPosition + end;
            var leftEndPosition = endPosition - end;

            /*Vector2 leftStart, rightStart, leftEnd, rightEnd;
            Drawing.WorldToScreen(leftStartPosition, out leftStart);
            Drawing.WorldToScreen(rightStartPosition, out rightStart);
            Drawing.WorldToScreen(leftEndPosition, out leftEnd);
            Drawing.WorldToScreen(rightEndPosition, out rightEnd);

            Drawing.DrawLine(leftStart, rightStart, Color.Orange);
            Drawing.DrawLine(rightStart, rightEnd, Color.Orange);
            Drawing.DrawLine(rightEnd, leftEnd, Color.Orange);
            Drawing.DrawLine(leftEnd, leftStart, Color.Orange);*/


            var p1 = new Point((int) rightStartPosition.X, (int) rightStartPosition.Y);
            var p2 = new Point((int) leftStartPosition.X, (int) leftStartPosition.Y);
            var p3 = new Point((int) rightEndPosition.X, (int) rightEndPosition.Y);
            var p4 = new Point((int) leftEndPosition.X, (int) leftEndPosition.Y);
            return new[] {p1, p2, p4, p3};
        }

        public static bool IsItemEnable(string name)
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

        public static float GetRealCastRange(this Ability ability)
        {
            var castRange = ability.GetCastRange();

            if (!ability.IsAbilityBehavior(AbilityBehavior.NoTarget))
            {
                castRange += Math.Max(castRange / 9, 80);
            }
            else
            {
                castRange += Math.Max(castRange / 7, 40);
            }

            return castRange;
        }

        public static float GetRealCastRange2(this Ability ability)
        {
            var range = ability.CastRange;
            if (range >= 1) return range;
            var data = ability.AbilitySpecialData.FirstOrDefault(x => x.Name.Contains("radius") || (x.Name.Contains("range") && !x.Name.Contains("ranged")));
            if (data == null) return range;
            var level = ability.Level == 0 ? 0 : ability.Level - 1;
            range = (uint)(data.Count > 1 ? data.GetValue(level) : data.Value);
            return range;
        }
    }
}