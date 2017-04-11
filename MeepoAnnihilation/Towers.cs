using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common;

namespace MeepoAnnihilation
{
    public static class Towers
    {
        #region Static Fields

        /// <summary>
        ///     The all.
        /// </summary>
        public static List<Building> All;

        /// <summary>
        ///     The dire.
        /// </summary>
        public static List<Building> Dire;

        /// <summary>
        ///     The loaded.
        /// </summary>
        private static bool _loaded;

        /// <summary>
        ///     The radiant.
        /// </summary>
        public static List<Building> Radiant;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Towers" /> class.
        /// </summary>
        static Towers()
        {
            All =
                ObjectManager.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassId == ClassId.CDOTA_BaseNPC_Tower)
                    .ToList();
            Dire = All.Where(x => x.Team == Team.Dire).ToList();
            Radiant = All.Where(x => x.Team == Team.Radiant).ToList();
            Events.OnLoad += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }

                Load();
            };
            if (!_loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
            }

            Events.OnClose += (sender, args) =>
            {
                ObjectManager.OnRemoveEntity -= ObjectMgr_OnRemoveEntity;
                _loaded = false;
            };
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The load.
        /// </summary>
        private static void Load()
        {
            All =
                ObjectManager.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassId == ClassId.CDOTA_BaseNPC_Tower)
                    .ToList();
            Dire = All.Where(x => x.Team == Team.Dire).ToList();
            Radiant = All.Where(x => x.Team == Team.Radiant).ToList();
            ObjectManager.OnRemoveEntity += ObjectMgr_OnRemoveEntity;
            _loaded = true;
        }

        /// <summary>
        ///     The object manager on remove entity.
        /// </summary>
        /// <param name="args">
        ///     The args.
        /// </param>
        private static void ObjectMgr_OnRemoveEntity(EntityEventArgs args)
        {
            var tower = args.Entity as Building;
            if (tower == null)
            {
                return;
            }

            All.Remove(tower);
            if (tower.Team == Team.Dire)
            {
                Dire.Remove(tower);
            }
            else
            {
                Radiant.Remove(tower);
            }
        }

        #endregion
    }
}