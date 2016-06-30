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
        public static List<Building> all;

        /// <summary>
        ///     The dire.
        /// </summary>
        public static List<Building> dire;

        /// <summary>
        ///     The loaded.
        /// </summary>
        private static bool loaded;

        /// <summary>
        ///     The radiant.
        /// </summary>
        public static List<Building> radiant;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes static members of the <see cref="Towers" /> class.
        /// </summary>
        static Towers()
        {
            all =
                ObjectManager.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower)
                    .ToList();
            dire = all.Where(x => x.Team == Team.Dire).ToList();
            radiant = all.Where(x => x.Team == Team.Radiant).ToList();
            Events.OnLoad += (sender, args) =>
            {
                if (loaded)
                {
                    return;
                }

                Load();
            };
            if (!loaded && ObjectManager.LocalHero != null && Game.IsInGame)
            {
                Load();
            }

            Events.OnClose += (sender, args) =>
            {
                ObjectManager.OnRemoveEntity -= ObjectMgr_OnRemoveEntity;
                loaded = false;
            };
        }

        #endregion

        #region Methods

        /// <summary>
        ///     The load.
        /// </summary>
        private static void Load()
        {
            all =
                ObjectManager.GetEntities<Building>()
                    .Where(x => x.IsAlive && x.ClassID == ClassID.CDOTA_BaseNPC_Tower)
                    .ToList();
            dire = all.Where(x => x.Team == Team.Dire).ToList();
            radiant = all.Where(x => x.Team == Team.Radiant).ToList();
            ObjectManager.OnRemoveEntity += ObjectMgr_OnRemoveEntity;
            loaded = true;
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

            all.Remove(tower);
            if (tower.Team == Team.Dire)
            {
                dire.Remove(tower);
            }
            else
            {
                radiant.Remove(tower);
            }
        }

        #endregion
    }
}