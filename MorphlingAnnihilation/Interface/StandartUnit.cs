using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace MorphlingAnnihilation.Interface
{
    public abstract class StandartUnit
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Orbwalker _orb;
        public Hero Me { get; }
        public Hero GlobalTarget;
        public bool IsAlive => Me.IsAlive;
        public bool IsValid => Me.IsValid && Me.IsAlive;
        protected StandartUnit(Hero me)
        {
            Me = me;
            Log.Debug($"[new]: {me.ClassId}");
        }
        public virtual void AttackTarget(Hero target=null, bool follow = false)
        {
            if (target == null)
                if (GlobalTarget == null || !GlobalTarget.IsValid)
                    GlobalTarget = TargetSelector.ClosestToMouse(Me);
            if (_orb == null)
                _orb = new Orbwalker(Me);
            var tempTarget = (GlobalTarget != null && GlobalTarget.IsValid) || target == null ? GlobalTarget : target;
            _orb.OrbwalkOn(tempTarget, followTarget: MenuManager.GetComboBehavior == 0 || target==null);
        }
        public virtual void FlushGlobalHero()
        {
            GlobalTarget = null;
            Log.Debug("[flush]");
        }
    }
}