using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Ensage;
using Ensage.Common.Extensions;
using SharpDX;

namespace MeepoAnnihilation
{
    /// <summary>
    /// </summary>
    public class JungleCamp
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
        /// <param name="canBeHere"></param>
        /// <param name="delayed"></param>
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
            this.CampPosition = campPosition;
            this.StackPosition = stackPosition;
            this.WaitPosition = waitPosition;
            this.Team = team;
            this.Id = id;
            this.StackTime = stackTime;
            this.Ancients = ancients;
            this.Name = name;
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

        public bool canBeHere { get; set; }

        public bool delayed { get; set; }

        public Hero stacking { get; set; }

        #endregion
    }

    /// <summary>
    /// </summary>
    public static class JungleCamps
    {
        #region Static Fields

        /// <summary>
        ///     The camps.
        /// </summary>
        private static readonly List<JungleCamp> Camps = new List<JungleCamp>
        {
            new JungleCamp
                {
                    CampPosition = new Vector3(-1655, -4329, 256), 
                    StackPosition = new Vector3(-1833, -3062, 256), 
                    WaitPosition = new Vector3(-1890, -3896, 256), 
                    Id = 1, StackTime = 54.5, Team = Team.Radiant, 
                    Ancients = false, Name = "Hard Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-260, -3234, 256), 
                    StackPosition = new Vector3(-554, -1925, 256), 
                    WaitPosition = new Vector3(-337, -2652, 256), 
                    Id = 2, StackTime = 55, Team = Team.Radiant, 
                    Ancients = false, Name = "Medium Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(1606, -3433, 256), 
                    StackPosition = new Vector3(1598, -5117, 256), 
                    WaitPosition = new Vector3(1541, -4265, 256), 
                    Id = 3, StackTime = 54.5, Team = Team.Radiant, 
                    Ancients = false, Name = "Medium Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(4495, -3488, 384), 
                    StackPosition = new Vector3(3002, -3936, 384), 
                    WaitPosition = new Vector3(4356, -4089, 384), 
                    Id = 4, StackTime = 53.1, Team = Team.Radiant, 
                    Ancients = false, Name = "Hard Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(3031, -4480, 256), 
                    StackPosition = new Vector3(1555, -5337, 384), 
                    WaitPosition = new Vector3(3099, -5325, 384), 
                    Id = 5, StackTime = 53, Team = Team.Radiant, 
                    Ancients = false, Name = "Easy Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-3097, 4, 384), 
                    StackPosition = new Vector3(-3472, -1566, 384), 
                    WaitPosition = new Vector3(-2471, -227, 384), 
                    Id = 6, StackTime = 54, Team = Team.Radiant, 
                    Ancients = true, Name = "ancients Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-3593, 825, 384), 
                    StackPosition = new Vector3(-3893, -737, 384), 
                    WaitPosition = new Vector3(-4129, 600, 384), 
                    Id = 7, StackTime = 53, Team = Team.Radiant, 
                    Ancients = true, Name = "Secret Hard Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(1266, 3271, 384), 
                    StackPosition = new Vector3(449, 4752, 384), 
                    WaitPosition = new Vector3(979, 3671, 384), 
                    Id = 8, StackTime = 54, Team = Team.Dire, 
                    Ancients = false, Name = "Hard Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-291, 3644, 384), 
                    StackPosition = new Vector3(236, 5000, 256), 
                    WaitPosition = new Vector3(-566, 4151, 384), 
                    Id = 9, StackTime = 54.5, Team = Team.Dire, 
                    Ancients = false, Name = "Medium Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-1640, 2562, 256), 
                    StackPosition = new Vector3(-1180, 4090, 384), 
                    WaitPosition = new Vector3(-1380, 2979, 256), 
                    Id = 10, StackTime = 53, Team = Team.Dire, 
                    Ancients = false, Name = "Medium Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-3084, 4492, 384), 
                    StackPosition = new Vector3(-3533, 6295, 384), 
                    WaitPosition = new Vector3(-3058, 4997, 384), 
                    Id = 11, StackTime = 54, Team = Team.Dire, 
                    Ancients = false, Name = "Easy Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(-4628, 3483, 384), 
                    StackPosition = new Vector3(-2801, 3684, 245), 
                    WaitPosition = new Vector3(-4200, 3850, 256), 
                    Id = 12, StackTime = 53, Team = Team.Dire, 
                    Ancients = false, Name = "Top Hard Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(4150, -678, 256), 
                    StackPosition = new Vector3(2493, -1059, 256), 
                    WaitPosition = new Vector3(3583, -736, 127), 
                    Id = 13, StackTime = 54, Team = Team.Dire, 
                    Ancients = true, Name = "ancients Camp"
                }, 
            new JungleCamp
                {
                    CampPosition = new Vector3(4280, 588, 384), 
                    StackPosition = new Vector3(3537, 1713, 256), 
                    WaitPosition = new Vector3(3710, 548, 384), 
                    Id = 14, StackTime = 54, Team = Team.Dire, 
                    Ancients = false, Name = "Secret Hard Camp"
                }
        };

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets the get camps.
        /// </summary>
        public static List<JungleCamp> GetCamps
        {
            get
            {
                return Camps;
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
        public static JungleCamp FindClosestCampForStacking(Unit me, bool useTeamCheck, bool ancients = false)
        {
            var pos = me.Position;
            var bestResult =
                (useTeamCheck
                    ? Camps.Where(x => x.canBeHere && (ancients || !x.Ancients) && Equals(x.stacking, me))
                        .OrderBy(x => pos.Distance2D(x.WaitPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()
                    : Camps.Where(x => x.canBeHere && x.Team == me.Team && (ancients || !x.Ancients) && Equals(x.stacking, me))
                        .OrderBy(x => pos.Distance2D(x.WaitPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()) ?? (useTeamCheck
                            ? Camps.Where(x => x.canBeHere && (ancients || !x.Ancients) && x.stacking==null)
                                .OrderBy(x => pos.Distance2D(x.WaitPosition))
                                .DefaultIfEmpty(null)
                                .FirstOrDefault()
                            : Camps.Where(x => x.canBeHere && x.Team == me.Team && (ancients || !x.Ancients) && x.stacking == null)
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
        public static JungleCamp FindClosestCamp(Unit me, bool useTeamCheck, bool ancients = false)
        {
            var pos = me.Position;
            var bestResult =
                useTeamCheck
                    ? Camps.Where(x => x.canBeHere && (ancients || !x.Ancients))
                        .OrderBy(x => pos.Distance2D(x.CampPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault()
                    : Camps.Where(x => x.canBeHere && x.Team == me.Team && (ancients || !x.Ancients))
                        .OrderBy(x => pos.Distance2D(x.CampPosition))
                        .DefaultIfEmpty(null)
                        .FirstOrDefault();

            return bestResult;
        }

        #endregion
    }
}