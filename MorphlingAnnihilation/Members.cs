using System.Collections.Generic;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace MorphlingAnnihilation
{
    public class Members
    {
        public static Dictionary<uint, string> Items;
        public static Dictionary<uint, string> Spells;

        public static Sleeper Updater;

        public static Team MyTeam;
        public static Hero MainHero;
        public static Player MainPlayer;

        public static class Enums
        {
            public enum Behavior
            {
                FollowTarget,
                FollowMouse
            }
        }
    }
}
