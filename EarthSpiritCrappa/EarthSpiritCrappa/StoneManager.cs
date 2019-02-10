using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ensage;
using Ensage.SDK.Extensions;
using Ensage.SDK.Helpers;
using SharpDX;

namespace EarthSpiritCrappa
{
    public class StoneManager
    {
        public StoneManager(EarthSpiritCrappa earthSpiritCrappa)
        {
            Main = earthSpiritCrappa;
            Stones =
                EntityManager<Unit>.Entities.Where(x => x.IsValid && x.NetworkName == ClassId.CDOTA_Unit_Earth_Spirit_Stone.ToString())
                    .ToList();

            EntityManager<Unit>.EntityAdded += (sender, unit) =>
            {
                if (unit.NetworkName == ClassId.CDOTA_Unit_Earth_Spirit_Stone.ToString())
                {
                    Stones.Add(unit);
                }
            };
            EntityManager<Unit>.EntityRemoved += (sender, unit) =>
            {
                if (unit.NetworkName == ClassId.CDOTA_Unit_Earth_Spirit_Stone.ToString())
                {
                    Stones.Remove(unit);
                }
            };
        }

        public EarthSpiritCrappa Main { get; set; }

        public List<Unit> Stones { get; set; }

        public Unit FindStone(Vector3 position, float range = 200f)
        {
            return
                Stones.Where(x => x.Health > 0 && x.IsInRange(position, range))
                    .OrderBy(x => x.Distance2D(position))
                    .FirstOrDefault();
        }
        public bool AnyStoneInRange(Vector3 position, float range = 200f)
        {
            return Stones.Any(x => x.Health > 0 && x.IsInRange(position, range));
        }
    }
}