using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Units.behaviour.Abilities;
using ArcAnnihilation.Units.behaviour.Enabled;
using ArcAnnihilation.Units.behaviour.Items;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using ArcAnnihilation.Units.behaviour.Range;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.SharpDX;

namespace ArcAnnihilation.Units
{
    public class MainHero : UnitBase
    {
        public MainHero()
        {
            Hero = ObjectManager.LocalHero;
            AbilityChecker = new MainHeroAbilityChecker();
            AbilitiesBehaviour = new CanUseAbilities();
            ItemsBehaviour = new CanUseItems();
            OrbwalkingBehaviour = new CanUseOrbwalking();
            DrawRanger = new DrawAttackRange();
        }

        public override void InitAbilities()
        {
            Flux = Hero.GetAbilityById(AbilityId.arc_warden_flux);
            MagneticField = Hero.GetAbilityById(AbilityId.arc_warden_magnetic_field);
            Spark = Hero.GetAbilityById(AbilityId.arc_warden_spark_wraith);
            TempestDouble = Hero.GetAbilityById(AbilityId.arc_warden_tempest_double);
        }

        public override void MoveAction(Unit target)
        {
            var time = Game.RawGameTime;
            if (time - LastMoveOrderIssuedTime < CooldownOnMoving)
            {
                return;
            }
            LastMoveOrderIssuedTime = Game.RawGameTime;
            if (MenuManager.OrbWalkType && target != null)
            {
                var targetPos = target.NetworkPosition;
                if (Hero.Distance2D(targetPos) >= Math.Min(MenuManager.OrbWalkingRange, Hero.GetAttackRange()))
                {
                    Hero.Move(targetPos);
                }
                else
                {
                    var pos = (targetPos - Hero.Position).Normalized();
                    pos *= MenuManager.OrbWalkingRange;
                    pos = targetPos - pos;
                    Hero.Move(pos);
                }
            }
            else
                Hero.Move(Game.MousePosition);
        }

        public override async Task UseAbilities(CancellationToken cancellationToken)
        {
            if (TempestDouble.CanBeCasted() && AbilityChecker.IsAbilityEnabled(TempestDouble.Id))
            {
                if (TempestManager.Tempest == null || !TempestManager.Tempest.IsValid || !Core.TempestHero.IsAlive)
                {
                    TempestDouble.UseAbility();
                    await Task.Delay(TempestDouble.GetAbilityDelay(), cancellationToken);
                }
            }
            await AbilitiesBehaviour.UseAbilities(this);
        }

        public override IEnumerable<Item> GetItems()
        {
            var items=Hero.Inventory.Items.Where(x=>AbilityChecker.IsItemEnabled(x.Id));
            if (MenuManager.CustomComboPriorityHero)
                items = items.OrderBy(x => MenuManager.GetItemOrderHero(x.Id));
            return items;
        }
    }
}