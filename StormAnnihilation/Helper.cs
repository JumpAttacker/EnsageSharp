using System.Collections.Generic;
using Ensage;
using Ensage.Common.Objects.UtilityObjects;

namespace StormAnnihilation
{
    public static class Helper
    {
        private static readonly Dictionary<uint, Orbwalker> OrbDict = new Dictionary<uint, Orbwalker>();
        public static void Orbwalk(Hero me, Unit target)
        {
            if (me == null)
            {
                return;
            }
            Orbwalker orb;
            if (!OrbDict.TryGetValue(me.Handle, out orb))
            {
                OrbDict.Add(me.Handle, new Orbwalker(me));
                return;
            }
            orb.OrbwalkOn(target, followTarget: true/*MenuManager.FollowTarget*/);
        }

        public static void Print(string s)
        {
            //Game.PrintMessage(s);
        }
    }
}