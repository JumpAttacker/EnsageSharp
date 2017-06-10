using System.Linq;
using Ensage;

namespace MeepoAnnihilation
{
    public class Fountains
    {
        private static Unit _ally;
        private static Unit _enemy;
        public static Unit GetAllyFountain()
        {
            if (_ally == null || !_ally.IsValid)
            {
                _ally = ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team == ObjectManager.LocalHero.Team && x.ClassId == ClassId.CDOTA_Unit_Fountain);
            }
            return _ally;
        }
        public static Unit GetEnemyFountain()
        {
            if (_enemy == null || !_enemy.IsValid)
            {
                _enemy = ObjectManager.GetEntities<Unit>()
                    .FirstOrDefault(x => x.Team != ObjectManager.LocalHero.Team && x.ClassId == ClassId.CDOTA_Unit_Fountain);
            }
            return _enemy;
        }
    }
}