using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Ensage;
using Ensage.Common;

namespace ArcAnnihilation
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class Objects
    {
        public class Towers
        {
            private static List<Unit> _towerList;

            public static List<Unit> GetTowers()
            {
                if (!Utils.SleepCheck("Towers.refresh")) return _towerList;
                _towerList = ObjectMgr.GetEntities<Unit>()
                    .Where(x => x.ClassID == ClassID.CDOTA_BaseNPC_Tower && x.IsValid && x.IsAlive)
                    .ToList();
                if (_towerList.Any())
                    Utils.Sleep(1000, "Towers.refresh");
                return _towerList;
            }
        }

        public class Tempest
        {
            private static IEnumerable<Hero> _clones;

            public static IEnumerable<Hero> GetCloneList(Hero me)
            {
                if (!Utils.SleepCheck("Tempest.refresh")) return _clones;
                _clones = ObjectMgr.GetEntities<Hero>()
                    .Where(
                        x =>
                            x.IsAlive && x.IsControllable && x.Team == me.Team &&
                            x.Modifiers.Any(y => y.Name == "modifier_kill")).ToList();
                if (_clones.Any())
                    Utils.Sleep(100, "Tempest.refresh");
                return _clones;
            } 
        }
        public class Necronomicon
        {
            private static IEnumerable<Unit> _necronomicon;

            public static IEnumerable<Unit> GetNecronomicons(Hero me)
            {
                if (!Utils.SleepCheck("Necronomicon.refresh")) return _necronomicon;
                _necronomicon =
                    ObjectMgr.GetEntities<Unit>()
                        .Where(x => x.IsAlive && x.IsControllable && x.Team == me.Team && x.IsSummoned)
                        .ToList();
                if (_necronomicon.Any())
                    Utils.Sleep(100, "Necronomicon.refresh");
                return _necronomicon;
            }
        }
        public class LaneCreeps
        {
            private static IEnumerable<Unit> _laneCreepsList;

            public static IEnumerable<Unit> GetCreeps()
            {
                if (!Utils.SleepCheck("LaneCreeps.refresh")) return _laneCreepsList;
                _laneCreepsList =
                    ObjectMgr.GetEntities<Unit>()
                        .Where(x => x.ClassID == ClassID.CDOTA_BaseNPC_Creep_Lane && x.IsValid && x.IsAlive)
                        .ToList();
                if (_laneCreepsList.Any())
                    Utils.Sleep(100, "LaneCreeps.refresh");
                return _laneCreepsList;
            }
        }
        public class Fountains
        {
            private static Unit _ally;
            private static Unit _enemy;
            public static Unit GetAllyFountain()
            {
                if (_ally == null || !_ally.IsValid)
                {
                    _ally = ObjectMgr.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team == ObjectMgr.LocalHero.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                }
                return _ally;
            }
            public static Unit GetEnemyFountain()
            {
                if (_enemy == null || !_enemy.IsValid)
                {
                    _enemy = ObjectMgr.GetEntities<Unit>()
                        .FirstOrDefault(x => x.Team != ObjectMgr.LocalHero.Team && x.ClassID == ClassID.CDOTA_Unit_Fountain);
                }
                return _enemy;
            }
        }
    }
}