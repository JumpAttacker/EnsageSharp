using System.Collections.Generic;
using System.Linq;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Units.behaviour.Abilities;
using ArcAnnihilation.Units.behaviour.Enabled;
using ArcAnnihilation.Units.behaviour.Items;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using Ensage;
using Ensage.Common.Extensions;

namespace ArcAnnihilation.Units
{
    public class Tempest : UnitBase
    {
        public Tempest()
        {
            Hero = TempestManager.Tempest;
            AbilityChecker = new TempestAbilityChecker();
            AbilitiesBehaviour = new CanUseAbilities();
            ItemsBehaviour = new CanUseItems();
            OrbwalkingBehaviour = new CanUseOrbwalking();
        }
        public bool IsValid => Hero != null && Hero.IsValid;

        public override void InitAbilities()
        {
            Flux = Hero.GetAbilityById(AbilityId.arc_warden_flux);
            MagneticField = Hero.GetAbilityById(AbilityId.arc_warden_magnetic_field);
            Spark = Hero.GetAbilityById(AbilityId.arc_warden_spark_wraith);
        }

        public override void MoveAction(Unit target)
        {
            var time = Game.RawGameTime;
            if (time - LastMoveOrderIssuedTime < CooldownOnMoving)
            {
                return;
            }

            LastMoveOrderIssuedTime = Game.RawGameTime;
            if (target != null)
                Hero.Move(target.Position);
            /*else
                Hero.Move(Game.MousePosition);*/
        }

        public override IEnumerable<Item> GetItems()
        {
            var items = Hero.Inventory.Items.Where(x=>AbilityChecker.IsItemEnabled(x.GetItemId()));
            if (MenuManager.CustomComboPriorityTempest)
                items = items.OrderBy(x => MenuManager.GetItemOrderTempest(x.GetItemId()));
            return items;
        }
    }
}