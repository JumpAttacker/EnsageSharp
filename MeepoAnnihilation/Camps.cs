using System.Collections.Generic;
using System.Linq;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using SharpDX;

namespace MeepoAnnihilation
{
    /// <summary>
    /// </summary>
    /*public class JungleCamp
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="JungleCamp" /> class.
        /// </summary>
        /// <param name="campPosition">
        ///     The camp position.
        /// </param>
        /// <param name="stackPosition">
        ///     The stack position.
        /// </param>
        /// <param name="waitPosition">
        ///     The wait position.
        /// </param>
        /// <param name="team">
        ///     The team.
        /// </param>
        /// <param name="id">
        ///     The id.
        /// </param>
        /// <param name="stackTime">
        ///     The stack time.
        /// </param>
        /// <param name="ancients">
        ///     The ancients.
        /// </param>
        /// <param name="name">
        ///     The name.
        /// </param>
        public JungleCamp(
            Vector3 campPosition,
            Vector3 stackPosition,
            Vector3 waitPosition,
            Team team,
            uint id,
            double stackTime,
            bool ancients,
            string name)
        {
            CampPosition = campPosition;
            StackPosition = stackPosition;
            WaitPosition = waitPosition;
            Team = team;
            Id = id;
            StackTime = stackTime;
            Ancients = ancients;
            Name = name;
        }

        public JungleCamp()
        {
            
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether it is ancient camp.
        /// </summary>
        public bool Ancients { get; set; }

        /// <summary>
        ///     Gets or sets the camp position.
        /// </summary>
        public Vector3 CampPosition { get; set; }

        /// <summary>
        ///     Gets or sets the camp id.
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the stack position.
        /// </summary>
        public Vector3 StackPosition { get; set; }

        /// <summary>
        ///     Gets or sets the stack time (time when creeps should be pulled out of the camp.
        /// </summary>
        public double StackTime { get; set; }

        /// <summary>
        ///     Gets or sets the camp team.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        ///     Gets or sets the wait position (position where you have vision over creeps but does not block it from spawning if
        ///     possible).
        /// </summary>
        public Vector3 WaitPosition { get; set; }

        public bool CanBeHere { get; set; }

        public bool Delayed { get; set; }

        public Hero Stacking { get; set; }

        #endregion
    }*/

    public class CustomJumgleCamp
    {
        public bool CanBeHere { get; set; }
        public bool Delayed { get; set; }
        public Hero Stacking { get; set; }
        public bool Ancients { get; set; }
        public Vector3 CampPosition { get; set; }
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 StackPosition { get; set; }
        public double StackTime { get; set; }
        public Team Team { get; set; }
        public Vector3 WaitPosition { get; set; }
        public CustomJumgleCamp(JungleCamp camp)
        {
            CanBeHere = true;
            Delayed = false;
            Stacking = null;
            Ancients = camp.Ancients;
            CampPosition = camp.CampPosition;
            Id = camp.Id;
            Name = camp.Name;
            StackPosition = camp.StackPosition;
            StackTime = camp.StackTime;
            Team = camp.Team;
            WaitPosition = camp.WaitPosition;
        }
    }
    
    public static class JungleCamps
    {
        #region Public Properties

        /// <summary>
        ///     Gets the get camps.
        /// </summary>
        public static List<CustomJumgleCamp> GetCamps => _camps;

        private static List<CustomJumgleCamp> _camps;
        public static void Init()
        {
            _camps = new List<CustomJumgleCamp>();
            var test = Ensage.Common.Objects.JungleCamps.GetCamps;
            foreach (var jungleCamp in test)
            {
                _camps.Add(new CustomJumgleCamp(jungleCamp));
            }
        }
        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The find closest camp for stacking.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="useTeamCheck"></param>
        /// <param name="ancients">Search with Ancients</param>
        /// <returns>
        ///     The <see cref="JungleCamp" />.
        /// </returns>
        public static CustomJumgleCamp FindClosestCampForStacking(Unit me, bool useTeamCheck, bool ancients = false)
        {
            var pos = me.Position;
            var bestResult =
                (useTeamCheck
                    ? _camps.Where(x => x.CanBeHere && (ancients || !x.Ancients) && Equals(x.Stacking, me))
                        .OrderBy(x => pos.Distance2D(x.WaitPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()
                    : _camps.Where(x => x.CanBeHere && x.Team == me.Team && (ancients || !x.Ancients) && Equals(x.Stacking, me))
                        .OrderBy(x => pos.Distance2D(x.WaitPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()) ?? (useTeamCheck
                            ? _camps.Where(x => x.CanBeHere && (ancients || !x.Ancients) && x.Stacking==null)
                                .OrderBy(x => pos.Distance2D(x.WaitPosition))
                                .DefaultIfEmpty(null)
                                .FirstOrDefault()
                            : _camps.Where(x => x.CanBeHere && x.Team == me.Team && (ancients || !x.Ancients) && x.Stacking == null)
                                .OrderBy(x => pos.Distance2D(x.WaitPosition))
                                .DefaultIfEmpty(null)
                                .FirstOrDefault());
            return bestResult;
        }
        /// <summary>
        ///     The find closest camp.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="useTeamCheck"></param>
        /// <param name="ancients">Search with Ancients</param>
        /// <returns>
        ///     The <see cref="JungleCamp" />.
        /// </returns>
        public static CustomJumgleCamp FindClosestCamp(Unit me, bool useTeamCheck, bool ancients = false)
        {
            var pos = me.Position;
            var bestResult =
                useTeamCheck
                    ? _camps.Where(x => x.CanBeHere && (ancients || !x.Ancients))
                        .OrderBy(x => pos.Distance2D(x.CampPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()
                    : _camps.Where(x => x.CanBeHere && x.Team == me.Team && (ancients || !x.Ancients))
                        .OrderBy(x => pos.Distance2D(x.CampPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();

            return bestResult;
        }

        #endregion
    }
}