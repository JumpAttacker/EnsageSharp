using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArcAnnihilation.Units.behaviour.Abilities;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using ArcAnnihilation.Utils;
using Ensage;
using Ensage.Common.Threading;

namespace ArcAnnihilation.Units
{
    public class Necronomicon : UnitBase
    {
        public Unit Necr;

        public Necronomicon(Unit necr)
        {
            Necr = necr;
        }

        public Ability ManaBurn { get; set; }

        public override void InitAbilities()
        {
            ManaBurn = Necr.Spellbook.Spell1;
            Printer.Print("init abilities: ManaBurn:" + ManaBurn.Name);
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
                Necr.Move(target.Position);
            /*else
                Hero.Move(Game.MousePosition);*/
        }

        public override IEnumerable<Item> GetItems()
        {
            throw new NotImplementedException();
        }
    }

    public class RangeNecr : Necronomicon
    {
        public RangeNecr(Unit necr) : base(necr)
        {
            AbilitiesBehaviour = new CanUseAbilitiesNecroArcher();
            OrbwalkingBehaviour= new CanUseOrbwalking();
        }

        public override async Task Combo(CancellationToken cancellationToken)
        {
            await TargetFinder(cancellationToken);
            await UseAbilities(cancellationToken);
        }
    }

    class MeleeNecr : Necronomicon
    {
        public MeleeNecr(Unit necr) : base(necr)
        {
            OrbwalkingBehaviour = new CanUseOrbwalkingOnlyForPushing();
        }
        public override async Task Combo(CancellationToken cancellationToken)
        {
            await TargetFinder(cancellationToken);
            await Attack(cancellationToken);
        }

        private async Task Attack(CancellationToken cancellationToken)
        {
            if (Orbwalker.CanAttack(Core.Target))
                Necr.Attack(Core.Target);
            await Await.Delay(250,cancellationToken);
        }
    }
}