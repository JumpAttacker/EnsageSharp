using Ensage;
using Ensage.Heroes;
// ReSharper disable MemberCanBePrivate.Global

namespace MeepoAnnihilation
{
    internal class MeepoSettings
    {
        public Meepo Hero { get; set; }
        public uint Handle { get; set; }
        public bool MainMenu { get; set; }

        public bool IsAlive
        {
            get { return Hero.IsAlive; }
        }
        public Program.OrderState CurrentOrderState
        {
            get
            {
                var handle = Hero.Handle;
                return Program.OrderStates[handle];
            }
            set {}
        }

        public Ability SpellQ
        {
            get { return Program.SpellQ[Hero.Handle]; }
        }
        public Ability SpellW
        {
            get { return Program.SpellW[Hero.Handle]; }
        }

        public int Id { get; set; }
    
        public MeepoSettings(Meepo meepo)
        {
            Hero = meepo;
            Handle = meepo.Handle;
            MainMenu = false;
            CurrentOrderState = Program.OrderState.Idle;
            Id = (byte) (Program.MeepoSet.Count+1);
            Game.PrintMessage("Init new Meepo: "+string.Format("Menu: {0}; CurrentOderState: {1}; Id:{2} ;",MainMenu,CurrentOrderState,Id),MessageType.ChatMessage);
        }
    }
}
