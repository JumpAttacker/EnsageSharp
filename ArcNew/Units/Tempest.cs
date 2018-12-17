using System;
using System.Collections.Generic;
using System.Linq;
using ArcAnnihilation.Manager;
using ArcAnnihilation.Units.behaviour.Abilities;
using ArcAnnihilation.Units.behaviour.Enabled;
using ArcAnnihilation.Units.behaviour.Items;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using ArcAnnihilation.Units.behaviour.Range;
using Ensage;
using Ensage.Common.Extensions;
using Ensage.Common.Extensions.SharpDX;

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
            DrawRanger = new DrawAttackRange();
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
                if (target.IsVisible)
                {
                    var targetPos = target.NetworkPosition;
                    if (Hero.Distance2D(targetPos) >= Math.Min(MenuManager.OrbWalkingRange, Hero.GetAttackRange()))
                        Hero.Move(targetPos);
                    else if (MenuManager.OrbWalkerGoBeyond)
                    {
                        var pos = (targetPos - Hero.Position).Normalized();
                        pos *= MenuManager.OrbWalkingRange;
                        pos = targetPos - pos;
                        Hero.Move(pos);
                    }
                }
                else
                {
                    Hero.Move(target.InFront(250));
                }

            /*else
                Hero.Move(Game.MousePosition);*/
        }

        public override IEnumerable<Item> GetItems()
        {
            var items = Hero.Inventory.Items.Where(x=>AbilityChecker.IsItemEnabled(x.Id));
            if (MenuManager.CustomComboPriorityTempest)
                items = items.OrderBy(x => MenuManager.GetItemOrderTempest(x.Id));
            return items;
        }
    }
}