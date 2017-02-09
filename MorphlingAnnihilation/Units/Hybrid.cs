using Ensage;
using MorphlingAnnihilation.Interface;

namespace MorphlingAnnihilation.Units
{
    public class Hybrid : ExtendedUnit
    {
        public Hero SelectedTarget;
        public Hybrid(Hero me) : base(me)
        {
        }
    }
}