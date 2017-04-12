using System.Linq;
using System.Reflection;
using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using Ensage.Common.Objects;
using Ensage.Common.Objects.UtilityObjects;
using log4net;
using PlaySharp.Toolkit.Logging;

namespace MorphlingAnnihilation.Interface
{
    
    public abstract class ExtendedUnit
    {
        private static readonly ILog Log = AssemblyLogs.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Orbwalker _orb;
        public Hero Me { get; }
        public Hero GlobalTarget;
        private readonly MultiSleeper _abilitySleeper;
        public bool IsAlive => Me.IsAlive;
        public bool IsValid => Me.IsValid && Me.IsAlive;
        protected ExtendedUnit(Hero me)
        {
            _abilitySleeper = new MultiSleeper();
            Me = me;
            Log.Debug($"[new]: {me.ClassId}");
        }
        
        public virtual void AttackTarget(Hero target, bool follow = false)
        {
            if (_orb == null)
                _orb = new Orbwalker(Me);
            _orb.OrbwalkOn(target, followTarget: MenuManager.GetComboBehavior == 0 || follow);
        }

        public virtual void UseAbilities(Hero target)
        {
            var spells =
                Me.Spellbook()
                    .Spells.Where(
                        x =>
                            !x.IsAbilityBehavior(AbilityBehavior.Passive) && x.AbilityType != AbilityType.Ultimate &&
                            x.CanBeCasted() && x.CanHit(target) &&
                            !_abilitySleeper.Sleeping(x));
            foreach (var ability in spells)
            {
                if ((ability.AbilityBehavior & AbilityBehavior.Point) != 0)
                {
                    if (ability.IsSkillShot())
                    {
                        if (!ability.CastSkillShot(target))
                            continue;
                    }
                    else
                        ability.UseAbility(target.Position);
                }
                else if ((ability.AbilityBehavior & AbilityBehavior.UnitTarget) != 0)
                {
                    if (ability.TargetTeamType == TargetTeamType.Enemy)
                    {
                        ability.UseAbility(target);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    ability.UseAbility();
                }
                var delay = Me.GetAbilityDelay(target, ability);
                Log.Debug($"Ability: {ability.StoredName()} -> {delay}ms");
                _abilitySleeper.Sleep(delay, ability);
            }
        }
        public virtual void DoCombo(Hero target=null)
        {
            if (target==null)
                if (GlobalTarget == null || !GlobalTarget.IsValid)
                    GlobalTarget = TargetSelector.ClosestToMouse(Me);
            if (Me.IsChanneling() || Me.Spellbook().Spells.Any(x=>x.IsInAbilityPhase))
                return;
            var tempTarget = (GlobalTarget != null && GlobalTarget.IsValid) || target == null ? GlobalTarget : target;
            AttackTarget(tempTarget, target == null);
            UseItems(tempTarget);
            UseAbilities(tempTarget);
        }

        public virtual void FlushGlobalHero()
        {
            GlobalTarget = null;
            Log.Debug("[flush]");
        }
        public virtual void UseItems(Hero target)
        {

        }
    }
}