using System;
using Ensage;
using Ensage.Abilities;
using Ensage.Heroes;
// ReSharper disable MemberCanBePrivate.Global

namespace MeepoAnnihilation
{
    internal class MeepoSettings
    {
        public Meepo Hero { get; set; }
        public uint Handle { get; set; }
        public bool MainMenu { get; set; }

        public bool IsAlive => Hero.IsAlive;

        public Program.OrderState CurrentOrderState
        {
            get { return Program.OrderStates[Handle]; }
            set {}
        }

        public Ability SpellQ => Program.SpellQ[Hero.Handle];

        public Ability SpellW => Program.SpellW[Hero.Handle];

        public int Id { get; set; }

        public float PoofStartTime;
    
        public MeepoSettings(Meepo meepo)
        {
            Hero = meepo;
            Handle = meepo.Handle;
            MainMenu = false;
            CurrentOrderState = Program.OrderState.Idle;
            var dividedWeStand = Hero.Spellbook.SpellR as DividedWeStand;
            if (dividedWeStand == null)
            {
                Console.WriteLine("[MeepoAnnihilation][InitNewMeepo] - cant find ultimate!");
                return;
            }
            Id = dividedWeStand.UnitIndex;
            Game.PrintMessage($"Init new Meepo: (Id: {Id})");
            PoofStartTime = float.MaxValue;
        }
    }
}
