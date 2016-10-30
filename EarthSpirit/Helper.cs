using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Menu;
using Ensage.Common.Objects;
using SharpDX;

namespace EarthAn
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

        public static Unit FindRemnant(Vector3 pos = default(Vector3),float range=200)
        {
            if (pos.IsZero)
                pos = Members.MyHero.NetworkPosition;
            var remnant = ObjectManager.GetEntities<Unit>()
                .Where(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == Members.MyTeam &&
                        pos.Distance2D(x.NetworkPosition) <= range && x.IsAlive).OrderBy(y => pos.Distance2D(y.NetworkPosition));
            return remnant.FirstOrDefault();
        }
        public static Unit FindRemnantWithModifier(Vector3 pos = default(Vector3),string mod="")
        {
            if (pos.IsZero)
                pos = Members.MyHero.NetworkPosition;
            var remnant = ObjectManager.GetEntities<Unit>()
                .Where(
                    x =>
                        x.ClassID == ClassID.CDOTA_Unit_Earth_Spirit_Stone && x.Team == Members.MyTeam &&
                        /*pos.Distance2D(x.NetworkPosition) <= 200 &&*/ x.IsAlive && x.HasModifier(mod)).OrderBy(y => pos.Distance2D(y.NetworkPosition));
            return remnant.FirstOrDefault();
        }

        public static Vector3 InFront(this Unit unit, float distance)
        {
            var v = unit.NetworkPosition + unit.Vector3FromPolarAngle() * distance;
            return new Vector3(v.X, v.Y, unit.Position.Z);
        }
    }
}