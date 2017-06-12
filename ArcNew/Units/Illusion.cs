using System.Collections.Generic;
using ArcAnnihilation.Units.behaviour.Orbwalking;
using Ensage;

namespace ArcAnnihilation.Units
{
    public class Illusion : UnitBase
    {
        public Illusion(Hero s)
        {
            Hero = s;
            OrbwalkingBehaviour = new CanUseOrbwalking();
            CooldownOnMoving = 0.05f;
        }
        public bool IsValid => Hero != null && Hero.IsValid;
        /*public override void Init()
        {
            Orbwalker = Orbwalker.GetNewOrbwalker(this);
            Orbwalker.Load();
            Printer.Both($"[Illusion][init] -> {Hero.GetRealName()} [{Hero.Handle}]");
            CooldownOnMoving = 0.05f;
        }*/

        public override void InitAbilities()
        {
            
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
        }

        public override IEnumerable<Item> GetItems()
        {
            throw new System.NotImplementedException();
        }
    }
}