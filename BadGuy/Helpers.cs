using System.Linq;
using Ensage;
using Ensage.SDK.Helpers;

namespace BadGuy
{
    internal static class Helpers
    {
        private static Unit _allyFountain;
        private static Unit _enemyFountain;
        public static Unit GetAllyFountain()
        {
            if (_allyFountain != null && _allyFountain.IsValid)
            {
                return _allyFountain;
            }
            _allyFountain =
                EntityManager<Unit>.Entities.FirstOrDefault(
                    x =>
                        x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain &&
                        x.Team == ObjectManager.LocalHero.Team);
            return _allyFountain;
        }
        public static Unit GetEnemyFountain()
        {
            if (_enemyFountain != null && _enemyFountain.IsValid)
            {
                return _enemyFountain;
            }
            _enemyFountain =
                EntityManager<Unit>.Entities.FirstOrDefault(
                    x =>
                        x != null && x.IsValid && x.ClassId == ClassId.CDOTA_Unit_Fountain &&
                        x.Team != ObjectManager.LocalHero.Team);
            return _enemyFountain;
        }
    }
}